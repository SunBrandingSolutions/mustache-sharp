﻿using System;
#if NETSTANDARD1_3
using System.Text.Encodings.Web;
#else
using System.Security;
#endif

namespace Mustache
{
    public sealed class HtmlFormatCompiler
    {
        private readonly FormatCompiler compiler;

        public HtmlFormatCompiler()
        {
            compiler = new FormatCompiler();
            compiler.AreExtensionTagsAllowed = true;
            compiler.RemoveNewLines = true;
        }

        /// <summary>
        /// Occurs when a placeholder is found in the template.
        /// </summary>
        public event EventHandler<PlaceholderFoundEventArgs> PlaceholderFound
        {
            add { compiler.PlaceholderFound += value; }
            remove { compiler.PlaceholderFound -= value; }
        }

        /// <summary>
        /// Occurs when a variable is found in the template.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs> VariableFound
        {
            add { compiler.VariableFound += value; }
            remove { compiler.VariableFound -= value; }
        }

        /// <summary>
        /// Registers the given tag definition with the parser.
        /// </summary>
        /// <param name="definition">The tag definition to register.</param>
        /// <param name="isTopLevel">Specifies whether the tag is immediately in scope.</param>
        public void RegisterTag(TagDefinition definition, bool isTopLevel)
        {
            compiler.RegisterTag(definition, isTopLevel);
        }

        /// <summary>
        /// Builds a text generator based on the given format.
        /// </summary>
        /// <param name="format">The format to parse.</param>
        /// <returns>The text generator.</returns>
        public Generator Compile(string format)
        {
            Generator generator = compiler.Compile(format);
            generator.TagFormatted += escapeInvalidHtml;
            return generator;
        }

        private static void escapeInvalidHtml(object sender, TagFormattedEventArgs e)
        {
            if (e.IsExtension)
            {
                // Do not escape text within triple curly braces
                return;
            }

#if NETSTANDARD1_3
            e.Substitute = HtmlEncoder.Default.Encode(e.Substitute);
#else
            e.Substitute = SecurityElement.Escape(e.Substitute);
#endif
        }
    }
}
