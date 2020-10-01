using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace CompileTimeExecution
{
    internal class AttributedMemberWalker<TAttribute> : CSharpSyntaxWalker
    {
        Action<MemberDeclarationSyntax> memberAction;
        public AttributedMemberWalker(Action<MemberDeclarationSyntax> memberAction)
        {
            this.memberAction = memberAction;
        }
        bool IsAttributed(MemberDeclarationSyntax node)
        {
            foreach (var attributeList in node.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (attribute.Name.GetLocalName() + "Attribute" == typeof(TAttribute).Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (IsAttributed(node))
                memberAction(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (IsAttributed(node))
                memberAction(node);
        }
    }
}
