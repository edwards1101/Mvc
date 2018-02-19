// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public static class ViewDataDirective
    {
        public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective(
            "viewdata",
            DirectiveKind.SingleLine,
            builder =>
            {
                builder
                    .AddMemberToken("PropertyName", "Property name")
                    .AddOptionalStringToken();

                builder.Usage = DirectiveUsage.FileScopedMultipleOccurring;
            });

        public static RazorProjectEngineBuilder Register(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddDirective(Directive);
            builder.Features.Add(new Pass());
            return builder;
        }

        internal class Pass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
        {
            // Runs after the @model and @namespace directives
            public override int Order => 10;

            protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
            {
                var visitor = new Visitor();
                visitor.Visit(documentNode);

                var properties = new HashSet<string>(StringComparer.Ordinal);

                for (var i = visitor.Directives.Count - 1; i >= 0; i--)
                {
                    var directive = visitor.Directives[i];
                    var tokens = directive.Tokens.ToArray();
                    if (tokens.Length < 1)
                    {
                        continue;
                    }

                    var memberName = tokens[0].Content;
                    string defaultValue = null;
                    if (tokens.Length > 1)
                    {
                        defaultValue = tokens[1].Content;
                    }

                    if (!properties.Add(memberName))
                    {
                        continue;
                    }

                    var injectNode = new ViewDataIntermediateNode()
                    {
                        MemberName = memberName,
                        DefaultValue = defaultValue,
                    };

                    visitor.Class.Children.Add(injectNode);
                }
            }
        }

        private class Visitor : IntermediateNodeWalker
        {
            public ClassDeclarationIntermediateNode Class { get; private set; }

            public IList<DirectiveIntermediateNode> Directives { get; } = new List<DirectiveIntermediateNode>();

            public override void VisitClassDeclaration(ClassDeclarationIntermediateNode node)
            {
                if (Class == null)
                {
                    Class = node;
                }

                base.VisitClassDeclaration(node);
            }

            public override void VisitDirective(DirectiveIntermediateNode node)
            {
                if (node.Directive == Directive)
                {
                    Directives.Add(node);
                }
            }
        }

        private class ViewDataIntermediateNode : ExtensionIntermediateNode
        {
            private const string ViewDataAttribute = "[global::Microsoft.AspNetCore.Mvc.ViewDataAttribute]";

            public string TypeName { get; set; }

            public string MemberName { get; set; }

            public string DefaultValue { get; set; }

            public override IntermediateNodeCollection Children => IntermediateNodeCollection.ReadOnly;

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                AcceptExtensionNode(this, visitor);
            }

            public override void WriteNode(CodeTarget target, CodeRenderingContext context)
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var property = $" public string {MemberName} {{ get; set; }}";
                if (DefaultValue != null)
                {
                    property += $" = {DefaultValue};";
                }

                context.CodeWriter
                    .WriteLine(ViewDataAttribute)
                    .WriteLine(property);
            }
        }
    }
}