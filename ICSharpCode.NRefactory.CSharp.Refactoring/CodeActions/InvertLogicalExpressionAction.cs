﻿// 
// InvertLogicalExpressionAction.cs
// 
// Author:
//      Ji Kun<jikun.nus@gmail.com>
// 
// Copyright (c) 2012 Ji Kun<jikun.nus@gmail.com>
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
	[NRefactoryCodeRefactoringProvider(Description = "Inverts a logical expression")]
	[ExportCodeRefactoringProvider("Invert logical expression", LanguageNames.CSharp)]
	public class InvertLogicalExpressionAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public override System.Collections.Generic.IEnumerable<CodeAction> GetActions(SemanticModel context)
//		{
//			Expression expr = null;
//			AstNode token;
//			if (!NegateRelationalExpressionAction.GetLogicalExpression (context, out expr, out token))
//				yield break;
//
//			var uOp = expr as UnaryOperatorExpression;
//			if (uOp != null) {
//				yield return new CodeAction(
//					string.Format(context.TranslateString("Invert '{0}'"), expr),
//					script => {
//						script.Replace(uOp, CSharpUtil.InvertCondition(CSharpUtil.GetInnerMostExpression(uOp.Expression)));
//					}, token
//				);	
//				yield break;
//			}
//
//			var negativeExpression = CSharpUtil.InvertCondition(expr);
//			if (expr.Parent is ParenthesizedExpression && expr.Parent.Parent is UnaryOperatorExpression) {
//				var unaryOperatorExpression = expr.Parent.Parent as UnaryOperatorExpression;
//				if (unaryOperatorExpression.Operator == UnaryOperatorType.Not) {
//					yield return new CodeAction(
//						string.Format(context.TranslateString("Invert '{0}'"), unaryOperatorExpression),
//						script => {
//							script.Replace(unaryOperatorExpression, negativeExpression);
//						}, token
//					);	
//					yield break;
//				}
//			}
//			var newExpression = new UnaryOperatorExpression(UnaryOperatorType.Not, new ParenthesizedExpression(negativeExpression));
//			yield return new CodeAction(
//				string.Format(context.TranslateString("Invert '{0}'"), expr),
//				script => {
//					script.Replace(expr, newExpression);
//				}, token
//			);
//		}
	}
}