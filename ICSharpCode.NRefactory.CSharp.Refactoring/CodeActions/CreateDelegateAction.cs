// 
// CreateDelegateAction.cs
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
	[NRefactoryCodeRefactoringProvider(Description = "Creates a delegate declaration out of an event declaration")]
	[ExportCodeRefactoringProvider("Create delegate", LanguageNames.CSharp)]
	public class CreateDelegateAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			var simpleType = context.GetNode<SimpleType>();
//			if (simpleType != null && (simpleType.Parent is EventDeclaration || simpleType.Parent is CustomEventDeclaration)) 
//				return GetActions(context, simpleType);
//
//			return Enumerable.Empty<CodeAction>();
//		}
//
//		static IEnumerable<CodeAction> GetActions(SemanticModel context, SimpleType node)
//		{
//			var resolveResult = context.Resolve(node) as UnknownIdentifierResolveResult;
//			if (resolveResult == null)
//				yield break;
//
//			yield return new CodeAction(context.TranslateString("Create delegate"), script => {
//				script.CreateNewType(CreateType(context,  node));
//			}, node);
//
//		}
//
//		static DelegateDeclaration CreateType(SemanticModel context, SimpleType simpleType)
//		{
//			var result = new DelegateDeclaration() {
//				Name = simpleType.Identifier,
//				Modifiers = ((EntityDeclaration)simpleType.Parent).Modifiers,
//				ReturnType = new PrimitiveType("void"),
//				Parameters = {
//					new ParameterDeclaration(new PrimitiveType("object"), "sender"),
//					new ParameterDeclaration(context.CreateShortType("System", "EventArgs"), "e")
//				}
//			};
//			return result;
//		}
	}
}