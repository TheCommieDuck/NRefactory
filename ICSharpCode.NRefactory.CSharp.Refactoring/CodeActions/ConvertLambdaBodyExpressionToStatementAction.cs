﻿// 
// ConvertLambdaBodyExpressionToStatementAction.cs
// 
// Author:
//      Mansheng Yang <lightyang0@gmail.com>
// 
// Copyright (c) 2012 Mansheng Yang <lightyang0@gmail.com>
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
	[NRefactoryCodeRefactoringProvider(Description = "Converts expression of lambda body to statement")]
	[ExportCodeRefactoringProvider("Converts expression of lambda body to statement", LanguageNames.CSharp)]
	public class ConvertLambdaBodyExpressionToStatementAction : SpecializedCodeAction<SimpleLambdaExpressionSyntax>
	{
		protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, SimpleLambdaExpressionSyntax node, CancellationToken cancellationToken)
		{
			yield break;
		}
//
//		protected override CodeAction GetAction (SemanticModel context, LambdaExpression node)
//		{
//			if (!node.ArrowToken.Contains (context.Location))
//				return null;
//
//			var bodyExpr = node.Body as Expression;
//			if (bodyExpr == null)
//				return null;
//			return new CodeAction (context.TranslateString ("Convert to lambda statement"),
//				script =>
//				{
//					var body = new BlockStatement ();
//					if (RequireReturnStatement (context, node)) {
//						body.Add (new ReturnStatement (bodyExpr.Clone ()));
//					} else {
//						body.Add (bodyExpr.Clone ());
//					}
//					script.Replace (bodyExpr, body);
//				},
//				node
//			);
//		}
//
//		static bool RequireReturnStatement (SemanticModel context, LambdaExpression lambda)
//		{
//			var type = LambdaHelper.GetLambdaReturnType (context, lambda);
//			return type != null && type.ReflectionName != "System.Void";
//		}
	}
}
