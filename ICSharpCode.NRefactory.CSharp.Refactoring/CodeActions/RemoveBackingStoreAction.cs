﻿// 
// RemoveBackingStore.cs
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
using System;
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
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	[NRefactoryCodeRefactoringProvider(Description = "Removes the backing store of a property and creates an auto property")]
	[ExportCodeRefactoringProvider("Remove backing store for property", LanguageNames.CSharp)]
	public class RemoveBackingStoreAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);

			SyntaxToken token = root.FindToken(span.Start);
			var property = token.Parent as PropertyDeclarationSyntax;
			if (property == null || !property.Identifier.Span.Contains(span))
				return Enumerable.Empty<CodeAction>();

			var field = GetBackingField(model, property);
			if (!IsValidField(field, property.Parent as TypeDeclarationSyntax))
				return Enumerable.Empty<CodeAction>();

			//variable declarator->declaration->field declaration
			var backingFieldNode = root.FindNode(field.Locations.First().SourceSpan).Ancestors().OfType<FieldDeclarationSyntax>().First();

			// create new auto property
			var accessorDeclList = new SyntaxList<AccessorDeclarationSyntax>()
				.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
				.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
			var newProperty = property.WithAccessorList(SyntaxFactory.AccessorList(accessorDeclList))
				.WithTrailingTrivia(property.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation);

			var propertyAnnotation = new SyntaxAnnotation();
			var fieldAnnotation = new SyntaxAnnotation();

			//annotate our property node and our field node
			root = root.ReplaceNode(property, property.WithAdditionalAnnotations(propertyAnnotation));
			root = root.ReplaceNode(root.FindNode(backingFieldNode.Span), backingFieldNode.WithAdditionalAnnotations(fieldAnnotation));

			return new[] { CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, "Convert to auto property", 
                PerformAction(document, model, root, field.Name, newProperty, propertyAnnotation, fieldAnnotation)) };
		}

		private Document PerformAction(Document document, SemanticModel model, SyntaxNode root, String name,
			PropertyDeclarationSyntax newProperty, SyntaxAnnotation propAnno, SyntaxAnnotation fieldAnno)
		{
			var oldField = root.GetAnnotatedNodes(fieldAnno).First() as FieldDeclarationSyntax;
			if (oldField.Declaration.Variables.Count == 1) {
				var newRoot = root.RemoveNode(oldField, SyntaxRemoveOptions.KeepNoTrivia);
				var oldProperty = newRoot.GetAnnotatedNodes(propAnno).First();
				newRoot = newRoot.ReplaceNode(oldProperty, newProperty);

				return document.WithSyntaxRoot(newRoot);
			} else {
				FieldDeclarationSyntax newField = oldField.WithDeclaration(SyntaxFactory.VariableDeclaration(oldField.Declaration.Type));
				//need to replace the field with one missing the variable field
				foreach (var variable in oldField.Declaration.Variables) {
					if (!variable.Identifier.ValueText.Equals(name))
						newField = newField.AddDeclarationVariables(variable);
				}
				var newRoot = root.ReplaceNode(oldField, newField.WithAdditionalAnnotations(Formatter.Annotation));
				var oldProperty = newRoot.GetAnnotatedNodes(propAnno).First();
				newRoot = newRoot.ReplaceNode(oldProperty, newProperty.WithAdditionalAnnotations(Formatter.Annotation));
				return document.WithSyntaxRoot(newRoot);
			}
		}

		internal static IFieldSymbol GetBackingField(SemanticModel model, PropertyDeclarationSyntax property)
		{
			var getter = property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
			var setter = property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

			// automatic properties always need getter & setter
			if (property == null || getter == null || setter == null || getter.Body == null || setter.Body == null)
				return null;
			//todo: check version?
			if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)) || property.Parent is InterfaceDeclarationSyntax)
				return null;
			var getterField = ScanGetter(model, getter);
			if (getterField == null)
				return null;
			var setterField = ScanSetter(model, setter);
			if (setterField == null)
				return null;
			if (!getterField.Equals(setterField))
				return null;
			return getterField;
		}

		internal static IFieldSymbol ScanGetter(SemanticModel model, AccessorDeclarationSyntax getter)
		{
			if (getter == null || getter.Body == null || getter.Body.Statements.Count != 1) //no getter/get;/get we can't easily work out
				return null;
			var retStatement = getter.Body.Statements.First() as ReturnStatementSyntax;
			if (retStatement == null)
				return null;
			if (!IsPossibleExpression(retStatement.Expression))
				return null;
			var retSymbol = model.GetSymbolInfo(retStatement.Expression).Symbol;
			return ((IFieldSymbol)retSymbol);
		}

		internal static IFieldSymbol ScanSetter(SemanticModel model, AccessorDeclarationSyntax setter)
		{
			if (setter == null || setter.Body == null || setter.Body.Statements.Count != 1) //no getter/get;/get we can't easily work out
				return null;
			var setAssignment = setter.Body.Statements.First().ChildNodes().OfType<ExpressionSyntax>().First();
			var assignment = setAssignment != null ? setAssignment as BinaryExpressionSyntax : null;
			if (assignment == null || !assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
				return null;
			var id = assignment.Right as IdentifierNameSyntax;
			if (id == null || id.Identifier.ValueText != "value")
				return null;
			if (!IsPossibleExpression(assignment.Left))
				return null;
			var retSymbol = model.GetSymbolInfo(assignment.Left).Symbol;
			return ((IFieldSymbol)retSymbol);

		}

		internal static bool IsPossibleExpression(ExpressionSyntax left)
		{
			if (left is IdentifierNameSyntax)
				return true;
			var mr = left as MemberAccessExpressionSyntax;
			if (mr == null)
				return false;
			return mr.Expression is ThisExpressionSyntax;
		}

		internal static bool IsValidField(IFieldSymbol field, TypeDeclarationSyntax type)
		{
			if (field == null || field.GetAttributes().Count() > 0)
				return false;
			foreach (var m in type.Members.OfType<FieldDeclarationSyntax>()) {
				foreach (var i in m.Declaration.Variables) {
					if (i.SpanStart == field.Locations.First().SourceSpan.Start) {
						if (i.Initializer != null)
							return false;
						break;
					}
				}
			}
			return true;
		}
	}
}

