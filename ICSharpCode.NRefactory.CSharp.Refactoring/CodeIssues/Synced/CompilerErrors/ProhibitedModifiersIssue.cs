//
// ProhibitedModifiersIssue.cs
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
	[ExportDiagnosticAnalyzer("Checks for prohibited modifiers", LanguageNames.CSharp)]
	public class ProhibitedModifiersIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "ProhibitedModifiersIssue";
		const string Description            = "Checks for prohibited modifiers";
		const string MessageFormat          = "Static constructors can't have any other modifier";
		const string Category               = IssueCategories.CompilerErrors;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}

		class GatherVisitor : GatherVisitorBase<ProhibitedModifiersIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

//			readonly Stack<TypeDeclaration> curType = new Stack<TypeDeclaration> ();
//			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
//			{
//				curType.Push(typeDeclaration); 
//				base.VisitTypeDeclaration(typeDeclaration);
//				curType.Pop();
//			}
//
//			void AddStaticRequiredError (EntityDeclaration entity, AstNode node)
//			{
//				AddIssue(new CodeIssue(
//					node,
//					ctx.TranslateString("'static' modifier is required inside a static class"),
//					ctx.TranslateString("Add 'static' modifier"), 
//					s => {
//						s.ChangeModifier(entity, (entity.Modifiers & ~(Modifiers.Virtual | Modifiers.Override)) | Modifiers.Static);
//					}
//				));
//			}
//
//			void CheckStaticRequired(EntityDeclaration entity)
//			{
//				if (!curType.Peek().HasModifier(Modifiers.Static) || entity.HasModifier(Modifiers.Static) || entity.HasModifier(Modifiers.Const))
//					return;
//				var fd = entity as FieldDeclaration;
//				if (fd != null) {
//					foreach (var init in fd.Variables)
//						AddStaticRequiredError(entity, init.NameToken);
//					return;
//				}
//
//				var ed = entity as EventDeclaration;
//				if (ed != null) {
//					foreach (var init in ed.Variables)
//						AddStaticRequiredError(entity, init.NameToken);
//					return;
//				}
//
//				AddStaticRequiredError(entity, entity.NameToken);
//			}
//
//			void CheckVirtual(EntityDeclaration entity)
//			{
//				if (!curType.Peek().HasModifier(Modifiers.Static) && !curType.Peek().HasModifier(Modifiers.Sealed) && entity.HasModifier(Modifiers.Virtual)) {
//					if (!entity.HasModifier(Modifiers.Public) && !entity.HasModifier(Modifiers.Protected)  && !entity.HasModifier(Modifiers.Internal)) {
//						AddIssue(new CodeIssue(
//							entity.NameToken,
//							ctx.TranslateString("'virtual' members can't be private")
//						));
//						return;
//					}
//
//				}
//
//				if (entity.HasModifier(Modifiers.Sealed) && !entity.HasModifier(Modifiers.Override)) {
//					AddIssue(new CodeIssue(
//						entity.ModifierTokens.First(t => t.Modifier == Modifiers.Sealed),
//						ctx.TranslateString("'sealed' modifier is not usable without override"),
//						ctx.TranslateString("Remove 'sealed' modifier"), 
//						s => {
//							s.ChangeModifier(entity, entity.Modifiers & ~Modifiers.Sealed);
//						}
//					));
//
//				}
//
//				if (!curType.Peek().HasModifier(Modifiers.Sealed) || !entity.HasModifier(Modifiers.Virtual))
//					return;
//				AddIssue(new CodeIssue(
//					entity.ModifierTokens.First(t => t.Modifier == Modifiers.Virtual),
//					ctx.TranslateString("'virtual' modifier is not usable in a sealed class"),
//					ctx.TranslateString("Remove 'virtual' modifier"), 
//					s => s.ChangeModifier(entity, entity.Modifiers & ~Modifiers.Virtual)
//				));
//			}
//
//			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
//			{
//				CheckStaticRequired(methodDeclaration);
//				CheckVirtual(methodDeclaration);
//				if (methodDeclaration.IsExtensionMethod) {
//					var parent = methodDeclaration.Parent as TypeDeclaration;
//					if (parent != null && !parent.HasModifier(Modifiers.Static)) {
//						var actions = new List<CodeAction>();
//						var token = methodDeclaration.Parameters.First().FirstChild;
//						actions.Add(new CodeAction(
//							ctx.TranslateString("Make class 'static'"),
//							s => s.ChangeModifier(parent, (parent.Modifiers & ~Modifiers.Sealed) | Modifiers.Static),
//							token
//							));
//
//						actions.Add(new CodeAction(
//							ctx.TranslateString("Remove 'this'"),
//							s => s.ChangeModifier(methodDeclaration.Parameters.First(), ParameterModifier.None),
//							token
//						));
//
//						AddIssue(new CodeIssue(
//							token,
//							ctx.TranslateString("Extension methods are only allowed in static classes"),
//							actions
//						));
//					}
//				}
//
//				base.VisitMethodDeclaration(methodDeclaration);
//			}
//
//			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
//			{
//				CheckStaticRequired(fieldDeclaration);
//				base.VisitFieldDeclaration(fieldDeclaration);
//			}
//
//			public override void VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
//			{
//				CheckStaticRequired(fixedFieldDeclaration);
//				base.VisitFixedFieldDeclaration(fixedFieldDeclaration);
//			}
//
//			public override void VisitEventDeclaration(EventDeclaration eventDeclaration)
//			{
//				CheckStaticRequired(eventDeclaration);
//				base.VisitEventDeclaration(eventDeclaration);
//			}
//
//			public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
//			{
//				CheckStaticRequired(eventDeclaration);
//				CheckVirtual(eventDeclaration);
//				base.VisitCustomEventDeclaration(eventDeclaration);
//			}
//
//			public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
//			{
//				CheckStaticRequired(constructorDeclaration);
//
//				if (constructorDeclaration.HasModifier(Modifiers.Static)) {
//					if ((constructorDeclaration.Modifiers & Modifiers.Static) != 0) {
//						foreach (var mod in constructorDeclaration.ModifierTokens) {
//							if (mod.Modifier == Modifiers.Static)
//								continue;
//							AddIssue(new CodeIssue(
//								mod,
//								ctx.TranslateString(""),
//								ctx.TranslateString(""), 
//								s => {
//									s.ChangeModifier(constructorDeclaration, Modifiers.Static);
//								}
//							));
//						}
//					}
//				}
//				base.VisitConstructorDeclaration(constructorDeclaration);
//			}
//
//			public override void VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
//			{
//				base.VisitDestructorDeclaration(destructorDeclaration);
//			}
//
//			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
//			{
//				CheckStaticRequired(propertyDeclaration);
//				CheckVirtual(propertyDeclaration);
//				base.VisitPropertyDeclaration(propertyDeclaration);
//			}
//
//			public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
//			{
//				CheckVirtual(indexerDeclaration);
//				base.VisitIndexerDeclaration(indexerDeclaration);
//			}
//
//			public override void VisitBlockStatement(BlockStatement blockStatement)
//			{
//				// SKIP
//			}
		}
	}

	[ExportCodeFixProvider(ProhibitedModifiersIssue.DiagnosticId, LanguageNames.CSharp)]
	public class ProhibitedModifiersFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return ProhibitedModifiersIssue.DiagnosticId;
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
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Remove prohibited modifier", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}