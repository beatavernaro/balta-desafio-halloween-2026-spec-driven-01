namespace PasswordGenerator.UnitTests;

public class PasswordGeneratorTests
{
    // Generation is random, so each property is checked over many generated passwords.
    private const int SampleSize = 1000;

    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%&*";
    private const string AllowedCharacters = UppercaseLetters + LowercaseLetters + Digits + Symbols;

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

    [Fact]
    public void Generate_ReturnsPasswordWithOnlyAllowedCharacters()
    {
        foreach (var password in GenerateSample())
        {
            Assert.All(password, character => Assert.Contains(character, AllowedCharacters));
        }
    }

    [Fact]
    public void Generate_ReturnsPasswordWithoutWhitespace()
    {
        foreach (var password in GenerateSample())
        {
            Assert.DoesNotContain(password, char.IsWhiteSpace);
        }
    }

    [Fact]
    public void Generate_ReturnsPasswordAcceptedByValidation()
    {
        foreach (var password in GenerateSample())
        {
            Assert.True(Api.Services.PasswordGenerator.IsValid(password), "Generated password failed validation.");
        }
    }

    // CB-01
    [Fact]
    public void IsValid_AcceptsPasswordWith16CharactersAndValidComposition()
    {
        Assert.True(Api.Services.PasswordGenerator.IsValid("Ab3!Cd4@Ef5#Gh6$"));
    }

    // CB-02
    [Theory]
    [InlineData("")]
    [InlineData("Ab3!Cd4@Ef5#Gh6")]
    [InlineData("Ab3!Cd4@Ef5#Gh6$x")]
    public void IsValid_RejectsPasswordWithLengthOtherThan16(string password)
    {
        Assert.False(Api.Services.PasswordGenerator.IsValid(password));
    }

    [Theory]
    [InlineData("ab3!cd4@ef5#gh6$")] // CB-03: no uppercase letter
    [InlineData("AB3!CD4@EF5#GH6$")] // CB-04: no lowercase letter
    [InlineData("Abc!Cde@Efg#Ghi$")] // CB-05: no digit
    [InlineData("Ab3xCd4yEf5zGh6w")] // CB-06: no allowed symbol
    public void IsValid_RejectsPasswordMissingRequiredCategory(string password)
    {
        Assert.False(Api.Services.PasswordGenerator.IsValid(password));
    }

    // CB-07
    [Theory]
    [InlineData("Ab3!Cd4@Ef5#Gh6 ")]
    [InlineData("Ab3!Cd4@\tf5#Gh6$")]
    public void IsValid_RejectsPasswordWithWhitespace(string password)
    {
        Assert.False(Api.Services.PasswordGenerator.IsValid(password));
    }

    // CB-08
    [Theory]
    [InlineData("Ab3!Cd4@Ef5#Gh6^")]
    [InlineData("Ab3!Cd4@Ef5#Gh6-")]
    [InlineData("Áb3!Cd4@Ef5#Gh6$")]
    public void IsValid_RejectsPasswordWithCharacterOutsideAllowedSet(string password)
    {
        Assert.False(Api.Services.PasswordGenerator.IsValid(password));
    }

    // CB-09
    [Theory]
    [InlineData("Aaaaaaaaaaaaaa1!")]
    [InlineData("Abcdefghijk1234!")]
    public void IsValid_AcceptsRepeatedOrSequentialCharactersWithValidComposition(string password)
    {
        Assert.True(Api.Services.PasswordGenerator.IsValid(password));
    }

    private static IEnumerable<string> GenerateSample() =>
        Enumerable.Range(0, SampleSize).Select(_ => Api.Services.PasswordGenerator.Generate());
}
