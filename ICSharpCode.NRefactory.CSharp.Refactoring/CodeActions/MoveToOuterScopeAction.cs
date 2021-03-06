//
// MoveToOuterScopeAction.cs
//
// Author:
//       Simon Lindgren <simon.n.lindgren@gmail.com>
//
// Copyright (c) 2012 Simon Lindgren
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
	[NRefactoryCodeRefactoringProvider(Description = "Moves a declaration to the parent scope")]
	[ExportCodeRefactoringProvider("Move to outer scope", LanguageNames.CSharp)]
	public class MoveToOuterScopeAction : ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync(cancellationToken);
			var root = await model.SyntaxTree.GetRootAsync(cancellationToken);
			return null;
		}
//		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
//		{
//			var variableDeclaration = context.GetNode<VariableDeclarationStatement>();
//			if (variableDeclaration == null)
//				yield break;
//			var entryNode = FindCurrentScopeEntryNode(variableDeclaration);
//			if (entryNode == null)
//				yield break;
//			var selectedInitializer = context.GetNode<VariableInitializer>();
//			if (selectedInitializer != null) {
//				if (!selectedInitializer.NameToken.Contains(context.Location))
//					yield break;
//				if (HasDependency(context, entryNode, selectedInitializer)) {
//					yield return MoveDeclarationAction(context, entryNode, variableDeclaration, selectedInitializer);
//				} else {
//					yield return MoveInitializerAction(context, entryNode, variableDeclaration, selectedInitializer);
//				}
//			} else {
//				if (!variableDeclaration.Type.Contains(context.Location) || variableDeclaration.Variables.Count <= 1)
//					yield break;
//				yield return new CodeAction(context.TranslateString("Move declaration to outer scope"), script => {
//					script.Remove(variableDeclaration);
//					script.InsertBefore(entryNode, variableDeclaration.Clone());
//				}, variableDeclaration);
//			}
//		}
//
//		static CodeAction MoveInitializerAction(SemanticModel context, AstNode insertAnchor,
//		                                        VariableDeclarationStatement declaration, VariableInitializer initializer)
//		{
//			var type = declaration.Type.Clone();
//			var name = initializer.Name;
//			return new CodeAction(context.TranslateString("Move initializer to outer scope"), script =>  {
//				if (declaration.Variables.Count != 1) {
//					var innerDeclaration = RemoveInitializer(declaration, initializer);
//					script.InsertBefore(declaration, innerDeclaration);
//				}
//				script.Remove(declaration);
//				var outerDeclaration = new VariableDeclarationStatement(type, name, initializer.Initializer.Clone());
//				script.InsertBefore(insertAnchor, outerDeclaration);
//			}, initializer.NameToken);
//		}
//
//		static CodeAction MoveDeclarationAction(SemanticModel context, AstNode insertAnchor,
//		                                        VariableDeclarationStatement declarationStatement, VariableInitializer initializer)
//		{
//			var type = declarationStatement.Type.Clone();
//			var name = initializer.Name;
//			
//			return new CodeAction(context.TranslateString("Move declaration to outer scope"), script =>  {
//				script.InsertBefore(declarationStatement, new ExpressionStatement {
//					Expression = new AssignmentExpression(new IdentifierExpression(name), initializer.Initializer.Clone())
//				});
//				script.Remove(declarationStatement);
//				script.InsertBefore(insertAnchor, new VariableDeclarationStatement(type, name, Expression.Null));
//			}, initializer.NameToken);
//		}
//
//		bool HasDependency(SemanticModel context, AstNode firstSearchNode, AstNode targetNode)
//		{
//			var referenceFinder = new FindReferences();
//			var identifiers = targetNode.Descendants
//				.Where(n => n is IdentifierExpression)
//					.Select<AstNode, IdentifierExpression>(node => (IdentifierExpression)node);
//			foreach (var identifier in identifiers) {
//				var resolveResult = context.Resolve(identifier);
//				var localResolveResult = resolveResult as LocalResolveResult;
//				if (localResolveResult == null)
//					continue;
//				bool referenceFound = false;
////				var variable = localResolveResult.Variable;
//				var syntaxTree = context.RootNode as SyntaxTree;
//				referenceFinder.FindLocalReferences(localResolveResult.Variable, context.UnresolvedFile, syntaxTree,
//				                                    context.Compilation, (node, nodeResolveResult) => {
//					if (node.StartLocation > firstSearchNode.StartLocation && node.EndLocation < targetNode.StartLocation)
//						referenceFound = true;
//				}, CancellationToken.None);
//				if (referenceFound)
//					return true;
//			}
//			return false;
//		}
//
//		static VariableDeclarationStatement RemoveInitializer(VariableDeclarationStatement variableDeclarationStatement, VariableInitializer selectedVariableInitializer)
//		{
//			var newVariableDeclarationStatement = new VariableDeclarationStatement() {
//				Type = variableDeclarationStatement.Type.Clone()
//			};
//			foreach (var variableInitializer in variableDeclarationStatement.Variables) {
//				if (variableInitializer != selectedVariableInitializer) {
//					newVariableDeclarationStatement.AddChild((VariableInitializer)variableInitializer.Clone(), Roles.Variable);
//				}
//			}
//			return newVariableDeclarationStatement;
//		}
//
//		AstNode FindCurrentScopeEntryNode(Statement startNode)
//		{
//			// Start one node up in the tree, otherwise we may stop at the BlockStatement
//			// of the current scope instead of moving up to the enclosing scope
//			var currentNode = startNode.Parent;
//			AstNode lastNode;
//			do {
//				lastNode = currentNode;
//				currentNode = currentNode.Parent;
//				if (currentNode == null)
//					return null;
//			} while (!(currentNode is BlockStatement));
//			return lastNode;
//		}
	}
}

