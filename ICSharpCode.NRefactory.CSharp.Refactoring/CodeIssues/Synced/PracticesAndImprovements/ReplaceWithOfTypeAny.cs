//
// ReplaceWithOfTypeAny.cs
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
	[ExportDiagnosticAnalyzer("Replace with OfType<T>().Any()", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "ReplaceWithOfType.Any")]
	public class ReplaceWithOfTypeAnyIssue : GatherVisitorCodeIssueProvider
	{
//		static readonly AstNode selectPattern =
//			new InvocationExpression(
//				new MemberReferenceExpression(
//					new InvocationExpression(
//						new MemberReferenceExpression(new AnyNode("targetExpr"), "Select"),
//						new LambdaExpression {
//							Parameters = { PatternHelper.NamedParameter ("param1", PatternHelper.AnyType ("paramType", true), Pattern.AnyString) },
//							Body = PatternHelper.OptionalParentheses (new AsExpression(new AnyNode("expr1"), PatternHelper.AnyType("type")))
//						}
//					), 
//					Pattern.AnyString
//				),
//				new LambdaExpression {
//					Parameters = { PatternHelper.NamedParameter ("param2", PatternHelper.AnyType ("paramType", true), Pattern.AnyString) },
//					Body = PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode("expr2"), BinaryOperatorType.InEquality, new NullReferenceExpression())
//				}
//			);
//
//		static readonly AstNode selectPatternWithFollowUp =
//			new InvocationExpression(
//				new MemberReferenceExpression(
//					new InvocationExpression(
//						new MemberReferenceExpression(new AnyNode("targetExpr"), "Select"),
//						new LambdaExpression {
//							Parameters = { PatternHelper.NamedParameter ("param1", PatternHelper.AnyType ("paramType", true), Pattern.AnyString) },
//							Body = PatternHelper.OptionalParentheses (new AsExpression(new AnyNode("expr1"), PatternHelper.AnyType("type")))
//						}
//					),	 
//					Pattern.AnyString
//				),
//				new NamedNode("lambda", 
//					new LambdaExpression {
//						Parameters = { PatternHelper.NamedParameter ("param2", PatternHelper.AnyType ("paramType", true), Pattern.AnyString) },
//						Body = new BinaryOperatorExpression(
//							PatternHelper.CommutativeOperatorWithOptionalParentheses(new AnyNode("expr2"), BinaryOperatorType.InEquality, new NullReferenceExpression()),
//							BinaryOperatorType.ConditionalAnd,
//							new AnyNode("followUpExpr")
//						)
//					}
//				)
//			);


		internal const string DiagnosticId  = "ReplaceWithOfTypeAnyIssue";
		const string Description            = "Replace with call to OfType<T>().Any()";
		const string MessageFormat          = "Replace with OfType<T>().Any()";
		const string Category               = IssueCategories.PracticesAndImprovements;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor<ReplaceWithOfTypeAnyIssue>(semanticModel, addDiagnostic, cancellationToken, "Any");
		}

		internal class GatherVisitor<T> : GatherVisitorBase<T> where T : GatherVisitorCodeIssueProvider
		{
			readonly string member;

			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken, string member)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
				this.member = member;
			}
//
//			bool CheckParameterMatches(IEnumerable<INode> param, IEnumerable<INode> expr)
//			{
//				var p = param.Single() as ParameterDeclaration;
//				var e = expr.Single();
//
//				if (p == null)
//					return false;
//				if (e is IdentifierExpression)
//					return p.Name == ((IdentifierExpression)e).Identifier;
//				return false;
//			}
//
//			public override void VisitInvocationExpression (InvocationExpression anyInvoke)
//			{
//				base.VisitInvocationExpression (anyInvoke);
//
//				var anyResolve = ctx.Resolve (anyInvoke) as InvocationResolveResult;
//				if (anyResolve == null || !HasPredicateVersion(anyResolve.Member))
//					return;
//				Match match;
//
//				if (member != "Where") {
//					match = selectPattern.Match(anyInvoke);
//					if (match.Success) {
//						if (!ReplaceWithOfTypeIssue.CheckParameterMatches(match.Get("param1"), match.Get("expr1")) ||
//							!ReplaceWithOfTypeIssue.CheckParameterMatches(match.Get("param2"), match.Get("expr2")))
//							return;
//						AddIssue(new CodeIssue(
//							anyInvoke,
//							string.Format(ctx.TranslateString("Replace with OfType<T>().{0}()"), member),
			//							string.Format(ctx.TranslateString("Replace with call to OfType<T>().{0}()"), member),
//							script => {
//							var target = match.Get<Expression>("targetExpr").Single().Clone();
//							var type = match.Get<AstType>("type").Single().Clone();
//							script.Replace(
//								anyInvoke,
//								new InvocationExpression(
//								new MemberReferenceExpression(
//								new InvocationExpression(new MemberReferenceExpression(target, "OfType", type)),
//								member
//							)
//							)
//							);
//						}
//						));
//						return;
//					}
//				}
//
//				match = selectPatternWithFollowUp.Match (anyInvoke);
//				if (match.Success) {
//					if (!ReplaceWithOfTypeIssue.CheckParameterMatches(match.Get("param1"), match.Get("expr1")) ||
//					    !ReplaceWithOfTypeIssue.CheckParameterMatches(match.Get("param2"), match.Get("expr2")))
//						return;
//					AddIssue(new CodeIssue(
//						anyInvoke,
			//						string.Format(ctx.TranslateString("Replace with OfType<T>().{0}()"), member),
//						string.Format(ctx.TranslateString("Replace with call to OfType<T>().{0}()"), member),
//						script => {
//							var target = match.Get<Expression>("targetExpr").Single().Clone();
//							var type = match.Get<AstType>("type").Single().Clone();
//							var lambda = match.Get<LambdaExpression>("lambda").Single();
//							script.Replace(
//								anyInvoke,
//								new InvocationExpression(
//									new MemberReferenceExpression(
//										new InvocationExpression(new MemberReferenceExpression(target, "OfType", type)),
//										member
//									),
//									new LambdaExpression {
//										Parameters = { (ParameterDeclaration)lambda.Parameters.First().Clone() },
//										Body = match.Get<Expression>("followUpExpr").Single().Clone()
//									}
//								)
//							 );
//						}
//					));
//					return;
//				}
//			}
//
//			bool IsQueryExtensionClass(ITypeDefinition typeDef)
//			{
//				if (typeDef == null || typeDef.Namespace != "System.Linq")
//					return false;
//				switch (typeDef.Name) {
//					case "Enumerable":
//						case "ParallelEnumerable":
//						case "Queryable":
//						return true;
//						default:
//						return false;
//				}
//			}
//
//			bool HasPredicateVersion(IParameterizedMember member)
//			{
//				if (!IsQueryExtensionClass(member.DeclaringTypeDefinition))
//					return false;
//				return member.Name == this.member;
//			}
		}
	}

	[ExportCodeFixProvider(ReplaceWithOfTypeAnyIssue.DiagnosticId, LanguageNames.CSharp)]
	public class ReplaceWithOfTypeAnyFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return ReplaceWithOfTypeAnyIssue.DiagnosticId;
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
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Replace with call to OfType<T>().Any()", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}