//
// ReplaceWithSimpleAssignmentIssue.cs
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
	[ExportDiagnosticAnalyzer("Replace with simple assignment", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "ReplaceWithSimpleAssignment")]
	public class ReplaceWithSimpleAssignmentIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "ReplaceWithSimpleAssignmentIssue";
		const string Description            = "Replace with simple assignment";
		const string MessageFormat          = "Replace with simple assignment";
		const string Category               = IssueCategories.PracticesAndImprovements;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Info, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}

		class GatherVisitor : GatherVisitorBase<ReplaceWithSimpleAssignmentIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

			public override void VisitBinaryExpression(BinaryExpressionSyntax node)
			{
				base.VisitBinaryExpression(node);
				//ignore non |= and &=
				if (node.IsKind(SyntaxKind.OrAssignmentExpression)) {
					LiteralExpressionSyntax right = node.Right as LiteralExpressionSyntax;
					//if right is true
					if (right != null && (bool)right.Token.Value) {
						AddIssue(Diagnostic.Create(Rule, node.GetLocation()));
					}

				} else if (node.IsKind(SyntaxKind.AndAssignmentExpression)) {
					LiteralExpressionSyntax right = node.Right as LiteralExpressionSyntax;
					//if right is false
					if (right != null && !(bool)right.Token.Value) {
						AddIssue(Diagnostic.Create(Rule, node.GetLocation()));
					}
				}

			}
		}
	}

	[ExportCodeFixProvider(ReplaceWithSimpleAssignmentIssue.DiagnosticId, LanguageNames.CSharp)]
	public class ReplaceWithSimpleAssignmentFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return ReplaceWithSimpleAssignmentIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan) as BinaryExpressionSyntax;
				if (node == null)
					continue;
				var newRoot = root.ReplaceNode(node, node.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.EqualsToken)));
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Replace with '{0}'", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}