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
using System.Globalization;


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
            //Rewriting - need to gather all the changes and apply them all at once, rather than my current method of doing it bit by bit
            //so first we find our nodes - the method to replace, the class name, and our new parameter name.
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            SyntaxToken methodToken = root.FindToken(span.Start);
            MethodDeclarationSyntax method = methodToken.Parent as MethodDeclarationSyntax;

            if (method == null)
                return Enumerable.Empty<CodeAction>(); //ignore non-methods
            if (method.Modifiers.Any(m => !(m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.PrivateKeyword)
                || m.IsKind(SyntaxKind.InternalKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword))))
                return Enumerable.Empty<CodeAction>(); //ignore any kind of special methods
            var className = (method.Parent as ClassDeclarationSyntax).Identifier.WithTrailingTrivia(); //needs no trivia, else it wants to generate Foo\r\n.Bar
            //generate a parameter name to put in the new method node
            String parameterName = (method.Parent as ClassDeclarationSyntax).Identifier.ToString();
            if (parameterName.Length > 1)
                parameterName = Char.ToLowerInvariant(parameterName[0]) + parameterName.Substring(1);

            //generate a new method modifier list with static included
            SyntaxTokenList methodModifiers = SyntaxFactory.TokenList(method.Modifiers.ToArray()).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            //now add the original object as a parameter
            ParameterSyntax newParam = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), default(SyntaxTokenList), SyntaxFactory.IdentifierName(className),
                SyntaxFactory.Identifier(parameterName), default(EqualsValueClauseSyntax));
            ParameterListSyntax newParamList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new ParameterSyntax[] { newParam })
                .AddRange(method.ParameterList.Parameters));

            changes.Add(method, method.WithModifiers(methodModifiers).WithParameterList(newParamList).WithAdditionalAnnotations(Formatter.Annotation));

            //find all the references so we can change them
            IMethodSymbol symbol = model.GetDeclaredSymbol(method, cancellationToken);
            var methodReferences = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution);
            SyntaxAnnotation updateInvocationAnnotation = new SyntaxAnnotation();
            foreach (var reference in methodReferences)
            {
                foreach (var location in reference.Locations)
                {
                    var expression = root.FindToken(location.Location.SourceSpan.Start).Parent.Parent as MemberAccessExpressionSyntax;
                    if (expression.Parent is InvocationExpressionSyntax)
                    {
                        //we're invoking the method, so add in the parameter
                        InvocationExpressionSyntax invocation = expression.Parent as InvocationExpressionSyntax;
                        ArgumentSyntax newArg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameterName));
                        ArgumentListSyntax newArgList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[] { newArg }).AddRange(invocation.ArgumentList.Arguments));
                        //and change the method to use the static version
                        expression = expression.Update(SyntaxFactory.IdentifierName(className), expression.OperatorToken, expression.Name).WithAdditionalAnnotations(Formatter.Annotation);
                        var newInvocation = SyntaxFactory.InvocationExpression(expression, newArgList).WithAdditionalAnnotations(Formatter.Annotation);
                        changes.Add(invocation, newInvocation);
                    }
                }
            }

            SyntaxNode newRoot = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);
            return new[] { CodeActionFactory.Create(methodToken.Span, DiagnosticSeverity.Info, "Add the static modifier to a method.", 
                document.WithSyntaxRoot(newRoot))};
        }
    }
}
