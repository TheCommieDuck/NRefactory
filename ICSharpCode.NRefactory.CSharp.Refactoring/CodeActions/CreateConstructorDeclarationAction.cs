// 
// CreateClassDeclarationAction.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2012 Xamarin <http://xamarin.com>
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
	[NRefactoryCodeRefactoringProvider(Description = "Creates a constructor declaration out of an object creation")]
	[ExportCodeRefactoringProvider("Create constructor", LanguageNames.CSharp)]
	public class CreateConstructorDeclarationAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			var createExpression = context.GetNode<Expression>() as ObjectCreateExpression;
//			if (createExpression == null) 
//				yield break;
//			
//			var resolveResult = context.Resolve(createExpression) as CSharpInvocationResolveResult;
//			if (resolveResult == null || !resolveResult.IsError || resolveResult.Member.DeclaringTypeDefinition == null || resolveResult.Member.DeclaringTypeDefinition.IsSealed || resolveResult.Member.DeclaringTypeDefinition.Region.IsEmpty)
//				yield break;
//
//			yield return new CodeAction(context.TranslateString("Create constructor"), script =>
//				script.InsertWithCursor(
//					context.TranslateString("Create constructor"),
//					resolveResult.Member.DeclaringTypeDefinition,
//					(s, c) => {
//						var decl = new ConstructorDeclaration {
//							Name = resolveResult.Member.DeclaringTypeDefinition.Name,
//							Modifiers = Modifiers.Public,
//							Body = new BlockStatement {
//								new ThrowStatement(new ObjectCreateExpression(c.CreateShortType("System", "NotImplementedException")))
//							}
//						};
//						decl.Parameters.AddRange(CreateMethodDeclarationAction.GenerateParameters(context, createExpression.Arguments));
//						return decl;
//					}
//				)
//			, createExpression) { Severity = ICSharpCode.NRefactory.Refactoring.Severity.Error };
//		}
	}
}