using System.Security.Cryptography;
using Identity.Application.Abstractions;

namespace Identity.Infrastructure.Security;

public sealed class PBKDF2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000, SaltSize = 16, KeySize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        return $"v1.{Iterations}.{B64(salt)}.{B64(key)}";
    }

    public bool Verify(string password, string packedHash)
    {
        var parts = packedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != "v1") return false;
        var iter = int.Parse(parts[1]);
        var salt = UB64(parts[2]);
        var expected = UB64(parts[3]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
        var actual = pbkdf2.GetBytes(expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static string B64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
    private static byte[] UB64(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return Convert.FromBase64String(s);
    }
}
