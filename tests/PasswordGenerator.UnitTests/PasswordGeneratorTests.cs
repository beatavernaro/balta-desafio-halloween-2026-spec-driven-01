namespace PasswordGenerator.UnitTests;

public class PasswordGeneratorTests
{
    // Generation is random, so each property is checked over many generated passwords.
    private const int SampleSize = 1000;

    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%&*";

    [Fact]
    public void Generate_ReturnsPasswordWithExactly16Characters()
    {
        foreach (var password in GenerateSample())
        {
            Assert.Equal(16, password.Length);
        }
    }

    [Theory]
    [InlineData(UppercaseLetters)]
    [InlineData(LowercaseLetters)]
    [InlineData(Digits)]
    [InlineData(Symbols)]
    public void Generate_ReturnsPasswordContainingRequiredCategory(string category)
    {
        foreach (var password in GenerateSample())
        {
            Assert.Contains(password, character => category.Contains(character));
        }
    }

    private static IEnumerable<string> GenerateSample() =>
        Enumerable.Range(0, SampleSize).Select(_ => Api.Services.PasswordGenerator.Generate());
}
