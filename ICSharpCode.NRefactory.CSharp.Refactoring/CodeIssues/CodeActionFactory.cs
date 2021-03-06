﻿//
// CodeActionFactory.cs
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
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ICSharpCode.NRefactory6.CSharp.Refactoring
{
	static class CodeActionFactory
	{
		public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Document changedDocument)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (changedDocument == null)
				throw new ArgumentNullException("changedDocument");
			return new DocumentChangeAction(textSpan, severity, description, ct => Task.FromResult<Document>(changedDocument));
		}

		public static CodeAction Create(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<Document>> createChangedDocument)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (createChangedDocument == null)
				throw new ArgumentNullException("createChangedDocument");
			return new DocumentChangeAction(textSpan, severity, description, createChangedDocument);
		}

		sealed class DocumentChangeAction : NRefactoryCodeAction
		{
			readonly string description;
			readonly Func<CancellationToken, Task<Document>> createChangedDocument;

			public override string Description {
				get {
					return description;
				}
			}

			public DocumentChangeAction(TextSpan textSpan, DiagnosticSeverity severity, string description, Func<CancellationToken, Task<Document>> createChangedDocument) : base(textSpan, severity)
			{
				this.description = description;
				this.createChangedDocument = createChangedDocument;
			}

			protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			{
				return createChangedDocument.Invoke(cancellationToken);
			}
		}
	}
}