using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Refactorizer.VSIX.Analyser
{
    class MethodSyntaxWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;

        public List<string> ClassReferences { get; } = new List<string>();

        public List<string> MethodReferences { get; } = new List<string>();

        public MethodSyntaxWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var objectCreationExpressionSyntaxs = node.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
            foreach (var expressionSyntax in objectCreationExpressionSyntaxs)
            {
                var symbol = _semanticModel.GetSymbolInfo(expressionSyntax).Symbol;
                if (symbol == null)
                    continue;

                var type = symbol.ContainingSymbol.ToDisplayString();
                ClassReferences.Add(type);
            }
            base.VisitAssignmentExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol == null)
                return;

            var type = symbol.ToDisplayString();
            MethodReferences.Add(type);
            base.VisitInvocationExpression(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node.Type).Symbol;
            if (symbol == null)
                return;
            var type = symbol.ToDisplayString();
            ClassReferences.Add(type);
            base.VisitVariableDeclaration(node);
        }
    }
}
