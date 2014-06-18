//
// AddNewModifierToMethodAction.cs
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

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp;
using ICSharpCode.NRefactory6.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
    /// <summary>
    /// Adds the new modifier to a method in a derived class.
    /// </summary>
    [NRefactoryCodeRefactoringProvider(Description = "Add the new modifier to a method.")]
    [ExportCodeRefactoringProvider("Add the new modifier to a method.", LanguageNames.CSharp)]
    public class AddNewModifierToMethodAction : ICodeRefactoringProvider
    {
        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);

            var token = root.FindToken(span.Start); //get our node
            MethodDeclarationSyntax method = token.Parent as MethodDeclarationSyntax;
            if (method == null)
                return Enumerable.Empty<CodeAction>(); //ignore non-methods
            if (method.Modifiers.Any((modifier => modifier.IsKind(SyntaxKind.OverrideKeyword) || modifier.IsKind(SyntaxKind.VirtualKeyword))))
                return Enumerable.Empty<CodeAction>(); //ignore overriding methods

            String methodName = (token.Parent as MethodDeclarationSyntax).Identifier.Text;
            var overrideMethod = model.LookupBaseMembers(span.Start).Where(m => m.IsVirtual && m.Name.Equals(methodName)).FirstOrDefault();
            if(overrideMethod == null)
                return Enumerable.Empty<CodeAction>(); //ignore methods that don't need new

            //so now add new to the modifiers
            var modifiers = SyntaxFactory.TokenList(method.Modifiers.ToArray()).Add(SyntaxFactory.Token(SyntaxKind.NewKeyword));
            MethodDeclarationSyntax newMethod = method.WithModifiers(modifiers).WithAdditionalAnnotations(Formatter.Annotation);
            SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
            return new[] { CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, "Add the new modifier to a method.", document.WithSyntaxRoot(newRoot)) };
        }
    }
}