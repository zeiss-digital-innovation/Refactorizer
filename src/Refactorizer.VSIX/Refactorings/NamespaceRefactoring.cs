using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Refactorizer.VSIX.Analyser;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.ViewModels;
using Solution = Refactorizer.VSIX.Models.Solution;

namespace Refactorizer.VSIX.Refactorings
{
    internal class NamespaceRefactoring : IRefactoring
    {
        private readonly ClassRefactoring _classRefactoring;
        private readonly SolutionParserBridge _solutionParserBridge;

        public NamespaceRefactoring(ClassRefactoring classRefactoring, SolutionParserBridge solutionParserBridge)
        {
            _classRefactoring = classRefactoring;
            _solutionParserBridge = solutionParserBridge;
        }

        public async Task<ActionResult> Delete(Namespace ns)
        {
            var result = ActionResult.Success;
            foreach (var @class in ns.Classes)
            {
                var actionResult = await _classRefactoring.Delete(@class);
                if (actionResult == ActionResult.Failed)
                    result = ActionResult.Failed;
            }

            return result;
        }

        public async Task<ActionResult> Rename(Namespace ns, string newName)
        {
            var @class = ns.Classes.FirstOrDefault();

            var document = await _solutionParserBridge.GetDocument(@class);
            if (document == null)
                return ActionResult.Failed;

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var node = GetNamespaceDeclarationSyntax(ns.Name, syntaxRoot, semanticModel);
            if (node == null)
                return ActionResult.Failed;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return ActionResult.Failed;

            var solution = document.Project.Solution;
            var optionSet = solution.Workspace.Options;
            var newSymbolNames = newName.Split('.').ToList();
            var parentSymbol = symbol.ContainingNamespace;
            var symbolsToChange = new List<ISymbol> {symbol};

            while (!string.IsNullOrEmpty(parentSymbol.Name))
            {
                symbolsToChange.Add(parentSymbol);
                parentSymbol = parentSymbol.ContainingNamespace;
            }
            symbolsToChange.Reverse();
            var index = 0;

            var newSolution = solution;
            foreach (var namespaceSymbol in symbolsToChange)
            {
                var newSymbolName = (index < newSymbolNames.Count)
                    ? newSymbolNames[index]
                    : (index == newSymbolNames.Count && newSymbolNames.Count > symbolsToChange.Count)
                        ? string.Join(".", newSymbolNames.GetRange(index, newSymbolNames.Count))
                        : string.Empty;
                index++;
                if (string.IsNullOrEmpty(newSymbolName))
                {
                    // TODO: This is a move
                }
                else
                {
                    if (!namespaceSymbol.Name.Equals(newSymbolName))
                    {
                        newSolution = await Renamer.RenameSymbolAsync(newSolution, namespaceSymbol, newSymbolName, optionSet);
                    }
                }
            }
            ns.Name = newName;

            return !solution.Workspace.TryApplyChanges(newSolution) ? ActionResult.Failed : ActionResult.Success;
        }

        private NamespaceDeclarationSyntax GetNamespaceDeclarationSyntax(string name, SyntaxNode syntaxNode,
            SemanticModel semanticModel)
        {
            var namespaces = syntaxNode.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
            foreach (var namespaceDeclarationSyntax in namespaces)
            {
                var symbol = semanticModel.GetDeclaredSymbol(namespaceDeclarationSyntax);
                if (symbol == null)
                    continue;

                if (symbol.ToDisplayString().Equals(name))
                    return namespaceDeclarationSyntax;
            }

            return null;
        }
    }
}