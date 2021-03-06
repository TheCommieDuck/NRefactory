﻿// 
// UseExplicitType.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2011 Mike Krüger <mkrueger@novell.com>
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
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using Microsoft.CodeAnalysis.CSharp;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[NRefactoryCodeRefactoringProvider(Description = "Converts local variable declaration to be explicit typed.")]
	[ExportCodeRefactoringProvider("Use explicit type", LanguageNames.CSharp)]
	public class UseExplicitTypeAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			var token = root.FindToken(span.Start);
			var varDecl = UseVarKeywordAction.GetVariableDeclarationStatement(token.Parent);
			ITypeSymbol type;
			TypeSyntax typeSyntax;
			if (varDecl != null) {
				var v = varDecl.Variables.FirstOrDefault();
				if (v == null || v.Initializer == null) 
					return Enumerable.Empty<CodeAction> ();
				type = model.GetTypeInfo(v.Initializer.Value).Type;
				typeSyntax = varDecl.Type;
			} else {
				var foreachStatement = UseVarKeywordAction.GetForeachStatement(token.Parent);
				if (foreachStatement == null) {
					return Enumerable.Empty<CodeAction> ();
				}
				type = model.GetTypeInfo(foreachStatement.Type).Type;
				typeSyntax = foreachStatement.Type;
			}

			if (type == null || !typeSyntax.IsVar || type.TypeKind == TypeKind.Error || type.TypeKind == TypeKind.Unknown)
				return Enumerable.Empty<CodeAction> ();
			if (!(type.SpecialType != SpecialType.System_Nullable_T && type.TypeKind != TypeKind.Unknown && !ContainsAnonymousType(type))) {
				return Enumerable.Empty<CodeAction> ();
			}
			return new[] {  CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, "Use explicit type", PerformAction (document, model, root, type, typeSyntax)) };
		}

		static Document PerformAction(Document document, SemanticModel model, SyntaxNode root, ITypeSymbol type, TypeSyntax typeSyntax)
		{
			var newRoot = root.ReplaceNode(
				typeSyntax,
				SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(model, typeSyntax.SpanStart))
				.WithLeadingTrivia(typeSyntax.GetLeadingTrivia())
				.WithTrailingTrivia(typeSyntax.GetTrailingTrivia())
			);
			return document.WithSyntaxRoot(newRoot);
		}

		static bool ContainsAnonymousType (ITypeSymbol type)
		{
			if (type.TypeKind == TypeKind.ArrayType && ContainsAnonymousType(((IArrayTypeSymbol)type).ElementType))
				return true;
			return type.TypeKind == TypeKind.DynamicType || type.IsAnonymousType;
		}
	}
}

