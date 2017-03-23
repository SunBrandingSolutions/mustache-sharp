using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Xunit;

namespace Mustache.Test
{
    /// <summary>
    /// Tests the FormatParser class.
    /// </summary>
    public class FormatCompilerTester
    {
        #region Tagless Formats

        /// <summary>
        /// If the given format is null, an exception should be thrown.
        /// </summary>
        [Fact]
        public void TestCompile_NullFormat_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                FormatCompiler compiler = new FormatCompiler();
                compiler.Compile(null);
            });
        }

        /// <summary>
        /// If the format string contains no tag, then the given format string
        /// should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_NoTags_PrintsFormatString()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "This is an ordinary string.";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal(format, result);
        }

        /// <summary>
        /// If a line is just whitespace, it should be printed out as is.
        /// </summary>
        [Fact]
        public void TestCompile_LineAllWhitespace_PrintsWhitespace()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "\t    \t";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal(format, result);
        }

        /// <summary>
        /// If a line has output, then the next line is blank, then both lines
        /// should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_OutputNewLineBlank_PrintsBothLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello{{#newline}}
    ";

            const string expected = @"Hello
    ";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal(expected, result);
        }

        #endregion

        #region Key

        /// <summary>
        /// Replaces placeholds with the actual value.
        /// </summary>
        [Fact]
        public void TestCompile_Key_ReplacesWithValue()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { Name = "Bob" });
            Assert.Equal("Hello, Bob!!!", result);
        }

        /// <summary>
        /// If we pass null as the source object and the format string contains "this",
        /// then nothing should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ThisIsNull_PrintsNothing()
        {
            FormatCompiler compiler = new FormatCompiler();
            Generator generator = compiler.Compile("{{this}}");
            string result = generator.Render(null);
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// If we try to print a key that doesn't exist, an exception should be thrown.
        /// </summary>
        [Fact]
        public void TestCompile_MissingKey_ReturnsOriginal()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name}}!!!";
            Generator generator = compiler.Compile(format);
            var actual = generator.Render(new object());
            var expected = format;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// If we try to print a key that doesn't exist, we can provide a
        /// handler to provide a substitute.
        /// </summary>
        [Fact]
        public void TestCompile_MissingKey_CallsKeyNotFoundHandler()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name}}!!!";
            Generator generator = compiler.Compile(format);
            generator.KeyNotFound += (obj, args) =>
            {
                args.Substitute = "Unknown";
                args.Handled = true;
            };
            string actual = generator.Render(new object());
            string expected = "Hello, Unknown!!!";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// If the key is the parent object, the search will go up the hierarchy.
        /// </summary>
        [Fact]
        public void TestCompile_KeyInParent_LooksUpKeyInParent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#with Address}}{{FirstName}} from {{City}}{{/with}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(new
            {
                FirstName = "Bob",
                Address = new
                {
                    City = "Philadelphia",
                }
            });
            string expected = "Bob from Philadelphia";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// If part of a key is wrong, the full details should be provided.
        /// </summary>
        [Fact]
        public void TestCompile_MultipartKey_PartMissing_ProvidesFullDetail()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{Customer.Name}}";
            Generator generator = compiler.Compile(format);
            generator.KeyNotFound += (obj, args) =>
            {
                args.Substitute = args.Key + "," + args.MissingMember;
                args.Handled = true;
            };
            string actual = generator.Render(new
            {
                Customer = new
                {
                    FirstName = "Bob"
                }
            });
            string expected = "Customer.Name,Name";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// If we specify an alignment with a key, the alignment should
        /// be used when rending the value.
        /// </summary>
        [Fact]
        public void TestCompile_KeyWithNegativeAlignment_AppliesAlignment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name,-10}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { Name = "Bob" });
            Assert.Equal("Hello, Bob       !!!", result);
        }

        /// <summary>
        /// If we specify an alignment with a key, the alignment should
        /// be used when rending the value.
        /// </summary>
        [Fact]
        public void TestCompile_KeyWithPositiveAlignment_AppliesAlignment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name,10}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { Name = "Bob" });
            Assert.Equal("Hello,        Bob!!!", result);
        }

        /// <summary>
        /// If we specify a positive alignment with a key with an optional + character, 
        /// the alignment should be used when rending the value.
        /// </summary>
        [Fact]
        public void TestCompile_KeyWithPositiveAlignment_OptionalPlus_AppliesAlignment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Name,+10}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { Name = "Bob" });
            Assert.Equal("Hello,        Bob!!!", result);
        }

        /// <summary>
        /// If we specify an format with a key, the format should
        /// be used when rending the value.
        /// </summary>
        [Fact]
        public void TestCompile_KeyWithFormat_AppliesFormatting()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{When:yyyyMMdd}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { When = new DateTime(2012, 01, 31) });
            Assert.Equal("Hello, 20120131!!!", result);
        }

        /// <summary>
        /// If we specify an alignment with a key, the alignment should
        /// be used when rending the value.
        /// </summary>
        [Fact]
        public void TestCompile_KeyWithAlignmentAndFormat_AppliesBoth()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{When,10:yyyyMMdd}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { When = new DateTime(2012, 01, 31) });
            Assert.Equal("Hello,   20120131!!!", result);
        }

        /// <summary>
        /// If we dot separate keys, the value will be found by searching
        /// through the properties.
        /// </summary>
        [Fact]
        public void TestCompile_NestedKeys_NestedProperties()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Top.Middle.Bottom}}!!!";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new { Top = new { Middle = new { Bottom = "Bob" } } });
            Assert.Equal("Hello, Bob!!!", result);
        }

        /// <summary>
        /// If a line has output, then the next line is blank, then both lines
        /// should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_OutputNewLineOutput_PrintsBothLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{this}}{{#newline}}
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render("Content");
            const string expected = @"Content
After";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If there is a line followed by a line with a key, both lines should be
        /// printed.
        /// </summary>
        [Fact]
        public void TestCompile_EmptyNewLineKey_PrintsBothLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#newline}}
{{this}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render("Content");
            const string expected = @"
Content";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If there is a no-output line followed by a line with a key, the first line
        /// should be removed.
        /// </summary>
        [Fact]
        public void TestCompile_NoOutputNewLineKey_PrintsBothLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#! comment }}
{{this}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render("Content");
            const string expected = @"Content";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If there is a comment on one line followed by a line with a key, the first line
        /// should be removed.
        /// </summary>
        [Fact]
        public void TestCompile_KeyKey_PrintsBothLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{this}}{{#newline}}
{{this}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render("Content");
            const string expected = @"Content
Content";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// We can track all of the keys that appear in a template by
        /// registering with the PlaceholderFound event.
        /// </summary>
        [Fact]
        public void TestCompile_FindsPlaceholders_RecordsPlaceholders()
        {
            FormatCompiler compiler = new FormatCompiler();
            HashSet<string> keys = new HashSet<string>();
            compiler.PlaceholderFound += (o, e) =>
            {
                keys.Add(e.Key);
            };
            compiler.Compile(@"{{FirstName}} {{LastName}}");
            string[] expected = new string[] { "FirstName", "LastName" };
            string[] actual = keys.OrderBy(s => s).ToArray();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We can track all of the keys that appear in a template by
        /// registering with the PlaceholderFound event.
        /// </summary>
        [Fact]
        public void TestCompile_FindsVariables_RecordsVariables()
        {
            FormatCompiler compiler = new FormatCompiler();
            HashSet<string> variables = new HashSet<string>();
            compiler.VariableFound += (o, e) =>
            {
                variables.Add(e.Name);
            };
            compiler.Compile(@"{{@FirstName}}{{@LastName}}");
            string[] expected = new string[] { "FirstName", "LastName" };
            string[] actual = variables.OrderBy(s => s).ToArray();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We can track all of the keys that appear in a template by
        /// registering with the PlaceholderFound event.
        /// </summary>
        [Fact]
        public void TestCompile_FindsPlaceholdersInIf_RecordsPlaceholders()
        {
            FormatCompiler compiler = new FormatCompiler();
            HashSet<string> keys = new HashSet<string>();
            compiler.PlaceholderFound += (o, e) =>
            {
                keys.Add(e.Key);
            };
            compiler.Compile(@"{{#if FirstName}}{{/if}}");
            string[] expected = new string[] { "FirstName" };
            string[] actual = keys.OrderBy(s => s).ToArray();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We can track all of the keys that appear in a template by
        /// registering with the PlaceholderFound event.
        /// </summary>
        [Fact]
        public void TestCompile_FindsVariablesInIf_RecordsVariables()
        {
            FormatCompiler compiler = new FormatCompiler();
            HashSet<string> variables = new HashSet<string>();
            compiler.VariableFound += (o, e) =>
            {
                variables.Add(e.Name);
            };
            compiler.Compile(@"{{#if @FirstName}}{{/if}}");
            string[] expected = new string[] { "FirstName" };
            string[] actual = variables.OrderBy(s => s).ToArray();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We can determine the context in which a placeholder is found by looking at the provided context array.
        /// </summary>
        [Fact]
        public void TestCompile_FindsPlaceholders_ProvidesContext()
        {
            FormatCompiler compiler = new FormatCompiler();
            Context[] context = null;
            compiler.PlaceholderFound += (o, e) =>
            {
                context = e.Context;
            };
            compiler.Compile(@"{{#with Address}}{{ZipCode}}{{/with}}");

            Assert.NotNull(context);
            Assert.Equal(2, context.Length);

            Assert.Equal(string.Empty, context[0].TagName);
            Assert.Equal("with", context[1].TagName);

            Assert.Equal(0, context[0].Parameters.Length);
            Assert.Equal(1, context[1].Parameters.Length);
            Assert.Equal("Address", context[1].Parameters[0].Argument);
        }

        /// <summary>
        /// I was leaving behind context even after reaching a closing tag. We need to make sure
        /// that context is like a call stack and that it is cleaned up after leaving the context.
        /// </summary>
        [Fact]
        public void TestCompile_ExitContext_RemoveContext()
        {
            FormatCompiler compiler = new FormatCompiler();
            Context[] context = null;
            compiler.PlaceholderFound += (o, e) =>
            {
                context = e.Context;
            };
            compiler.Compile(@"{{#with Address}}{{/with}}{{FirstName}}");

            Assert.NotNull(context);
            Assert.Equal(1, context.Length);

            Assert.Equal(string.Empty, context[0].TagName);
        }

        /// <summary>
        /// If a key refers to a public field, its value should be substituted in the output.
        /// </summary>
        [Fact]
        public void TestGenerate_KeyRefersToPublicField_SubstitutesValue()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Field}}!!!";
            Generator generator = compiler.Compile(format);
            ClassWithPublicField instance = new ClassWithPublicField() { Field = "Bob" };
            string result = generator.Render(instance);
            Assert.Equal("Hello, Bob!!!", result);
        }

        public class ClassWithPublicField
        {
            public string Field;
        }

        /// <summary>
        /// If a derived class replaces a property/field in the base class (via new)
        /// it should be used, instead of causing an exception or using the base's
        /// property/field.
        /// </summary>
        [Fact]
        public void TestGenerate_NewPropertyInDerivedClass_UsesDerivedProperty()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Value}}!!!";
            Generator generator = compiler.Compile(format);
            DerivedClass instance = new DerivedClass() { Value = "Derived" };
            string result = generator.Render(instance);
            Assert.Equal("Hello, Derived!!!", result);
        }

        public class BaseClass
        {
            public int Value { get; set; }
        }

        public class DerivedClass : BaseClass
        {
            public DerivedClass()
            {
                base.Value = 1;
            }

            public new string Value { get; set; }
        }

        /// <summary>
        /// If a derived class replaces a property/field in the base class (via new)
        /// it should be used, instead of causing an exception or using the base's
        /// property/field.
        /// </summary>
        [Fact]
        public void TestGenerate_NewPropertyInGenericDerivedClass_UsesDerivedProperty()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello, {{Value}}!!!";
            Generator generator = compiler.Compile(format);
            DerivedClass<string> instance = new DerivedClass<string>() { Value = "Derived" };
            string result = generator.Render(instance);
            Assert.Equal("Hello, Derived!!!", result);
        }

        public class DerivedClass<T> : BaseClass
        {
            public DerivedClass()
            {
                base.Value = 1;
            }

            public new T Value { get; set; }
        }

        #endregion

        #region Comment

        /// <summary>
        /// Removes comments from the middle of text.
        /// </summary>
        [Fact]
        public void TestCompile_Comment_RemovesComment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "Before{{#! This is a comment }}After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// Removes comments surrounding text.
        /// </summary>
        [Fact]
        public void TestCompile_CommentContentComment_RemovesComment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "{{#! comment }}Middle{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            Assert.Equal("Middle", result);
        }

        /// <summary>
        /// If blank space is surrounded by comments, the line should be removed.
        /// </summary>
        [Fact]
        public void TestCompile_CommentBlankComment_RemovesLine()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "{{#! comment }}    {{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            Assert.Equal("    ", result);
        }

        /// <summary>
        /// If a comment follows text, the comment should be removed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentComment_RemovesComment()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "Front{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            Assert.Equal("Front", result);
        }

        /// <summary>
        /// If a comment follows text, the comment should be removed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentCommentContentComment_RemovesComments()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = "Front{{#! comment }}Middle{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            Assert.Equal("FrontMiddle", result);
        }

        /// <summary>
        /// If a comment makes up the entire format string, the nothing should be printed out.
        /// </summary>
        [Fact]
        public void TestCompile_CommentAloneOnlyLine_PrintsSurroundingSpace()
        {
            FormatCompiler compiler = new FormatCompiler();
            Generator generator = compiler.Compile("    {{#! comment }}    ");
            string result = generator.Render(null);
            Assert.Equal("        ", result);
        }

        /// <summary>
        /// If a comment is on a line by itself, irrespective of leading or trailing whitespace,
        /// the line should be removed from output.
        /// </summary>
        [Fact]
        public void TestCompile_ContentNewLineCommentNewLineContent_RemovesLine()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Before
    {{#! This is a comment }}    
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            const string expected = @"Before        After";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If multiple comments are on a line by themselves, irrespective of whitespace,
        /// the line should be removed from output.
        /// </summary>
        [Fact]
        public void TestCompile_ContentNewLineCommentCommentNewLineContent_RemovesLine()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Before
    {{#! This is a comment }}    {{#! This is another comment }}    
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            const string expected = @"Before            After";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If comments are on a multiple lines by themselves, irrespective of whitespace,
        /// the lines should be removed from output.
        /// </summary>
        [Fact]
        public void TestCompile_CommentsOnMultipleLines_RemovesLines()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Before
    {{#! This is a comment }}    
    {{#! This is another comment }}    
    {{#newline}}
    {{#! This is the final comment }}
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            const string expected = @"Before                    
    After";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If a comment is followed by text, the line should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentNewLineCommentContentNewLineContent_PrintsLine()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Before
    {{#! This is a comment }}Extra
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(new object());
            const string expected = @"Before    ExtraAfter";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If a comment is followed by the last line in a format string,
        /// the comment line should be eliminated and the last line printed.
        /// </summary>
        [Fact]
        public void TestCompile_CommentNewLineBlank_PrintsBlank()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"    {{#! comment }}
        ";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("            ", result);
        }

        /// <summary>
        /// If a comment is followed by the last line in a format string,
        /// the comment line should be eliminated and the last line printed.
        /// </summary>
        [Fact]
        public void TestCompile_CommentNewLineContent_PrintsContent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"    {{#! comment }}
After";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("    After", result);
        }

        /// <summary>
        /// If a line with content is followed by a line with a comment, the first line should
        /// be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentNewLineComment_PrintsContent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"First
{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("First", result);
        }

        /// <summary>
        /// If a line has a comment, followed by line with content, followed by a line with a comment, only
        /// the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_CommentNewLineContentNewLineComment_PrintsContent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#! comment }}
First
{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("First", result);
        }

        /// <summary>
        /// If there are lines with content, then a comment, then content, then a comment, only
        /// the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentNewLineCommentNewLineContentNewLineComment_PrintsContent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"First{{#newline}}
{{#! comment }}
Middle
{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            const string expected = @"First
Middle";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If there is content and a comment on a line, followed by a comment,
        /// only the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentCommentNewLineComment_PrintsContent()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"First{{#! comment }}
{{#! comment }}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("First", result);
        }

        #endregion

        #region If

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_EvaluatesToFalse_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(false);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_null_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_DBNull_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(DBNull.Value);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_EmptyIEnumerable_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(Enumerable.Empty<int>());
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_NullChar_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render('\0');
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_ZeroInt_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(0);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_ZeroFloat_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(0f);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_ZeroDouble_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(0.0);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_ZeroDecimal_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(0m);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to false, the content of an if statement should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_If_EvaluatesToTrue_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Content{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("BeforeContentAfter", result);
        }

        /// <summary>
        /// If the header and footer appear on lines by themselves, they should not generate new lines.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineContentNewLineEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
Content
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("Content", result);
        }

        /// <summary>
        /// If the header and footer appear on lines by themselves, they should not generate new lines.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineEndIf_PrintsNothing()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// If the footer has content in front of it, the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineContentEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
Content{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("Content", result);
        }

        /// <summary>
        /// If the header has content after it, the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfContentNewLineEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}Content
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("Content", result);
        }

        /// <summary>
        /// If the header has content after it, the content should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_ContentIfNewLineEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"Content{{#if this}}
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("Content", result);
        }

        /// <summary>
        /// If the header and footer are adjacent, then there is no content.
        /// </summary>
        [Fact]
        public void TestCompile_IfEndIf_PrintsNothing()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// If the header and footer are adjacent, then there is no inner content.
        /// </summary>
        [Fact]
        public void TestCompile_ContentIfEndIf_PrintsNothing()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"Content{{#if this}}{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("Content", result);
        }

        /// <summary>
        /// If the header and footer are adjacent, then there is no inner content.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineCommentEndIf_PrintsNothing()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
{{#! comment}}{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// If the a header follows a footer, it shouldn't generate a new line.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineContentNewLineEndIfIfNewLineContentNewLineEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
First
{{/if}}{{#if this}}
{{#newline}}
Last
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            const string expected = @"First
Last";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If the content separates two if statements, it should be unaffected.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineEndIfNewLineContentNewLineIfNewLineEndIf_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
{{/if}}
Content
{{#if this}}
{{/if}}";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            const string expected = @"Content";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// If there is trailing text of any kind, the newline after content should be preserved.
        /// </summary>
        [Fact]
        public void TestCompile_IfNewLineEndIfNewLineContentNewLineIfNewLineEndIfContent_PrintsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = @"{{#if this}}
{{/if}}
First
{{#newline}}
{{#if this}}
{{/if}}
Last";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            const string expected = @"First
Last";
            Assert.Equal(expected, result);
        }

        #endregion

        #region If/Else

        /// <summary>
        /// If the condition evaluates to false, the content of an else statement should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElse_EvaluatesToFalse_PrintsElse()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Yay{{#else}}Nay{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(false);
            Assert.Equal("BeforeNayAfter", result);
        }

        /// <summary>
        /// If the condition evaluates to true, the content of an if statement should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElse_EvaluatesToTrue_PrintsIf()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if this}}Yay{{#else}}Nay{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(true);
            Assert.Equal("BeforeYayAfter", result);
        }

        /// <summary>
        /// Second else blocks will result in an exceptions being thrown.
        /// </summary>
        [Fact]
        public void TestCompile_IfElse_TwoElses_IncludesSecondElseInElse_Throws()
        {
            Assert.Throws<FormatException>(() =>
            {
                FormatCompiler parser = new FormatCompiler();
                const string format = "Before{{#if this}}Yay{{#else}}Nay{{#else}}Bad{{/if}}After";
                Generator generator = parser.Compile(format);
                string result = generator.Render(false);
                Assert.Equal("BeforeNay{{#else}}BadAfter", result);
            });
        }

        #endregion

        #region If/Elif/Else

        /// <summary>
        /// If the if statement evaluates to true, its block should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElifElse_IfTrue_PrintsIf()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if First}}First{{#elif Second}}Second{{#else}}Third{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { First = true, Second = true });
            Assert.Equal("BeforeFirstAfter", result);
        }

        /// <summary>
        /// If the elif statement evaluates to true, its block should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElifElse_ElifTrue_PrintsIf()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if First}}First{{#elif Second}}Second{{#else}}Third{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { First = false, Second = true });
            Assert.Equal("BeforeSecondAfter", result);
        }

        /// <summary>
        /// If the elif statement evaluates to false, the else block should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElifElse_ElifFalse_PrintsElse()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if First}}First{{#elif Second}}Second{{#else}}Third{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { First = false, Second = false });
            Assert.Equal("BeforeThirdAfter", result);
        }

        #endregion

        #region If/Elif

        /// <summary>
        /// If the elif statement evaluates to false and there is no else statement, nothing should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElif_ElifFalse_PrintsNothing()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if First}}First{{#elif Second}}Second{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { First = false, Second = false });
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If there are two elif statements and the first is false, the second elif block should be printed.
        /// </summary>
        [Fact]
        public void TestCompile_IfElifElif_ElifFalse_PrintsSecondElif()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#if First}}First{{#elif Second}}Second{{#elif Third}}Third{{/if}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { First = false, Second = false, Third = true });
            Assert.Equal("BeforeThirdAfter", result);
        }

        #endregion

        #region Each

        /// <summary>
        /// If we pass an empty collection to an each statement, the content should not be printed.
        /// </summary>
        [Fact]
        public void TestCompile_Each_EmptyCollection_SkipsContent()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#each this}}{{this}}{{/each}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new int[0]);
            Assert.Equal("BeforeAfter", result);
        }

        /// <summary>
        /// If we pass a populated collection to an each statement, the content should be printed
        /// for each item in the collection, using that item as the new scope context.
        /// </summary>
        [Fact]
        public void TestCompile_Each_PopulatedCollection_PrintsContentForEach()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#each this}}{{this}}{{/each}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new int[] { 1, 2, 3 });
            Assert.Equal("Before123After", result);
        }

        /// <summary>
        /// We can use the index tag to get the current iteration.
        /// </summary>
        [Fact]
        public void TestCompile_Each_Index_PrintsIndexOfItem()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "<ul>{{#each this}}<li value=\"{{this}}\">Item {{#index}}</li>{{/each}}</ul>";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new int[] { 1, 2, 3 });
            const string expected = @"<ul><li value=""1"">Item 0</li><li value=""2"">Item 1</li><li value=""3"">Item 2</li></ul>";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// A bug was found where the index tag was trying to read the arguments for the next tag.
        /// This was caused by the index tag chewing up more of the input than it was supposed to.
        /// </summary>
        [Fact]
        public void TestCompile_Each_ContextAfterIndexTag()
        {
            List<TestObject> objects = new List<TestObject>();
            objects.Add(new TestObject { Name = "name1", Val = "val1" });
            objects.Add(new TestObject { Name = "name2", Val = "val2" });
            objects.Add(new TestObject { Name = "name3", Val = "val3" });

            const string template = @"{{#each this}}
Item Number: {{#index}}<br />{{#newline}}
{{/each}}
{{#each this}}
Item Number: foo<br />{{#newline}}
{{/each}}";

            FormatCompiler compiler = new FormatCompiler();
            Generator generator = compiler.Compile(template);
            string actual = generator.Render(objects);

            const string expected = @"Item Number: 0<br />
Item Number: 1<br />
Item Number: 2<br />
Item Number: foo<br />
Item Number: foo<br />
Item Number: foo<br />
";

            Assert.Equal(expected, actual);
        }

        public class TestObject
        {
            public string Name { get; set; }

            public string Val { get; set; }
        }

        #endregion

        #region With

        /// <summary>
        /// The object replacing the placeholder should be used as the context of a with statement.
        /// </summary>
        [Fact]
        public void TestCompile_With_AddsScope()
        {
            FormatCompiler parser = new FormatCompiler();
            const string format = "Before{{#with Nested}}{{this}}{{/with}}After";
            Generator generator = parser.Compile(format);
            string result = generator.Render(new { Nested = "Hello" });
            Assert.Equal("BeforeHelloAfter", result);
        }

        #endregion

        #region Default Parameter

        /// <summary>
        /// If a tag is defined with a default parameter, the default value 
        /// should be returned if an argument is not provided.
        /// </summary>
        [Fact]
        public void TestCompile_MissingDefaultParameter_ProvidesDefault()
        {
            FormatCompiler compiler = new FormatCompiler();
            compiler.RegisterTag(new DefaultTagDefinition(), true);
            const string format = @"{{#default}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal("123", result);
        }

        private sealed class DefaultTagDefinition : InlineTagDefinition
        {
            public DefaultTagDefinition()
                : base("default")
            {
            }

            protected override IEnumerable<TagParameter> GetParameters()
            {
                return new TagParameter[] { new TagParameter("param") { IsRequired = false, DefaultValue = 123 } };
            }

            public override void GetText(TextWriter writer, Dictionary<string, object> arguments, Scope contextScope)
            {
                writer.Write(arguments["param"]);
            }
        }

        #endregion

        #region Compound Tags

        /// <summary>
        /// If a format contains multiple tags, they should be handled just fine.
        /// </summary>
        [Fact]
        public void TestCompile_MultipleTags()
        {
            var cultureUS = new CultureInfo("en-US");
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"Hello {{Customer.FirstName}}:
{{#newline}}
{{#newline}}
{{#with Order}}
{{#if LineItems}}
Below are your order details:
{{#newline}}
{{#newline}}
{{#each LineItems}}
    {{Name}}: {{UnitPrice:C}} x {{Quantity}}{{#newline}}
{{/each}}
{{#newline}}
Your order total was: {{Total:C}}
{{/if}}
{{/with}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(cultureUS, new
            {
                Customer = new { FirstName = "Bob" },
                Order = new
                {
                    Total = 7.50m,
                    LineItems = new object[] 
                    {
                        new { Name = "Banana", UnitPrice = 2.50m, Quantity = 1 },
                        new { Name = "Orange", UnitPrice = .50m, Quantity = 5 },
                        new { Name = "Apple", UnitPrice = .25m, Quantity = 10 },
                    }
                }
            });
            const string expected = @"Hello Bob:

Below are your order details:

    Banana: $2.50 x 1
    Orange: $0.50 x 5
    Apple: $0.25 x 10

Your order total was: $7.50";
            Assert.Equal(expected, result);
        }

        #endregion

        #region Unknown Tags

        /// <summary>
        /// If an unknown tag is encountered, an exception should be thrown.
        /// </summary>
        [Fact]
        public void TestCompile_UnknownTag()
        {
            Assert.Throws<FormatException>(() =>
            {
                FormatCompiler compiler = new FormatCompiler();
                compiler.Compile("{{#split Names}}");
            });
        }

        #endregion

        #region Context Variables

        /// <summary>
        /// We will use the index variable to determine whether or not to print out a line.
        /// </summary>
        [Fact]
        public void TestCompile_CanUseContextVariablesToMakeDecisions()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#each this}}{{#if @index}}{{#index}}{{/if}}{{/each}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(new int[] { 1, 1, 1, 1, });
            string expected = "123";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// I can set context variables to control the flow of the code generation.
        /// </summary>
        [Fact]
        public void TestCompile_CanUseContextVariableToToggle()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#set even}}{{#each this}}{{#if @even}}Even {{#else}}Odd {{/if}}{{#set even}}{{/each}}";
            Generator generator = compiler.Compile(format);
            generator.ValueRequested += (sender, e) =>
            {
                e.Value = !(bool)(e.Value ?? false);
            };
            string actual = generator.Render(new int[] { 1, 1, 1, 1 });
            string expected = "Even Odd Even Odd ";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// I can set context variables to control the flow of the code generation.
        /// It should even support nested context variables... for some reason.
        /// </summary>
        [Fact]
        public void TestCompile_CanUseNestedContextVariableToToggle()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#set this.variables.even}}
{{#each this}}
{{#if @variables.even}}
Even
{{#else}}
Odd
{{/if}}
{{#set variables.even}}
{{/each}}";
            Generator generator = compiler.Compile(format);
            generator.ValueRequested += (sender, e) =>
            {
                e.Value = !(bool)(e.Value ?? false);
            };
            string actual = generator.Render(new int[] { 1, 1, 1, 1 });
            string expected = "EvenOddEvenOdd";
            Assert.Equal(expected, actual);
        }

        #endregion

        #region New Line Management

		/// <summary>
		/// If the compiler is configured to ignore new lines,
        /// they should not be removed from the output.
		/// </summary>
		[Fact]
		public void TestCompile_PreserveNewLines() 
        {
		    FormatCompiler compiler = new FormatCompiler();
		    compiler.RemoveNewLines = false;
		    const string format = @"Hello
    ";

		    const string expected = @"Hello
    ";
		    Generator generator = compiler.Compile(format);
		    string result = generator.Render(null);
		    Assert.Equal(expected, result);
		}

        #endregion

        #region Strings

        /// <summary>
        /// We will use a string variable to determine whether or not to print out a line.
        /// </summary>
        [Fact]
        public void TestCompile_StringArgument_PassedToTag()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#if 'hello'}}Hello{{/if}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(null);
            string expected = "Hello";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We will use a string variable to determine whether or not to print out a line.
        /// </summary>
        [Fact]
        public void TestCompile_EmptyStringArgument_PassedToTag()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#if ''}}Hello{{/if}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(null);
            string expected = "";
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Numbers

        /// <summary>
        /// We will use a number variable to determine whether or not to print out a line.
        /// </summary>
        [Fact]
        public void TestCompile_NumberArgument_PassedToTag()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#if 4}}Hello{{/if}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(null);
            string expected = "Hello";
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// We will use a string variable to determine whether or not to print out a line.
        /// </summary>
        [Fact]
        public void TestCompile_ZeroNumberArgument_PassedToTag()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = @"{{#if 00.0000}}Hello{{/if}}";
            Generator generator = compiler.Compile(format);
            string actual = generator.Render(null);
            string expected = "";
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Custom Tags

        [Fact]
        public void TestCompile_NestedContext_ConsolidatesWriter()
        {
            FormatCompiler compiler = new FormatCompiler();
            compiler.RegisterTag(new UrlEncodeTagDefinition(), true);

            const string format = @"{{#urlencode}}{{url}}{{/urlencode}}";
            Generator generator = compiler.Compile(format);

            string actual = generator.Render(new { url = "https://google.com" });
            string expected = UrlEncoder.Default.Encode("https://google.com");
            Assert.Equal(expected, actual);
        }

        public class UrlEncodeTagDefinition : ContentTagDefinition
        {
            public UrlEncodeTagDefinition()
                : base("urlencode")
            {
            }

            public override IEnumerable<NestedContext> GetChildContext(TextWriter writer, Scope keyScope, Dictionary<string, object> arguments, Scope contextScope)
            {
                NestedContext context = new NestedContext()
                {
                    KeyScope = keyScope,
                    Writer = new StringWriter(),
                    WriterNeedsConsidated = true,
                };
                yield return context;
            }

            public override IEnumerable<TagParameter> GetChildContextParameters()
            {
                return new TagParameter[] { new TagParameter("collection") };
            }

            public override string ConsolidateWriter(TextWriter writer, Dictionary<string, object> arguments)
            {
                return UrlEncoder.Default.Encode(writer.ToString());
            }
        }

        #endregion

        #region Partials

        /// <summary>
        /// Stores a basic definition of a partial to be used in unit tests.
        /// </summary>
        private readonly PartialDefinition TestPartial =
            new PartialDefinition("myPartial", "{{Name}}");

        /// <summary>
        /// The normal behavior [when rendering] a partial that is not found
        /// is for the implementation to throw an error.
        /// </summary>
        [Fact]
        public void TestCompile_NoPartial_ShouldThrow()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = $"{{> {TestPartial.Name} }}";

            Assert.Throws<PartialNotFoundException>(() =>
            {
                Generator generator = compiler.Compile(format);

                const string name = "I am myself";
                string result = generator.Render(new { Name = name });
            });
        }

        /// <summary>
        /// http://handlebarsjs.com/partials.html#basic-partials
        /// You can define partial templates to facilitate template code reuse.
        /// </summary>
        [Fact]
        public void TestCompile_BasicPartials()
        {
            FormatCompiler compiler = new FormatCompiler();
            compiler.RegisterPartial(TestPartial);
            const string format = $"{{> {TestPartial.Name} }}";
            Generator generator = compiler.Compile(format);
            const string name = "I am myself";
            string result = generator.Render(new { Name = name });
            Assert.Equal(name, result);
        }


        /// <summary>
        /// Dynamic partials are not supported
        /// http://handlebarsjs.com/partials.html#dynamic-partials
        /// You can pass custom contexts to the partial template calls.
        /// http://handlebarsjs.com/partials.html#partial-context
        /// </summary>
        [Fact]
        public void TestCompile_PartialContexts()
        {
            FormatCompiler compiler = new FormatCompiler();
            compiler.RegisterPartial(TestPartial);
            const string format = $"{{> {TestPartial.Name} Person }}";
            Generator generator = compiler.Compile(format);
            const string name = "I am myself";
            string result = generator.Render(new { Person = new { Name = name } });
            Assert.Equal(name, result);
        }

        /// <summary>
        /// http://handlebarsjs.com/partials.html#partial-parameters
        /// You can pass custom parameters to partials.
        /// </summary>
        [Fact]
        public void TestCompile_PartialParameters()
        {
            FormatCompiler compiler = new FormatCompiler();
            compiler.RegisterPartial(TestPartial);
            const string format = $"{{> {TestPartial.Name} Name=Person.Name }}";
            Generator generator = compiler.Compile(format);
            const string name = "I am myself";
            string result = generator.Render(new { Person = new { Name = name } });
            Assert.Equal(name, result);
        }

        /// <summary>
        /// http://handlebarsjs.com/partials.html#partial-block
        /// To not throw errors for partials that may not be defined,
        /// you can pass in a block of failover content to be displayed
        /// should the partial not be found.
        /// </summary>
        [Fact]
        public void TestCompile_PartialBlocks_WithFailover()
        {
            // Template passing (using "@partial-block") is not supported
            FormatCompiler compiler = new FormatCompiler();
            const string failover = "Failover content";
            const string format = $@"{{> {TestPartial.Name} }}{failover}{{/{TestPartial.Name}}}";
            Generator generator = compiler.Compile(format);
            string result = generator.Render(null);
            Assert.Equal(failover, result);
        }

        /// <summary>
        /// http://handlebarsjs.com/partials.html#inline-partials
        /// Inline partials will be available to the current block and all children.
        /// However, having them available in the execution of other partials is not supported.
        /// </summary>
        [Fact]
        public void TestCompile_InlinePartials()
        {
            FormatCompiler compiler = new FormatCompiler();
            const string format = string.Format(@"{{#*inline ""{0}""}}{1}{{#newline}}{{/inline}}
{#each Names}}
  {{> {0}}}
{{/each}}", TestPartial.Name, TestPartial.Definition);
            Generator generator = compiler.Compile(format);
            string[] names = new string[]
            {
                "I am myself",
                "You are not different from me",
                "And you never were."
            };
            string result = generator.Render(new { Names = names });
            Assert.Equal(string.Join("\n", names), result);
        }

        #endregion
    }
}
