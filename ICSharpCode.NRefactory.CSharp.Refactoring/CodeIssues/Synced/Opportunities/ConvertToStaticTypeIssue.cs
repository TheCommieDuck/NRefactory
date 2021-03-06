﻿// 
// ConvertToStaticTypeIssue.cs
//  
// Author:
//       Ji Kun <jikun.nus@gmail.com>
// 
// Copyright (c) 2013  Ji Kun <jikun.nus@gmail.com>
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
	[ExportDiagnosticAnalyzer("Class can be converted to static", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "ConvertToStaticType")]
	public class ConvertToStaticTypeIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId = "ConvertToStaticTypeIssue";
		const string Description = "If all fields, properties and methods members are static, the class can be made static.";
		const string MessageFormat = "This class is recommended to be defined as static";
		const string Category = IssueCategories.Opportunities;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Info, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}


		class GatherVisitor : GatherVisitorBase<ConvertToStaticTypeIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base(semanticModel, addDiagnostic, cancellationToken)
			{
			}

			internal static bool IsMainMethod(IMethodSymbol m)
			{
				return (m.ReturnType.SpecialType == SpecialType.System_Int32 || m.ReturnType.SpecialType == SpecialType.System_Void) && m.IsStatic && m.Name.Equals("Main");
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				base.VisitClassDeclaration(node);

				ITypeSymbol classType = semanticModel.GetDeclaredSymbol(node);
				if (!node.Modifiers.Any() || node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) || classType.IsAbstract || classType.IsStatic)
					return;
				//ignore implicitly declared (e.g. default ctor)
				if (classType.GetMembers().Where(m => !(m is ITypeSymbol)).Any(f => (!f.IsStatic && !f.IsImplicitlyDeclared) || (f is IMethodSymbol && IsMainMethod((IMethodSymbol)f))))
					return;

				AddIssue(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
			}
		}
	}

	[ExportCodeFixProvider(ConvertToStaticTypeIssue.DiagnosticId, LanguageNames.CSharp)]
	public class ConvertToStaticTypeFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return ConvertToStaticTypeIssue.DiagnosticId;
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			foreach (var diagonstic in diagnostics) {
				var node = root.FindNode(diagonstic.Location.SourceSpan) as ClassDeclarationSyntax;
				if (node == null)
					continue;
				var sealedMod = node.Modifiers.Where(m => m.IsKind(SyntaxKind.SealedKeyword)).FirstOrDefault();
				var newRoot = root.ReplaceNode(node, node.WithModifiers(node.Modifiers.Remove(sealedMod)
					.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")))));
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Make class static", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}