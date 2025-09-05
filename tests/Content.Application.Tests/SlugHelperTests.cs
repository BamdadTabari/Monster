using FluentAssertions;
using Monster.BuildingBlocks.Text;

namespace Content.Application.Tests;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("C# & .NET", "c-net")]
    [InlineData("  Multiple   spaces  ", "multiple-spaces")]
    [InlineData("آزمایش اسلاگ", "ازمایش-اسلاگ")] // adjust if you want custom Persian handling
    public void ToSlug_should_generate_expected_slugs(string input, string expected)
    {
        SlugHelper.ToSlug(input).Should().Be(expected);
    }
}
