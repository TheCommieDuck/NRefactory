//
// ConvertMethodGroupToLambdaAction.cs
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
	[NRefactoryCodeRefactoringProvider(Description = "Convert method group to lambda expression")]
	[ExportCodeRefactoringProvider("Convert method group to lambda expression", LanguageNames.CSharp)]
	public class ConvertMethodGroupToLambdaAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			Expression node = context.GetNode<IdentifierExpression>();
//			if (node == null) {
//				var mr = context.GetNode<MemberReferenceExpression>();
//				if (mr == null || !mr.MemberNameToken.IsInside(context.Location))
//					yield break;
//				node = mr;
//			}
//			if (node == null)
//				yield break;
//			var rr = context.Resolve(node) as MethodGroupResolveResult;
//			if (rr == null || rr.IsError)
//				yield break;
//			var type = TypeGuessing.GetValidTypes(context.Resolver, node).FirstOrDefault(t => t.Kind == TypeKind.Delegate);
//			if (type == null)
//				yield break;
//			var invocationMethod = type.GetDelegateInvokeMethod();
//			if (invocationMethod == null)
//				yield break;
//
//			yield return new CodeAction(
//				context.TranslateString("Convert to lambda expression"), 
//				script => {
//					var invocation = new InvocationExpression(node.Clone(), invocationMethod.Parameters.Select(p => new IdentifierExpression(context.GetNameProposal(p.Name))));
//					var lambda = new LambdaExpression {
//						Body = invocation
//					};
//					lambda.Parameters.AddRange(
//						invocation.Arguments
//							.Cast<IdentifierExpression>()
//							.Select(p => new ParameterDeclaration { Name = p.Identifier })
//					);
//					script.Replace(node, lambda);
//				},
//				node
//			);
//		}
	}
}