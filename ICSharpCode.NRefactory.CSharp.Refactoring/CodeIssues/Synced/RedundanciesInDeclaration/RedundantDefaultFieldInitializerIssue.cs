﻿// 
// RedundantDefaultFieldInitializerIssue.cs
// 
// Author:
//      Mansheng Yang <lightyang0@gmail.com>
// 
// Copyright (c) 2012 Mansheng Yang <lightyang0@gmail.com>
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
	[ExportDiagnosticAnalyzer("Redundant field initializer", LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "RedundantDefaultFieldInitializer")]
	public class RedundantDefaultFieldInitializerIssue : GatherVisitorCodeIssueProvider
	{
		internal const string DiagnosticId  = "RedundantDefaultFieldInitializerIssue";
		const string Description            = "Initializing field with default value is redundant.";
		const string MessageFormat          = "Initializing field by default value is redundant";
		const string Category               = IssueCategories.RedundanciesInDeclarations;

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

		class GatherVisitor : GatherVisitorBase<RedundantDefaultFieldInitializerIssue>
		{
			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}

//			public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
//			{
//				base.VisitFieldDeclaration(fieldDeclaration);
//				if (fieldDeclaration.HasModifier(Modifiers.Const) || fieldDeclaration.HasModifier(Modifiers.Readonly))
//					return;
//				var defaultValueExpr = GetDefaultValueExpression(fieldDeclaration.ReturnType);
//				if (defaultValueExpr == null)
//					return;
//
//				foreach (var variable1 in fieldDeclaration.Variables) {
//					var variable = variable1;
//					if (!defaultValueExpr.Match(variable.Initializer).Success)
//						continue;
//
//					AddIssue(new CodeIssue(variable.Initializer, ctx.TranslateString(""),
//					         new CodeAction(ctx.TranslateString(""),
//					                         script => script.Replace(variable, new VariableInitializer(variable.Name)),
//							variable.Initializer)) { IssueMarker = IssueMarker.GrayOut });
//				}
//			}
//
//			Expression GetDefaultValueExpression(AstType astType)
//			{
//				var type = ctx.ResolveType(astType);
//
//				if ((type.IsReferenceType ?? false) || type.Kind == TypeKind.Dynamic)
//					return new NullReferenceExpression();
//
//				var typeDefinition = type.GetDefinition();
//				if (typeDefinition != null) {
//					switch (typeDefinition.KnownTypeCode) {
//						case KnownTypeCode.Boolean:
//							return new PrimitiveExpression(false);
//
//						case KnownTypeCode.Char:
//							return new PrimitiveExpression('\0');
//
//						case KnownTypeCode.SByte:
//						case KnownTypeCode.Byte:
//						case KnownTypeCode.Int16:
//						case KnownTypeCode.UInt16:
//						case KnownTypeCode.Int32:
//							return new PrimitiveExpression(0);
//
//						case KnownTypeCode.Int64:
//							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0L) };
//						case KnownTypeCode.UInt32:
//							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0U) };
//						case KnownTypeCode.UInt64:
//							return new Choice {
//								new PrimitiveExpression(0), new PrimitiveExpression(0U), new PrimitiveExpression(0UL)
//							};
//						case KnownTypeCode.Single:
//							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0F) };
//						case KnownTypeCode.Double:
//							return new Choice {
//								new PrimitiveExpression(0), new PrimitiveExpression(0F), new PrimitiveExpression(0D)
//							};
//						case KnownTypeCode.Decimal:
//							return new Choice { new PrimitiveExpression(0), new PrimitiveExpression(0M) };
//
//						case KnownTypeCode.NullableOfT:
//							return new NullReferenceExpression();
//					}
//					if (type.Kind == TypeKind.Struct)
//						return new ObjectCreateExpression(astType.Clone());
//				}
//				return new DefaultValueExpression(astType.Clone());
//			}
		}
	}

	[ExportCodeFixProvider(RedundantDefaultFieldInitializerIssue.DiagnosticId, LanguageNames.CSharp)]
	public class RedundantDefaultFieldInitializerFixProvider : ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			yield return RedundantDefaultFieldInitializerIssue.DiagnosticId;
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
				result.Add(CodeActionFactory.Create(node.Span, diagonstic.Severity, "Remove field initializer", document.WithSyntaxRoot(newRoot)));
			}
			return result;
		}
	}
}