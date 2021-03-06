//
// NonReadonlyReferencedInGetHashCodeIssue.cs
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
	[ExportDiagnosticAnalyzer("Non-readonly field referenced in 'GetHashCode()'", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "NonReadonlyReferencedInGetHashCode")]
	public class NonReadonlyReferencedInGetHashCodeIssue : GatherVisitorCodeIssueProvider
	{	
		internal const string DiagnosticId  = "NonReadonlyReferencedInGetHashCodeIssue";
		const string Description            = "Non-readonly field referenced in 'GetHashCode()'";
		const string MessageFormat          = "Non-readonly field referenced in 'GetHashCode()'";
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

		class GatherVisitor : GatherVisitorBase<NonReadonlyReferencedInGetHashCodeIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

			#region Skip these
			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
			}

			public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
			{
			}

			public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
			{
			}

			public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
			{
			}

			public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
			{
			}

			public override void VisitEventDeclaration(EventDeclarationSyntax node)
			{
			}

			public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
			{
			}

			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
			{
			}

			#endregion

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				IMethodSymbol method = semanticModel.GetDeclaredSymbol(node);
				if (method == null || method.Name != "GetHashCode" || !method.IsOverride || method.Parameters.Count() > 0)
					return;
				if (method.ReturnType.SpecialType != SpecialType.System_Int32)
					return;
				base.VisitMethodDeclaration(node);
			}


			//note that visiting both identifier AND memberaccess would double up on issues - r.r.a in TestInspector4 would give issues for r, r.r, and r.r.a.
			public override void VisitIdentifierName(IdentifierNameSyntax node)
			{
				base.VisitIdentifierName(node);
				CheckNode(node, node.GetLocation());
			}

			private void CheckNode(SyntaxNode node, Location location)
			{
				var symbol = semanticModel.GetSymbolInfo(node).Symbol as IFieldSymbol;
				if (symbol == null)
					return;
				if (!symbol.IsReadOnly && !symbol.IsConst) {
					AddIssue(Diagnostic.Create(Rule, location));
					Console.WriteLine(location.ToString());
				}

			}
		}
	}

	[ExportCodeFixProvider(NonReadonlyReferencedInGetHashCodeIssue.DiagnosticId, LanguageNames.CSharp)]
	public class NonReadonlyReferencedInGetHashCodeIssueFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return NonReadonlyReferencedInGetHashCodeIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan);
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, diagonstic.GetMessage(), document));
			}
			return result;
		}
	}
}