//
// ExceptionParameterCantBeResolvedIssue.cs
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
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[DiagnosticAnalyzer]
	[ExportDiagnosticAnalyzer("Cannot resolve symbol in text argument", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "NotResolvedInText")]
	public class NotResolvedInTextIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "NotResolvedInTextIssue";
		const string Description            = "Cannot resolve symbol in text argument";
		const string MessageFormat          = "The parameter '{0}' can't be resolved";
		const string Category               = IssueCategories.CodeQualityIssues;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}

		class GatherVisitor : GatherVisitorBase<NotResolvedInTextIssue>
		{
//			readonly BaseSemanticModel context;

			static GatherVisitor()
			{
			}

			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

//			static string GetArgumentParameterName(Expression expression)
//			{
//				var pExpr = expression as PrimitiveExpression;
//				if (pExpr != null)
//					return pExpr.Value as string;
//				return null;
//			}
//
//
//			bool CheckExceptionType(ObjectCreateExpression objectCreateExpression, out Expression paramNode, out Expression altParam, out bool canAddParameterName)
//			{
//				paramNode = null;
//				altParam = null;
//				canAddParameterName = false;
//				var rr = context.Resolve(objectCreateExpression.Type) as TypeResolveResult;
//				if (rr == null)
//					return false;
//
//				var type = rr.Type;
//				if (type.Name == typeof(ArgumentException).Name && type.Namespace == typeof(ArgumentException).Namespace) {
//					if (objectCreateExpression.Arguments.Count >= 2) {
//						altParam = objectCreateExpression.Arguments.ElementAt(0);
//						paramNode = objectCreateExpression.Arguments.ElementAt(1);
//
//					}
//					return paramNode != null;
//				}
//				if (type.Name == typeof(ArgumentNullException).Name && type.Namespace == typeof(ArgumentNullException).Namespace ||
//				    type.Name == typeof(ArgumentOutOfRangeException).Name && type.Namespace == typeof(ArgumentOutOfRangeException).Namespace ||
//				    type.Name == typeof(DuplicateWaitObjectException).Name && type.Namespace == typeof(DuplicateWaitObjectException).Namespace) {
//					canAddParameterName = objectCreateExpression.Arguments.Count == 1;
//					if (objectCreateExpression.Arguments.Count >= 1) {
//						paramNode = objectCreateExpression.Arguments.FirstOrDefault();
//						if (objectCreateExpression.Arguments.Count == 2) {
//							altParam = objectCreateExpression.Arguments.ElementAt(1);
//							if (!context.Resolve(altParam).Type.IsKnownType(KnownTypeCode.String))
//								paramNode = null;
//						}
//						if (objectCreateExpression.Arguments.Count == 3)
//							altParam = objectCreateExpression.Arguments.ElementAt(2);
//					}
//					return paramNode != null;
//				}
//				return false;
//			}
//
//			static List<string> GetValidParameterNames(ObjectCreateExpression objectCreateExpression)
//			{
//				var names = new List<string>();
//				var node = objectCreateExpression.Parent;
//				while (node != null && !(node is TypeDeclaration) && !(node is AnonymousTypeCreateExpression)) {
//					var lambda = node as LambdaExpression;
//					if (lambda != null)
//						names.AddRange(lambda.Parameters.Select(p => p.Name));
//					var anonymousMethod = node as AnonymousMethodExpression;
//					if (anonymousMethod != null)
//						names.AddRange(anonymousMethod.Parameters.Select(p => p.Name));
//
//					var indexer = node as IndexerDeclaration;
//					if (indexer != null) {
//						names.AddRange(indexer.Parameters.Select(p => p.Name));
//						break;
//					}
//
//					var methodDeclaration = node as MethodDeclaration;
//					if (methodDeclaration != null) {
//						names.AddRange(methodDeclaration.Parameters.Select(p => p.Name));
//						break;
//					}
//
//					var constructorDeclaration = node as ConstructorDeclaration;
//					if (constructorDeclaration != null) {
//						names.AddRange(constructorDeclaration.Parameters.Select(p => p.Name));
//						break;
//					}
//					var accessor = node as Accessor;
//					if (accessor != null) {
//						if (accessor.Role == PropertyDeclaration.SetterRole ||
//						    accessor.Role == CustomEventDeclaration.AddAccessorRole ||
//						    accessor.Role == CustomEventDeclaration.RemoveAccessorRole) {
//							names.Add("value");
//						}
//						break;
//					}
//
//					node = node.Parent;
//				}
//				return names;
//			}
//
//			string GetParameterName(Expression expr)
//			{
//				foreach (var node in expr.DescendantsAndSelf) {
//					if (!(node is Expression))
//						continue;
//					var rr = context.Resolve(node) as LocalResolveResult;
//					if (rr != null && rr.Variable.SymbolKind == SymbolKind.Parameter)
//						return rr.Variable.Name;
//				}
//				return null;
//			}
//
//			string GuessParameterName(ObjectCreateExpression objectCreateExpression, List<string> validNames)
//			{
//				if (validNames.Count == 1)
//					return validNames[0];
//				var parent = objectCreateExpression.GetParent<IfElseStatement>();
//				if (parent == null)
//					return null;
//				return GetParameterName(parent.Condition);
//			}
//
//			public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
//			{
//				base.VisitObjectCreateExpression(objectCreateExpression);
//
//				Expression paramNode;
//				Expression altParamNode;
//				bool canAddParameterName;
//				if (!CheckExceptionType(objectCreateExpression, out paramNode, out altParamNode, out canAddParameterName))
//					return;
//
//				var paramName = GetArgumentParameterName(paramNode);
//				if (paramName == null)
//					return;
//				var validNames = GetValidParameterNames(objectCreateExpression);
//
//				if (!validNames.Contains(paramName)) {
//					// Case 1: Parameter name is swapped
//					var altParamName = GetArgumentParameterName(altParamNode);
//					if (altParamName != null && validNames.Contains(altParamName)) {
//						AddIssue(new CodeIssue(
//							paramNode,
//							string.Format(context.TranslateString(""), paramName),
//							context.TranslateString("."),
//							script => {
//								var newAltNode = paramNode.Clone();
//								script.Replace(paramNode, altParamNode.Clone());
//								script.Replace(altParamNode, newAltNode);
//							}
//						));
//						AddIssue(new CodeIssue(
//							altParamNode,
//							context.TranslateString("The parameter name is on the wrong argument."),
//							context.TranslateString("Swap parameter."),
//							script => {
//								var newAltNode = paramNode.Clone();
//								script.Replace(paramNode, altParamNode.Clone());
//								script.Replace(altParamNode, newAltNode);
//							}
//						));
//						return;
//					}
//					var guessName = GuessParameterName(objectCreateExpression, validNames);
//					if (guessName != null) {
//
//						var actions = new List<CodeAction>();
//						
//						actions.Add(new CodeAction (
//							string.Format(context.TranslateString("Replace with '\"{0}\"'."), guessName),
//							script => {
//								script.Replace(paramNode, new PrimitiveExpression(guessName));
//							}, paramNode
//						)); 
//
//						if (canAddParameterName) {
//							actions.Add(new CodeAction(
//								string.Format(context.TranslateString("Add '\"{0}\"' parameter."), guessName),
//								script => {
//									var oce = (ObjectCreateExpression)objectCreateExpression.Clone();
//									oce.Arguments.Clear();
//									oce.Arguments.Add(new PrimitiveExpression(guessName));
//									oce.Arguments.Add(objectCreateExpression.Arguments.First().Clone());
//									script.Replace(objectCreateExpression, oce);
//								}, paramNode
//							)); 
//						}
//
//						AddIssue(new CodeIssue(
//							paramNode,
//							string.Format(context.TranslateString("The parameter '{0}' can't be resolved"), paramName),
//							actions
//						));
//						return;
//					}
//
//					// General case: mark only
//					AddIssue(new CodeIssue(
//						paramNode,
//						string.Format(context.TranslateString("The parameter '{0}' can't be resolved"), paramName)
//					));
//				}
//			}
		}
	}

	[ExportCodeFixProvider(NotResolvedInTextIssue.DiagnosticId, LanguageNames.CSharp)]
	public class NotResolvedInTextIssueFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return NotResolvedInTextIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan);
				//if (!node.IsKind(SyntaxKind.BaseList))
				//	continue;
				var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Swap parameter", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}