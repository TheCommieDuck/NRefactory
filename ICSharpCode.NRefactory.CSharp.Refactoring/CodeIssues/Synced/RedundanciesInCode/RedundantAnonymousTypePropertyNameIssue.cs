//
// RedundantAnonymousTypePropertyNameIssue.cs
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[DiagnosticAnalyzer]
	[ExportDiagnosticAnalyzer("Redundant anonymous type property namen", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzerAttribute(AnalysisDisableKeyword = "RedundantAnonymousTypePropertyName")]
	public class RedundantAnonymousTypePropertyNameIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "RedundantAnonymousTypePropertyNameIssue";
		const string Category               = IssueCategories.RedundanciesInCode;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, "The name can be inferred from the initializer expression", "Redundant explicit property name", Category, DiagnosticSeverity.Warning, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}

		class GatherVisitor : GatherVisitorBase<RedundantAnonymousTypePropertyNameIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

			static string GetAnonymousTypePropertyName(SyntaxNode expr)
			{
				var mAccess = expr as MemberAccessExpressionSyntax;
				return mAccess != null ? mAccess.Name.ToString() : expr.ToString();
			}

			public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
			{
				base.VisitAnonymousObjectCreationExpression(node);

				foreach (var expr in node.Initializers) {
					if (expr.NameEquals == null || expr.NameEquals.Name == null)
						continue;

					if (expr.NameEquals.Name.ToString() == GetAnonymousTypePropertyName(expr.Expression)) {
						AddIssue (Diagnostic.Create(Rule, expr.NameEquals.GetLocation()));
					}
				}
			}
		}
	}

	[ExportCodeFixProvider(RedundantAnonymousTypePropertyNameIssue.DiagnosticId, LanguageNames.CSharp)]
	public class RedundantAnonymousTypePropertyNameFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return RedundantAnonymousTypePropertyNameIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan);
				if (node.IsKind(SyntaxKind.NameEquals)) {
					var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
					result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Remove redundant name", document.WithSyntaxRoot(newRoot)));
				}
			}
			return result;
		}
	}
}