//
// SealedMemberInSealedClassIssue.cs
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
	[ExportDiagnosticAnalyzer("Sealed member in sealed class", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "SealedMemberInSealedClass")]
	public class SealedMemberInSealedClassIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "SealedMemberInSealedClassIssue";
		const string Description            = "'sealed' modifier is redundant in sealed classes";
		const string MessageFormat          = "Keyword 'sealed' is redundant in sealed classes.";
		const string Category               = IssueCategories.RedundanciesInDeclarations;

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

		class GatherVisitor : GatherVisitorBase<SealedMemberInSealedClassIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

			public bool HasIssue(SyntaxNode node)
			{
				var type = node.Parent as TypeDeclarationSyntax;
				if (type == null || !type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
					return false;
				return semanticModel.GetDeclaredSymbol(node).IsSealed;
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				base.VisitMethodDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
				
			}

			public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
			{
				base.VisitFieldDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Declaration.GetLocation()));
			}

			public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
				base.VisitIndexerDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.ThisKeyword.GetLocation()));
			}

			public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
			{
				base.VisitPropertyDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitEventDeclaration(EventDeclarationSyntax node)
			{
				base.VisitEventDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				base.VisitConstructorDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
			{
				base.VisitOperatorDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.OperatorKeyword.GetLocation()));
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				base.VisitClassDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitStructDeclaration(StructDeclarationSyntax node)
			{
				base.VisitStructDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
			{
				base.VisitInterfaceDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}

			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
			{
				base.VisitDelegateDeclaration(node);
				if (HasIssue(node))
					AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}
		}
	}

	[ExportCodeFixProvider(SealedMemberInSealedClassIssue.DiagnosticId, LanguageNames.CSharp)]
	public class SealedMemberInSealedClassFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return SealedMemberInSealedClassIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan);
				//if (!node.IsKind(SyntaxKind.BaseList))
				//	continue;
				var newRoot = root.ReplaceNode(node, RedundantPrivateIssue.RemoveModifierFromNode(node, SyntaxKind.SealedKeyword));
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Remove redundant 'sealed' modifier", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}

