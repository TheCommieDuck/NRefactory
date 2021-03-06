//
// SplitIfAction.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
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
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[NRefactoryCodeRefactoringProvider(Description = "Splits an if statement into two nested if statements")]
	[ExportCodeRefactoringProvider("Split 'if' statement", LanguageNames.CSharp)]
	public class SplitIfAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			var token = root.FindToken(span.Start);
			var ifNode = token.Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().FirstOrDefault();
			if (ifNode == null)
				return Enumerable.Empty<CodeAction>();
			var binOp = token.Parent as BinaryExpressionSyntax;

			if (binOp == null)
				return Enumerable.Empty<CodeAction>();

			if (binOp.Ancestors().OfType<BinaryExpressionSyntax>().Any(b => !b.OperatorToken.IsKind(binOp.OperatorToken.CSharpKind())))
				return Enumerable.Empty<CodeAction>();
			if (binOp.OperatorToken.IsKind(SyntaxKind.AmpersandAmpersandToken)) {
				var nestedIf = ifNode.WithCondition(GetRightSide(binOp));
				var outerIf = ifNode.WithCondition(GetLeftSide(binOp)).WithStatement(SyntaxFactory.Block(nestedIf));
				return new[] { CodeActionFactory.Create(span, DiagnosticSeverity.Info, "Split if", document.WithSyntaxRoot(
                    root.ReplaceNode(ifNode, outerIf.WithAdditionalAnnotations(Formatter.Annotation))))};
			} else if (binOp.OperatorToken.IsKind(SyntaxKind.BarBarToken)) {
				var newElse = ifNode.WithCondition(GetRightSide(binOp));
				var newIf = ifNode.WithCondition(GetLeftSide(binOp)).WithElse(SyntaxFactory.ElseClause(newElse));
				return new[] { CodeActionFactory.Create(span, DiagnosticSeverity.Info, "Split if", document.WithSyntaxRoot(
                    root.ReplaceNode(ifNode, newIf.WithAdditionalAnnotations(Formatter.Annotation))))};
			}
			return Enumerable.Empty<CodeAction>();
		}

		internal static ExpressionSyntax GetRightSide(BinaryExpressionSyntax expression)
		{
			var parent = expression.Parent as BinaryExpressionSyntax;
			if (parent != null) {
				if (parent.Left.IsEquivalentTo(expression)) {
					var parentClone = (parent as BinaryExpressionSyntax).WithLeft(expression.Right);
					return parentClone;
				}
			}
			return expression.Right;
		}

		internal static ExpressionSyntax GetLeftSide(BinaryExpressionSyntax expression)
		{
			var parent = expression.Parent as BinaryExpressionSyntax;
			if (parent != null) {
				if (parent.Right.IsEquivalentTo(expression)) {
					var parentClone = (parent as BinaryExpressionSyntax).WithRight(expression.Left);
					return parentClone;
				}
			}
			return expression.Left;
		}
	}
}

