using System;
using System.Collections.Generic;
using System.IO;

namespace Mustache
{
    public class PartialCallTagDefinition : TagDefinition
    {
        private const string PartialCallTag = ">";
        private const string NameParameter = "name";
        private const string ContextParameter = "context";
        private static readonly TagParameter[] InnerTags =
        {
            new TagParameter(NameParameter) { IsRequired = true },
            new TagParameter(ContextParameter) { IsRequired = false }
        };
        private static readonly TagParameter[] InnerContextTags =
        {
            new TagParameter(ContextParameter) { IsRequired = false }
        };

        private bool _hasContent;

        public PartialCallTagDefinition(bool hasContent)
            : base(PartialCallTag, true)
        {
            this._hasContent = hasContent;
        }

        protected override IEnumerable<TagParameter> GetParameters() => InnerTags;

        public override IEnumerable<TagParameter> GetChildContextParameters() => InnerContextTags;

        protected override bool GetHasContent() => _hasContent;

        public override IEnumerable<NestedContext> GetChildContext(
            TextWriter writer,
            Scope keyScope,
            Dictionary<string, object> arguments,
            Scope contextScope)
        {
            object contextSource = arguments[ContextParameter];
            NestedContext context = new NestedContext()
            {
                KeyScope = keyScope.CreateChildScope(contextSource),
                Writer = writer,
                ContextScope = contextScope.CreateChildScope()
            };
            yield return context;
        }
    }
}
