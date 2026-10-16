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

        return new string(characters);
    }

    public static bool IsValid(string password) => throw new NotImplementedException();

    // GetInt32 draws uniformly within the range, avoiding the bias of applying modulo to raw random bytes.
    private static char PickRandomCharacter(string source) =>
        source[RandomNumberGenerator.GetInt32(source.Length)];
}
