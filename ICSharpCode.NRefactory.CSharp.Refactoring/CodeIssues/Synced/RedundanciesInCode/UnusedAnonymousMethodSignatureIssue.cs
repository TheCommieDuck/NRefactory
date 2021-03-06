// 
// UnusedAnonymousMethodSignatureIssue.cs
// 
// Author:
//      Luís Reis <luiscubal@gmail.com>
//      Mike Krüger <mkrueger@xamarn.com>
//
// Copyright (c) 2013 Luís Reis
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
	[ExportDiagnosticAnalyzer("Anonymous method signature is not required", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "UnusedAnonymousMethodSignature")]
	public class UnusedAnonymousMethodSignatureIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "UnusedAnonymousMethodSignatureIssue";
		const string Description            = "Detects when no delegate parameter is used in the anonymous method body.";
		const string MessageFormat          = "Specifying signature is redundant because no parameter is used";
		const string Category               = IssueCategories.RedundanciesInCode;

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

		class GatherVisitor : GatherVisitorBase<UnusedAnonymousMethodSignatureIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

//			bool IsParameterListRedundant(Expression expression)
//			{
//				var validTypes = TypeGuessing.GetValidTypes(ctx.Resolver, expression);
//				return validTypes.Count(t => t.Kind == TypeKind.Delegate) == 1;
//			}
//
//			public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
//			{
//				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
//				if (!anonymousMethodExpression.HasParameterList || !IsParameterListRedundant(anonymousMethodExpression))
//					return;
//
//				var parameters = anonymousMethodExpression.Parameters.ToList();
//				if (parameters.Count > 0) {
//					var usageAnalysis = new ConvertToConstantIssue.VariableUsageAnalyzation(ctx);
//					anonymousMethodExpression.Body.AcceptVisitor(usageAnalysis); 
//					foreach (var parameter in parameters) {
//						var rr = ctx.Resolve(parameter) as LocalResolveResult;
//						if (rr == null)
//							continue;
//						if (usageAnalysis.GetStatus(rr.Variable) != ICSharpCode.NRefactory6.CSharp.Refactoring.ExtractMethod.VariableState.None)
//							return;
//					}
//				}
//
//				AddIssue(new CodeIssue(anonymousMethodExpression.LParToken.StartLocation,
//					anonymousMethodExpression.RParToken.EndLocation,
//					ctx.TranslateString(""),
//					ctx.TranslateString(""),
//					script => {
//						int start = script.GetCurrentOffset(anonymousMethodExpression.DelegateToken.EndLocation);
//						int end = script.GetCurrentOffset(anonymousMethodExpression.Body.StartLocation);
//
//						script.Replace(start, end - start, " ");
//					}) { IssueMarker = IssueMarker.GrayOut });
//			}
		}
	}

	[ExportCodeFixProvider(UnusedAnonymousMethodSignatureIssue.DiagnosticId, LanguageNames.CSharp)]
	public class UnusedAnonymousMethodSignatureFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return UnusedAnonymousMethodSignatureIssue.DiagnosticId;
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
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Remove redundant signature", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}