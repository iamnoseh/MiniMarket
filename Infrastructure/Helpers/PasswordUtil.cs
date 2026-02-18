using System.Security.Cryptography;

namespace Infrastructure.Helpers;

public static class PasswordUtil
{
    public static string GenerateRandomPassword(int length = 12)
    {
        if (length < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 4 characters.");
        }

        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string numericChars = "0123456789";
        const string specialChars = "-!?@#$%^&*";

        var allChars = upperChars + lowerChars + numericChars + specialChars;
        var chars = new List<char>(length)
        {
            upperChars[RandomNumberGenerator.GetInt32(upperChars.Length)],
            lowerChars[RandomNumberGenerator.GetInt32(lowerChars.Length)],
            numericChars[RandomNumberGenerator.GetInt32(numericChars.Length)],
            specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)]
        };

        for (var i = chars.Count; i < length; i++)
        {
            chars.Add(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
        }

        for (var i = chars.Count - 1; i > 0; i--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[swapIndex]) = (chars[swapIndex], chars[i]);
        }

        return new string(chars.ToArray());
    }
}