// 
// DeclareLocalVariableAction.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
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
	[NRefactoryCodeRefactoringProvider(Description = "Declare a local variable out of a selected expression")]
	[ExportCodeRefactoringProvider("Declare local variable", LanguageNames.CSharp)]
	public class DeclareLocalVariableAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			if (!context.IsSomethingSelected) {
//				yield break;
//			}
//			var selected = new List<AstNode>(context.GetSelectedNodes());
//
//			if (selected.Count != 1 || !(selected [0] is Expression)) {
//				yield break;
//			}
//
//			var expr = selected [0] as Expression;
//			if (expr is ArrayInitializerExpression) {
//				var arr = (ArrayInitializerExpression)expr;
//				if (arr.IsSingleElement) {
//					expr = arr.Elements.First();
//				} else {
//					yield break;
//				}
//			}
//
//			var visitor = new SearchNodeVisitior(expr);
//			
//			var node = context.GetNode <BlockStatement>();
//			if (node != null) {
//				node.AcceptVisitor(visitor);
//			}
//
//			yield return new CodeAction(context.TranslateString("Declare local variable"), script => {
//				var resolveResult = context.Resolve(expr);
//				var guessedType = resolveResult.Type;
//				if (resolveResult is MethodGroupResolveResult) {
//					guessedType = GetDelegateType(context, ((MethodGroupResolveResult)resolveResult).Methods.First(), expr);
//				}
//				var name = CreateMethodDeclarationAction.CreateBaseName(expr, guessedType);
//				name = context.GetLocalNameProposal(name, expr.StartLocation);
//				var type = context.UseExplicitTypes ? context.CreateShortType(guessedType) : new SimpleType("var");
//				var varDecl = new VariableDeclarationStatement(type, name, expr.Clone());
//				var replaceNode = visitor.Matches.First () as Expression;
//				if (replaceNode.Parent is ExpressionStatement) {
//					script.Replace(replaceNode.Parent, varDecl);
//					script.Select(varDecl.Variables.First().NameToken);
//				} else {
//					var containing = replaceNode.Parent;
//					while (!(containing.Parent is BlockStatement)) {
//						containing = containing.Parent;
//					}
//
//					script.InsertBefore(containing, varDecl);
//					var identifierExpression = new IdentifierExpression(name);
//					script.Replace(replaceNode, identifierExpression);
//					script.Link(varDecl.Variables.First().NameToken, identifierExpression);
//				}
//			}, expr);
//
//			if (visitor.Matches.Count > 1) {
//				yield return new CodeAction(string.Format(context.TranslateString("Declare local variable (replace '{0}' occurrences)"), visitor.Matches.Count), script => {
//					var resolveResult = context.Resolve(expr);
//					var guessedType = resolveResult.Type;
//					if (resolveResult is MethodGroupResolveResult) {
//						guessedType = GetDelegateType(context, ((MethodGroupResolveResult)resolveResult).Methods.First(), expr);
//					}
//					var linkedNodes = new List<AstNode>();
//					var name = CreateMethodDeclarationAction.CreateBaseName(expr, guessedType);
//					name = context.GetLocalNameProposal(name, expr.StartLocation);
//					var type = context.UseExplicitTypes ? context.CreateShortType(guessedType) : new SimpleType("var");
//					var varDecl = new VariableDeclarationStatement(type, name, expr.Clone());
//					linkedNodes.Add(varDecl.Variables.First().NameToken);
//					var first = visitor.Matches [0];
//					if (first.Parent is ExpressionStatement) {
//						script.Replace(first.Parent, varDecl);
//					} else {
//						var containing = first.Parent;
//						while (!(containing.Parent is BlockStatement)) {
//							containing = containing.Parent;
//						}
//
//						script.InsertBefore(containing, varDecl);
//						var identifierExpression = new IdentifierExpression(name);
//						linkedNodes.Add(identifierExpression);
//						script.Replace(first, identifierExpression);
//					}
//					for (int i = 1; i < visitor.Matches.Count; i++) {
//						var identifierExpression = new IdentifierExpression(name);
//						linkedNodes.Add(identifierExpression);
//						script.Replace(visitor.Matches [i], identifierExpression);
//					}
//					script.Link(linkedNodes.ToArray ());
//				}, expr);
//			}
//		}
//
//		// Gets Action/Func delegate types for a given method.
//		IType GetDelegateType(SemanticModel context, IMethod method, Expression expr)
//		{
//			var parameters = new List<IType>();
//			var invoke = expr.Parent as InvocationExpression;
//			if (invoke == null) {
//				return null;
//			}
//			foreach (var arg in invoke.Arguments) {
//				parameters.Add(context.Resolve(arg).Type);
//			}
//
//			ITypeDefinition genericType;
//			if (method.ReturnType.FullName == "System.Void") {
//				genericType = context.Compilation.GetAllTypeDefinitions().FirstOrDefault(t => t.FullName == "System.Action" && t.TypeParameterCount == parameters.Count);
//			} else {
//				parameters.Add(method.ReturnType);
//				genericType = context.Compilation.GetAllTypeDefinitions().FirstOrDefault(t => t.FullName == "System.Func" && t.TypeParameterCount == parameters.Count);
//			}
//			if (genericType == null) {
//				return null;
//			}
//			return new ParameterizedType(genericType, parameters);
//		}
//
//				
//		internal class SearchNodeVisitior : DepthFirstAstVisitor
//		{
//			readonly AstNode searchForNode;
//			public readonly List<AstNode> Matches = new List<AstNode> ();
//			
//			public SearchNodeVisitior (AstNode searchForNode)
//			{
//				this.searchForNode = searchForNode;
//				AddNode (searchForNode);
//			}
//
//			void AddNode(AstNode node)
//			{
//				if (node.Parent is ParenthesizedExpression) {
//					Matches.Add(node.Parent);
//				} else {
//					Matches.Add(node);
//				}
//			}
//			
//			protected override void VisitChildren(AstNode node)
//			{
//				if (node.StartLocation > searchForNode.StartLocation && node.IsMatch(searchForNode)) {
//					AddNode(node);
//					return;
//				} 
//				base.VisitChildren (node);
//			}
//		}
	}
}
