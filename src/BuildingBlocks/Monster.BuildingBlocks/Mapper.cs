using Serilog;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Monster.BuildingBlocks;

/// <summary>
/// Cycle-safe object mapper with collections, dictionary/set support,
/// culture-invariant conversions, and small reflection caches.
/// NOTE: Requires parameterless ctor for destination types (we can add constructor-binding later if needed).
/// </summary>
public static class Mapper
{
    public static TDestination Map<TSource, TDestination>(TSource source)
        where TDestination : class, new()
        where TSource : class
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var ctx = new MappingContext();
        return (TDestination)MapObject(typeof(TSource), typeof(TDestination), source, ctx)!;
    }

    // --- Core ---
    private static object? MapObject(Type sourceType, Type destType, object? source, MappingContext ctx)
    {
        if (source is null) return HandleNullForDestination(destType);

        if (IsSimpleType(sourceType) && IsSimpleType(destType))
            return ConvertIfNeeded(source, sourceType, destType);

        if (destType.IsAssignableFrom(sourceType)) return source;
        if (IsNullableMatch(destType, sourceType)) return source;

        if (IsCollectionType(sourceType) && IsCollectionType(destType) && source is IEnumerable srcEnum)
            return MapCollection(srcEnum, sourceType, destType, ctx);

        var key = (Source: source, DestType: destType);
        if (ctx.Cache.TryGetValue(key, out var existing)) return existing;

        var destination = CreateInstance(destType);
        ctx.Cache[key] = destination;

        MapProperties(sourceType, destType, source, destination, ctx);
        return destination;
    }

    private static void MapProperties(Type sourceType, Type destType, object source, object destination, MappingContext ctx)
    {
        var sourceProps = GetReadableProps(sourceType);
        var destProps = GetWritableProps(destType);

        foreach (var sProp in sourceProps)
        {
            if (!destProps.TryGetValue(sProp.Name, out var dProp)) continue;

            object? sValue;
            try { sValue = sProp.GetValue(source); }
            catch (Exception ex) { Log.Error(ex, "Error reading property {PropertyName}", sProp.Name); continue; }

            try
            {
                if (sValue is null)
                {
                    if (IsNullableType(dProp.PropertyType) || !dProp.PropertyType.IsValueType)
                        dProp.SetValue(destination, null);
                    else
                        dProp.SetValue(destination, Activator.CreateInstance(dProp.PropertyType));
                    continue;
                }

                var sType = sProp.PropertyType;
                var dType = dProp.PropertyType;

                if (dType.IsAssignableFrom(sType) || IsNullableMatch(dType, sType))
                { dProp.SetValue(destination, sValue); continue; }

                if (IsCollectionType(sType) && IsCollectionType(dType) && sValue is IEnumerable sEnum)
                { TrySetValue(dProp, destination, MapCollection(sEnum, sType, dType, ctx)); continue; }

                if (IsComplexType(sType) && IsComplexType(dType))
                { TrySetValue(dProp, destination, MapObject(sType, dType, sValue, ctx)); continue; }

                TrySetValue(dProp, destination, ConvertIfNeeded(sValue, sType, dType));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error mapping property {PropertyName}", sProp.Name);
                throw;
            }
        }
    }

    // --- Collections / Dictionaries / Sets ---
    private static object? MapCollection(IEnumerable sourceEnum, Type sourceCollectionType, Type destCollectionType, MappingContext ctx)
    {
        if (IsDictionary(sourceCollectionType) && IsDictionary(destCollectionType))
        {
            var dArgs = destCollectionType.GetGenericArguments(); // [TKey, TValue]
            var dDict = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(dArgs))!;
            var add = dDict.GetType().GetMethod("Add")!;
            foreach (var kv in sourceEnum)
            {
                var k = kv.GetType().GetProperty("Key")!.GetValue(kv);
                var v = kv.GetType().GetProperty("Value")!.GetValue(kv);
                var mk = MapObject(k!.GetType(), dArgs[0], k, ctx);
                var mv = MapObject(v?.GetType() ?? dArgs[1], dArgs[1], v, ctx);
                add.Invoke(dDict, new[] { mk, mv });
            }
            return dDict;
        }

        var sElem = GetCollectionElementType(sourceCollectionType);
        var dElem = GetCollectionElementType(destCollectionType);
        if (sElem is null || dElem is null) return CreateEmptyCompatibleCollection(destCollectionType, dElem);

        var tempList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dElem))!;
        foreach (var item in sourceEnum)
        {
            if (item is null) { tempList.Add(null); continue; }
            if (dElem.IsAssignableFrom(item.GetType())) tempList.Add(item);
            else tempList.Add(MapObject(item.GetType(), dElem, item, ctx));
        }

        if (destCollectionType.IsArray)
        {
            var arr = Array.CreateInstance(dElem, tempList.Count);
            for (int i = 0; i < tempList.Count; i++) arr.SetValue(tempList[i], i);
            return arr;
        }

        if (IsSet(destCollectionType))
        {
            var set = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(dElem))!;
            var add = set.GetType().GetMethod("Add")!;
            foreach (var x in tempList) add.Invoke(set, new[] { x });
            return set;
        }

        var dColl = TryCreateGenericCollection(destCollectionType, dElem);
        if (dColl != null)
        {
            var add = dColl.GetType().GetMethod("Add");
            foreach (var x in tempList) add!.Invoke(dColl, new[] { x });
            return dColl;
        }

        if (destCollectionType.IsAssignableFrom(tempList.GetType())) return tempList;
        return tempList;
    }

    // --- Helpers ---
    private static bool IsComplexType(Type type) => type.IsClass && type != typeof(string);

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsPrimitive || type.IsEnum) return true;
        return type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid);
    }

    private static object? ConvertIfNeeded(object value, Type sourceType, Type destType)
    {
        var underlying = Nullable.GetUnderlyingType(destType);
        if (underlying != null) destType = underlying;
        if (destType.IsAssignableFrom(value.GetType())) return value;

        if (destType.IsEnum)
        {
            if (value is string s) return Enum.Parse(destType, s, true);
            return Enum.ToObject(destType, System.Convert.ChangeType(value, Enum.GetUnderlyingType(destType), CultureInfo.InvariantCulture)!);
        }

        if (destType == typeof(Guid) && value is string sg && Guid.TryParse(sg, out var g)) return g;
        if (destType == typeof(DateTime) && value is string sd && DateTime.TryParse(sd, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)) return dt;
        if (destType == typeof(DateTimeOffset) && value is string sdo && DateTimeOffset.TryParse(sdo, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto)) return dto;

        try { return System.Convert.ChangeType(value, destType, CultureInfo.InvariantCulture); }
        catch { return value; }
    }

    private static bool IsNullableMatch(Type t1, Type t2) =>
        (IsNullableType(t1) && Nullable.GetUnderlyingType(t1) == t2) ||
        (IsNullableType(t2) && Nullable.GetUnderlyingType(t2) == t1);

    private static bool IsNullableType(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    private static bool IsCollectionType(Type t) => typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string);
    private static bool IsDictionary(Type t) => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    private static bool IsSet(Type t) => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

    private static Type? GetCollectionElementType(Type t)
    {
        if (t.IsArray) return t.GetElementType();
        if (t.IsGenericType)
        {
            foreach (var i in t.GetInterfaces().Concat(new[] { t }))
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return i.GetGenericArguments()[0];
        }
        return null;
    }

    private static object CreateInstance(Type t)
    {
        var ctor = t.GetConstructor(Type.EmptyTypes);
        if (ctor == null) throw new InvalidOperationException($"Type {t.FullName} has no parameterless constructor.");
        return ctor.Invoke(null);
    }

    private static object? HandleNullForDestination(Type destType)
    {
        if (!destType.IsValueType || IsNullableType(destType)) return null;
        return Activator.CreateInstance(destType);
    }

    private static void TrySetValue(PropertyInfo prop, object target, object? value)
    {
        try
        {
            if (value == null && prop.PropertyType.IsValueType && !IsNullableType(prop.PropertyType))
                value = Activator.CreateInstance(prop.PropertyType);
            prop.SetValue(target, value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SetValue error for {PropertyName}", prop.Name);
            throw;
        }
    }

    // reflection caches
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _readableProps = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _writableProps = new();

    private static PropertyInfo[] GetReadableProps(Type t) =>
        _readableProps.GetOrAdd(t, _ =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToArray());

    private static Dictionary<string, PropertyInfo> GetWritableProps(Type t) =>
        _writableProps.GetOrAdd(t, _ =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
             .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase));

    private sealed class MappingContext
    {
        public Dictionary<(object Source, Type DestType), object> Cache { get; } =
            new(new SourceAndDestTypeComparer());
    }

    private sealed class SourceAndDestTypeComparer : IEqualityComparer<(object Source, Type DestType)>
    {
        public bool Equals((object Source, Type DestType) x, (object Source, Type DestType) y) =>
            ReferenceEquals(x.Source, y.Source) && x.DestType == y.DestType;

        public int GetHashCode((object Source, Type DestType) obj) =>
            (RuntimeHelpers.GetHashCode(obj.Source) * 397) ^ obj.DestType.GetHashCode();
    }

    // Creates an empty collection compatible with the destination type
// e.g. T[] -> empty T[], ICollection<T>/IList<T>/IEnumerable<T> -> new List<T>()
private static object? CreateEmptyCompatibleCollection(Type destCollectionType, Type? elementType)
{
    if (elementType == null) return null;

    // If destination is an array
    if (destCollectionType.IsArray)
        return Array.CreateInstance(elementType, 0);

    // If we can construct a compatible generic collection
    var dColl = TryCreateGenericCollection(destCollectionType, elementType);
    if (dColl != null) return dColl;

    // Fallback: List<T>
    return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
}

// Tries to create an instance of the destination collection type that can accept T
// - Interfaces (IEnumerable<T>/ICollection<T>/IList<T>) -> List<T>
// - Concrete types with parameterless ctor and Add(T) -> new instance
private static object? TryCreateGenericCollection(Type collectionType, Type? elementType)
{
    if (elementType == null) return null;

    // Interface targets -> List<T>
    if (collectionType.IsInterface)
    {
        if (collectionType.IsGenericType)
        {
            var def = collectionType.GetGenericTypeDefinition();
            if (def == typeof(IEnumerable<>) || def == typeof(ICollection<>) || def == typeof(IList<>))
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
        }
        // any other IEnumerable<T>-like interface -> List<T>
        if (collectionType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
    }

    // Concrete, constructible type with Add(T)
    if (!collectionType.IsAbstract && collectionType.GetConstructor(Type.EmptyTypes) != null)
    {
        var instance = Activator.CreateInstance(collectionType);
        var add = collectionType.GetMethod("Add", new[] { elementType });
        // if Add(T) exists, we can use it
        if (add != null) return instance;

        // Some collections declare Add(object) or Add with assignable param
        var anyAdd = collectionType.GetMethods()
            .FirstOrDefault(m => m.Name == "Add" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.IsAssignableFrom(elementType));
        if (anyAdd != null) return instance;
    }

    return null;
}

}
