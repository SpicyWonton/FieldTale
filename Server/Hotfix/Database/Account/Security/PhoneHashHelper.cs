using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Fantasy.Database.Account.Security;

public static partial class PhoneHashHelper
{
    [GeneratedRegex("^1[3-9]\\d{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex ChinaMobilePattern();

    public static string Hash(string phoneNumber, byte[] key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length < 32)
        {
            throw new ArgumentException("The phone hash key must contain at least 32 bytes.", nameof(key));
        }

        var normalizedPhoneNumber = phoneNumber.Trim();
        if (!ChinaMobilePattern().IsMatch(normalizedPhoneNumber))
        {
            throw new ArgumentException("Phone number must be an 11-digit Chinese mainland mobile number.", nameof(phoneNumber));
        }

        var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(normalizedPhoneNumber));
        return Convert.ToHexString(hash);
    }
}
