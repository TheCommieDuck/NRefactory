//
// UnassignedReadonlyFieldIssue.cs
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
	[ExportDiagnosticAnalyzer("Unassigned readonly field", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "UnassignedReadonlyField.Compiler", PragmaWarning = 649)]
	public class UnassignedReadonlyFieldIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "UnassignedReadonlyFieldIssue";
		const string Description            = "Unassigned readonly field";
		const string MessageFormat          = "Readonly field is never assigned";
		const string Category               = IssueCategories.CompilerWarnings;

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

		class GatherVisitor : GatherVisitorBase<UnassignedReadonlyFieldIssue>
		{
			//readonly Stack<List<Tuple<VariableInitializer, IVariable>>> fieldStack = new Stack<List<Tuple<VariableInitializer, IVariable>>>();

			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

//			void Collect()
//			{
//				foreach (var varDecl in fieldStack.Peek()) {
//					var resolveResult = ctx.Resolve(varDecl.Item1) as MemberResolveResult;
//					if (resolveResult == null || resolveResult.IsError)
//						continue;
//					AddIssue(new CodeIssue(
//						varDecl.Item1.NameToken,
//						string.Format(ctx.TranslateString(""), varDecl.Item1.Name),
//						ctx.TranslateString(""),
//						script => {
//						script.InsertWithCursor(
//							ctx.TranslateString("Create constructor"),
//							resolveResult.Member.DeclaringTypeDefinition,
//							(s, c) => {
//							return new ConstructorDeclaration {
//								Name = resolveResult.Member.DeclaringTypeDefinition.Name,
//								Modifiers = Modifiers.Public,
//								Body = new BlockStatement {
//										new AssignmentExpression(
//											new MemberReferenceExpression(new ThisReferenceExpression(), varDecl.Item1.Name),
//											new IdentifierExpression(varDecl.Item1.Name)
//										)
//								},
//								Parameters = {
//									new ParameterDeclaration(c.CreateShortType(resolveResult.Type), varDecl.Item1.Name)
//								}
//							};
//						}
//						);
//					}
//					));
//				}
//			}
//
//			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
//			{
//				var list = new List<Tuple<VariableInitializer, IVariable>>();
//				fieldStack.Push(list);
//				foreach (var fieldDeclaration in ConvertToConstantIssue.CollectFields(this, typeDeclaration)) {
//					if (!fieldDeclaration.HasModifier(Modifiers.Readonly))
//						continue;
////					var rr = ctx.Resolve(fieldDeclaration.ReturnType);
//				
//					if (fieldDeclaration.Variables.Count() > 1)
//						continue;
//					if (!fieldDeclaration.Variables.First().Initializer.IsNull)
//						continue;
//					var variable = fieldDeclaration.Variables.First();
//					var mr = ctx.Resolve(variable) as MemberResolveResult;
//					if (mr == null)
//						continue;
//					list.Add(Tuple.Create(variable, mr.Member as IVariable)); 
//				}
//				base.VisitTypeDeclaration(typeDeclaration);
//				Collect();
//				fieldStack.Pop();
//			}
//
//			public override void VisitBlockStatement(BlockStatement blockStatement)
//			{
//				var assignmentAnalysis = new ConvertToConstantIssue.VariableUsageAnalyzation(ctx);
//				var newVars = new List<Tuple<VariableInitializer, IVariable>>();
//				blockStatement.AcceptVisitor(assignmentAnalysis); 
//				foreach (var variable in fieldStack.Pop()) {
//					if (assignmentAnalysis.GetStatus(variable.Item2) == VariableState.Changed)
//						continue;
//					newVars.Add(variable);
//				}
//				fieldStack.Push(newVars);
//			}
		}
	}

	[ExportCodeFixProvider(UnassignedReadonlyFieldIssue.DiagnosticId, LanguageNames.CSharp)]
	public class UnassignedReadonlyFieldFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return UnassignedReadonlyFieldIssue.DiagnosticId;
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
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Initialize field from constructor parameter", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}