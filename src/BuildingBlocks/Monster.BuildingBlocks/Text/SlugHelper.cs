using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Monster.BuildingBlocks.Text;

/// <summary>Deterministic, culture-invariant slug generator.</summary>
public static class SlugHelper
{
    private static readonly Regex Hyphens = new("-{2,}", RegexOptions.Compiled);

    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;

            var c = char.ToLowerInvariant(ch);

            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (char.IsWhiteSpace(c) || c == '-' || c == '_') sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        return Hyphens.Replace(slug, "-").Trim('-');
    }
}
