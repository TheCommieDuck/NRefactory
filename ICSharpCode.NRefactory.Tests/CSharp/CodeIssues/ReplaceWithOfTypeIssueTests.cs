//
// ReplaceWithOfTypeIssueTests.cs
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
using NUnit.Framework;
using ICSharpCode.NRefactory6.CSharp.Refactoring;
using ICSharpCode.NRefactory6.CSharp.CodeActions;
using System.Linq;

namespace ICSharpCode.NRefactory6.CSharp.CodeIssues
{
	[TestFixture]
	public class ReplaceWithOfTypeIssueTests : InspectionActionTestBase
	{
		[Test]
		public void TestCaseSelectNotNull ()
		{
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull((object o) => o as Test);
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => o as Test);
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
		}

		[Test]
		public void TestCaseSelectNotNullWithParentheses ()
		{
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => ((o as Test)));
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => o as Test);
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
		}


		[Test]
		public void TestCaseSelectWhereCase1 ()
		{
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Where(o => o is Test).Select (o => o as Test);
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => o as Test);
	}
}",@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
		}

		[Test]
		public void TestCaseSelectWhereGabage ()
		{
			Analyze<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Where(o => o is Test).Select (x => o as Test);
	}
}");
			Analyze<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => null as Test);
	}
}");
		}


		[Test]
		public void TestCaseSelectWhereCase2WithParens ()
		{
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.Where(o => (o is Test)).Select (o => ((Test)(o)));
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
			Test<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.SelectNotNull(o => o as Test);
	}
}", @"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		obj.OfType<Test> ();
	}
}");
		}



		[Test]
		public void TestDisable ()
		{
			Analyze<ReplaceWithOfTypeIssue>(@"using System.Linq;
class Test
{
	public void Foo(object[] obj)
	{
		// ReSharper disable once ReplaceWithOfType
		obj.SelectNotNull((object o) => o as Test);
	}
}");
		}



	}
	
}

