﻿//
// ConstructFixer.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
using ICSharpCode.NRefactory.Editor;
using System.Text;
using System.Reflection;
using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
	abstract class ConstructCompleter
	{
		public abstract bool TryFix (ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset);

		protected AstNode GetLastNonErrorChild (AstNode node)
		{
			var lastNode = node.LastChild;

			while (lastNode is ErrorNode) {
				lastNode = lastNode.GetPrevNode(FormattingVisitor.NoWhitespacePredicate);
			}
			return lastNode;
		}
	}

	class TypeDeclarationCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var typeDeclaration = syntaxTree.GetNodeAt<TypeDeclaration>(location); 
			if (typeDeclaration != null) {
				if (typeDeclaration.LBraceToken.IsNull && typeDeclaration.RBraceToken.IsNull) {
					if (typeDeclaration.Members.Any())
						return false;
					var lastNode = GetLastNonErrorChild (typeDeclaration);
					if (lastNode == null)
						return false;
					var insertionOffset = document.GetOffset(lastNode.EndLocation);
					document.Insert(insertionOffset, fixer.GenerateBody (typeDeclaration, fixer.Options.ClassBraceStyle, false, ref newOffset));
					return true;
				}
			}
			return false;
		}
	}

	class DelegateDeclarationCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var typeDeclaration = syntaxTree.GetNodeAt<DelegateDeclaration>(location); 
			if (typeDeclaration != null) {
				if (typeDeclaration.RParToken.IsNull) {
					var lastNode = GetLastNonErrorChild (typeDeclaration);
					if (lastNode == null)
						return false;
					var insertionOffset = document.GetOffset(lastNode.EndLocation);
					document.Insert(insertionOffset, ");\n");
					newOffset += ");\n".Length;
					return true;
				}
			}
			return false;
		}
	}

	class MethodDeclarationCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var methodDeclaration = syntaxTree.GetNodeAt<MethodDeclaration>(location); 
			if (methodDeclaration != null) {
				if (!methodDeclaration.LParToken.IsNull && methodDeclaration.RParToken.IsNull) {
					var lastNode = GetLastNonErrorChild (methodDeclaration);
					if (lastNode == null)
						return false;

					var insertionOffset = document.GetOffset(lastNode.EndLocation);
					document.Insert(insertionOffset, ")\n\t{\t\t\n\t}");
					newOffset += ")\n\t{\t\t".Length;
					return true;
				}
			}
			return false;
		}
	}

	class IfStatementCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var ifStatement = syntaxTree.GetNodeAt<IfElseStatement>(location); 
			if (ifStatement != null) {
				if (!ifStatement.LParToken.IsNull && ifStatement.RParToken.IsNull) {
					var lastNode = GetLastNonErrorChild (ifStatement);
					if (lastNode == null)
						return false;

					var insertionOffset = document.GetOffset(lastNode.EndLocation);
					document.Insert(insertionOffset, fixer.GenerateBody (ifStatement, fixer.Options.StatementBraceStyle, true, ref newOffset));
					return true;
				}
			}
			return false;
		}
	}

	class SwitchStatementCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var switchStatement = syntaxTree.GetNodeAt<SwitchStatement>(location); 
			if (switchStatement != null) {
				if (!switchStatement.LParToken.IsNull && switchStatement.RParToken.IsNull) {
					var lastNode = GetLastNonErrorChild (switchStatement);
					if (lastNode == null)
						return false;

					var insertionOffset = document.GetOffset(lastNode.EndLocation);
					document.Insert(insertionOffset, fixer.GenerateBody (switchStatement, fixer.Options.StatementBraceStyle, true, ref newOffset));
					return true;
				}
			}
			return false;
		}
	}

	class InvocationCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var invocationExpression = syntaxTree.GetNodeAt<InvocationExpression>(location); 

			if (invocationExpression != null) {
				if (!invocationExpression.LParToken.IsNull && invocationExpression.RParToken.IsNull) {
					var lastNode = GetLastNonErrorChild (invocationExpression);
					if (lastNode == null)
						return false;

					var insertionOffset = document.GetOffset(lastNode.EndLocation);

					newOffset = insertionOffset;


					var text = ")";
					newOffset++;
					var expressionStatement = invocationExpression.Parent as ExpressionStatement;
					if (expressionStatement != null) {
						if (expressionStatement.SemicolonToken.IsNull)
							text = ");";
						newOffset ++;
					}
					document.Insert(insertionOffset, text);


					return true;
				}

			}
			return false;
		}
	}

	class ExpressionStatementCompleter : ConstructCompleter
	{
		public override bool TryFix(ConstructFixer fixer, SyntaxTree syntaxTree, IDocument document, TextLocation location, ref int newOffset)
		{
			var expressionStatement = syntaxTree.GetNodeAt<ExpressionStatement>(location); 

			if (expressionStatement != null) {
				int offset = document.GetOffset(expressionStatement.Expression.EndLocation);
				if (expressionStatement.SemicolonToken.IsNull) {
					document.Insert(offset, ";");
					newOffset = offset + 1;
				}
				return true;
			}
			return false;
		}
	}


	public class ConstructFixer
	{
		static readonly ConstructCompleter[] completer = {
			new TypeDeclarationCompleter(),
			new DelegateDeclarationCompleter (),
			new MethodDeclarationCompleter (),
			new IfStatementCompleter (),
			new SwitchStatementCompleter (),

			new InvocationCompleter (),
			new ExpressionStatementCompleter ()
		};

		readonly CSharpFormattingOptions options;

		public CSharpFormattingOptions Options {
			get {
				return options;
			}
		}

		public ConstructFixer(CSharpFormattingOptions options)
		{
			this.options = options;
		}
		

		string GetIndent(AstNode node)
		{
			if (node == null || node is SyntaxTree)
				return "";
			if (node is BlockStatement || node is TypeDeclaration || node is NamespaceDeclaration)
				return "\t" + GetIndent(node.Parent);
			return GetIndent(node.Parent);
		}

		internal string GenerateBody(AstNode node, BraceStyle braceStyle, bool addClosingBracket, ref int newOffset)
		{
			StringBuilder result = new StringBuilder();
			if (addClosingBracket)
				result.Append(")");
			var nodeIndent = GetIndent(node.Parent);
			switch (braceStyle) {
				case BraceStyle.DoNotChange:
				case BraceStyle.BannerStyle:
				case BraceStyle.EndOfLine:
					result.Append(" ");
					result.Append("{");
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					break;
				case BraceStyle.EndOfLineWithoutSpace:
					result.Append("{");
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					break;
				case BraceStyle.NextLine:
					result.Append("\n");
					result.Append(nodeIndent);
					result.Append("{");
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					break;
				case BraceStyle.NextLineShifted:
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					result.Append("{");
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					break;
				case BraceStyle.NextLineShifted2:
					result.Append("\n");
					result.Append(nodeIndent + "\t");
					result.Append("{");
					result.Append("\n");
					result.Append(nodeIndent + "\t" + "\t");
					break;
			}

			newOffset += result.Length;
			result.Append("\n");
			result.Append(nodeIndent);
			result.Append("}");

			return result.ToString();
		}

		public bool TryFix (IDocument document, int offset, out int newOffset)
		{
			newOffset = offset;

			var syntaxTree = SyntaxTree.Parse(document, "a.cs"); 
			var location = document.GetLocation(offset - 1);
			foreach (var c in completer) {
				if (c.TryFix(this, syntaxTree, document, location, ref newOffset)) {
					return true;
				}
			}
			return false;
		}
	}
}
