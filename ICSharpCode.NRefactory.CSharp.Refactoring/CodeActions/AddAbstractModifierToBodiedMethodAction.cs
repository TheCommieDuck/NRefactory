//
// AddAbstractModifierToBodiedMethodAction.cs
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

using ICSharpCode.NRefactory6.CSharp;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
    /// <summary>
    /// Converts a method in an abstract class to an abstract method.
    /// </summary>
    [NRefactoryCodeRefactoringProvider(Description = "Convert method to an abstract method, preserving the method body.")]
    [ExportCodeRefactoringProvider("Convert method to an abstract method, preserving the method body.", LanguageNames.CSharp)]
    public class AddAbstractModifierToBodiedMethodAction : ICodeRefactoringProvider
    {
        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            var token = root.FindToken(span.Start); //get our node
            MethodDeclarationSyntax method = token.Parent as MethodDeclarationSyntax;
            if(method == null)
                return Enumerable.Empty<CodeAction>();

            ClassDeclarationSyntax enclosingClass = method.Ancestors().Where(parent => parent.IsKind(SyntaxKind.ClassDeclaration)).First() as ClassDeclarationSyntax;
            if(!enclosingClass.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword))) //if non-abstract class
                return Enumerable.Empty<CodeAction>();

            //so get the body content and copy to a comment node. Remove the braces
            var body = method.Body.WithTrailingTrivia().ToString().Remove(0, 1);
            body = body.Remove(body.Length - 1);
            var comment = SyntaxFactory.Comment("/*" + body + "*/");
            var leadingTrivia = SyntaxFactory.TriviaList(comment);
            leadingTrivia.AddRange(method.GetLeadingTrivia());

            //then remove the body
            var modifiers = SyntaxFactory.TokenList(method.Modifiers.ToArray()).Add(SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
            MethodDeclarationSyntax newMethod = method.RemoveNode(method.Body, SyntaxRemoveOptions.KeepLeadingTrivia).WithModifiers(modifiers)
                .WithAdditionalAnnotations(Formatter.Annotation).WithLeadingTrivia(leadingTrivia);
            SyntaxNode newRoot = root.ReplaceNode(method, newMethod.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            return new[] { CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, "Convert method to abstract method, preserving the method body", document.WithSyntaxRoot(newRoot)) };
        }
    }
}
