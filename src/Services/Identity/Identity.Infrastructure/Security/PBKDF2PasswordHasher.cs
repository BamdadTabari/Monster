using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions;

namespace Identity.Infrastructure.Security;

/// <summary>
/// PBKDF2 (HMACSHA256, 100k) packed as: v1.{iterations}.{salt}.{hash} (base64url)
/// </summary>
public sealed class PBKDF2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize  = 32;

    public string Hash(string password)
    {
        // salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // derive
        byte[] pwd = Encoding.UTF8.GetBytes(password);
        try
        {
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password: pwd,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: KeySize);

            return $"v1.{Iterations}.{B64(salt)}.{B64(key)}";
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pwd);
        }
    }

    public bool Verify(string password, string packedHash)
    {
        var parts = packedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != "v1") return false;

        if (!int.TryParse(parts[1], out var iter) || iter <= 0) return false;
        var salt = UB64(parts[2]);
        var expected = UB64(parts[3]);

        byte[] pwd = Encoding.UTF8.GetBytes(password);
        byte[] actual = Array.Empty<byte>();
        try
        {
            actual = Rfc2898DeriveBytes.Pbkdf2(
                password: pwd,
                salt: salt,
                iterations: iter,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: expected.Length);

            var ok = CryptographicOperations.FixedTimeEquals(actual, expected);
            return ok;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pwd);
            if (actual.Length > 0) CryptographicOperations.ZeroMemory(actual);
        }
    }

    private static string B64(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');

    private static byte[] UB64(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return Convert.FromBase64String(s);
    }
}
