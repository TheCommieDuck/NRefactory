//
// CallToObjectEqualsViaBaseTests.cs
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
using NUnit.Framework;
using ICSharpCode.NRefactory6.CSharp.Refactoring;

namespace ICSharpCode.NRefactory6.CSharp.CodeIssues
{
	[TestFixture]
	public class CallToObjectEqualsViaBaseTests : InspectionActionTestBase
	{
		[Test]
		public void SimpleCase()
		{
			Analyze<CallToObjectEqualsViaBaseIssue>(@"
class Foo
{
	Foo()
	{
		bool b = $base.Equals(""blah"")$;
	}
}", @"
class Foo
{
	Foo()
	{
		bool b = object.ReferenceEquals(this, ""blah"");
	}
}");
		}
		
		[Test]
		public void NonObjectBase()
		{
			Analyze<CallToObjectEqualsViaBaseIssue>(@"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		bool b = $base.Equals(""blah"")$;
	}
}", @"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		bool b = object.ReferenceEquals(this, ""blah"");
	}
}");
		}
		
		[Test]
		public void IgnoresCallsToOtherObjects()
		{
			Analyze<CallToObjectEqualsViaBaseIssue>(@"
class Foo
{
}
class Bar : Foo
{
	void Baz ()
	{
		var foo1 = new Foo();
		var foo2 = new Foo();
		bool b = foo1.Equals(foo2);
	}
}");
		}
	}
}