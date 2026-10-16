using System.Security.Cryptography;

namespace PasswordGenerator.Api.Services;

public static class PasswordGenerator
{
    public const int PasswordLength = 16;

    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%&*";
    private const string AllowedCharacters = UppercaseLetters + LowercaseLetters + Digits + Symbols;

    private static readonly string[] _requiredCategories = [UppercaseLetters, LowercaseLetters, Digits, Symbols];

    public static string Generate()
    {
        var characters = new char[PasswordLength];

        for (var index = 0; index < _requiredCategories.Length; index++)
        {
            characters[index] = PickRandomCharacter(_requiredCategories[index]);
        }

        for (var index = _requiredCategories.Length; index < PasswordLength; index++)
        {
            characters[index] = PickRandomCharacter(AllowedCharacters);
        }

        // Without shuffling, the required categories would always occupy the first positions.
        Shuffle(characters);

        var password = new string(characters);

        // Safeguard required by the plan: a password is only returned after its composition is verified.
        if (!IsValid(password))
        {
            throw new InvalidOperationException("Generated password does not satisfy the composition rules.");
        }

        return password;
    }

    public static bool IsValid(string password) =>
        password.Length == PasswordLength
        && password.All(character => AllowedCharacters.Contains(character))
        && _requiredCategories.All(category => password.Any(character => category.Contains(character)));

    // Fisher–Yates: each position swaps with a uniformly chosen position at or before it.
    private static void Shuffle(char[] characters)
    {
        for (var index = characters.Length - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
        }
    }

    // GetInt32 draws uniformly within the range, avoiding the bias of applying modulo to raw random bytes.
    private static char PickRandomCharacter(string source) =>
        source[RandomNumberGenerator.GetInt32(source.Length)];
}
