//
// AddStaticModifierToMethodAction.cs
//
// Author:
//       Mark Garnett <mg10g13@soton.ac.uk>
//
// Copyright (c) 2014 Mark Garnett (mg10g13@soton.ac.uk)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;


namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
    /// <summary>
    /// Converts a method to a static method.
    /// </summary>
    [NRefactoryCodeRefactoringProvider(Description = "Add the static modifier to a method.")]
    [ExportCodeRefactoringProvider("Add the static modifier to a method.", LanguageNames.CSharp)]
    public class AddStaticModifierToMethodAction : ICodeRefactoringProvider
    {
        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);

            SyntaxToken token = root.FindToken(span.Start); //get our node
            MethodDeclarationSyntax method = token.Parent as MethodDeclarationSyntax;
            if (method == null)
                return Enumerable.Empty<CodeAction>(); //ignore non-methods
            if (method.Modifiers.Any(m => !(m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.PrivateKeyword) 
                || m.IsKind(SyntaxKind.InternalKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword))))
                return Enumerable.Empty<CodeAction>(); //ignore any kind of special methods

            //so now add static to the modifiers
            SyntaxTokenList modifiers = SyntaxFactory.TokenList(method.Modifiers.ToArray()).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            MethodDeclarationSyntax newMethod = method.WithModifiers(modifiers).WithAdditionalAnnotations(Formatter.Annotation);

            //and now we can replace all references
            IMethodSymbol symbol = model.GetDeclaredSymbol(method, cancellationToken);
            var references = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution);
            foreach(var reference in references)
            {
                foreach(var location in reference.Locations)
                {
                    var invocation = root.FindToken(location.Location.SourceSpan.Start).Parent.Parent.Parent.Parent;
                }
            }
            return null;
            //SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
            //return new[] { CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, "Add the static modifier to a method.", document.WithSyntaxRoot(newRoot)) };
        }
    }
}
