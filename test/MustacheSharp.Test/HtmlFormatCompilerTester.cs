using Xunit;

namespace Mustache.Test
{
    public class HtmlFormatCompilerTester
    {
        [Fact]
        public void ShouldEscapeValueContainingHTMLCharacters()
        {
            HtmlFormatCompiler compiler = new HtmlFormatCompiler();
            var generator = compiler.Compile("<html><body>Hello, {{Name}}!!!</body></html>");
            string html = generator.Render(new
            {
                Name = "John \"The Man\" Standford"
            });
            Assert.Equal("<html><body>Hello, John &quot;The Man&quot; Standford!!!</body></html>", html);
        }

        [Fact]
        public void ShouldIgnoreHTMLCharactersInsideTripleCurlyBraces()
        {
            HtmlFormatCompiler compiler = new HtmlFormatCompiler();
            var generator = compiler.Compile("<html><body>Hello, {{{Name}}}!!!</body></html>");
            string html = generator.Render(new
            {
                Name = "John \"The Man\" Standford"
            });
            Assert.Equal("<html><body>Hello, John \"The Man\" Standford!!!</body></html>", html);
        }
    }
}
