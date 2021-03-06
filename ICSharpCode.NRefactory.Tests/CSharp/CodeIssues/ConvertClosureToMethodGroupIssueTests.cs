//
// SimplifyAnonymousMethodToDelegateIssueTests.cs
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

namespace ICSharpCode.NRefactory6.CSharp.CodeIssues
{
	[Ignore("TODO - roslyn port")]
	[TestFixture]
	public class ConvertClosureToMethodGroupIssueTests : InspectionActionTestBase
	{
		[Test]
		public void TestSimpleVoidLambda ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => MyMethod (foo, bar)$;
	}
	void MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = MyMethod;
	}
	void MyMethod(int foo, int bar) {}
}");
		}
		
		[Test]
		public void TestSimpleBoolLambda ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, int, bool> action = $(foo, bar) => MyMethod (foo, bar)$;
	}
	bool MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, int, bool> action = MyMethod;
	}
	bool MyMethod(int foo, int bar) {}
}");
		}

		[Test]
		public void TestLambdaWithBody ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => { return MyMethod (foo, bar); }$;
	}
	void MyMethod(int foo, int bar) {}
}", @"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = MyMethod;
	}
	void MyMethod(int foo, int bar) {}
}");
		}

		[Test]
		public void Lambda_SwapParameterOrder ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int, int> action = $(foo, bar) => MyMethod (bar, foo);
	}
	void MyMethod(int foo, int bar) {}
}");
		}

		[Test]
		public void TestSimpleAnonymousMethod ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<int, int, int> action = $delegate(int foo, int bar) { return MyMethod (foo, bar); }$;
	}
}", @"using System;
class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<int, int, int> action = MyMethod;
	}
}");
		}

		[Test]
		public void TestSkipComplexCase ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
using System.Linq;

class Foo
{
	int MyMethod (int x, int y) { return x * y; }

	void Bar (string str)
	{
		Func<char[]> action = $() => str.Where (c => c != 'a').ToArray ();
	}
}");
		}

		[Test]
		public void CallInvolvesOptionalParameter ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	int MyMethod (int x, int y = 1) { return x * y; }

	void Bar (string str)
	{
		Func<int, int> action = $foo => MyMethod (foo);
	}
}");
		}

		[Test]
		public void CallExpandsParams ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	int MyMethod (params object[] args) { return 0; }

	void Bar (string str)
	{
		Func<string, int> action = $foo => MyMethod (foo);
	}
}");
		}

		/// <summary>
		/// Bug 12184 - Expression can be reduced to delegate fix can create ambiguity
		/// </summary>
		[Test]
		public void TestBug12184 ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
using System.Threading.Tasks;

class C
{
	public static C GetResponse () { return null; }

	public static void Foo ()
	{
		Task.Factory.StartNew (() => GetResponse());
	}
}");
/*			CheckFix (context, issues, @"using System;
using System.Threading.Tasks;

class C
{
	public static C GetResponse () { return null; }

	public static void Foo ()
	{
		Task.Factory.StartNew ((Func<C>)GetResponse);
	}
}");*/

		}

		[Test]
		public void Return_ReferenceConversion ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, object> action = $foo => MyMethod(foo)$;
	}
	string MyMethod(int foo) {}
}");
		}
		
		[Test]
		public void Return_BoxingConversion ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Func<int, object> action = $foo => MyMethod(foo)$;
	}
	bool MyMethod(int foo) {}
}");
		}
		
		[Test]
		public void Parameter_ReferenceConversion ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<string> action = $foo => MyMethod(foo)$;
	}
	void MyMethod(object foo) {}
}");
		}
		
		[Test]
		public void Parameter_BoxingConversion ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
class Foo
{
	void Bar (string str)
	{
		Action<int> action = $foo => MyMethod(foo)$;
	}
	void MyMethod(object foo) {}
}");
		}

		/// <summary>
		/// Bug 14759 - Lambda expression can be simplified to method group issue
		/// </summary>
		[Test]
		public void TestBug14759 ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"using System;
using System.Collections.Generic;

class C
{
	static bool DoStuff (int i)
	{
		return true;
	}
 
	public static void Main(string[] args)
	{
		List<int> list = new List<int> (2);
		list.Add (1);
		list.Add (3);
		list.ForEach (u => DoStuff (u));
	}
}");
		}

		[Test]
		public void TestTargetCollision ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"
using System;

class Program
{
	public void Foo(Action act) {}
	public void Foo(Action<int> act) {}

	void Test ()
	{
		Foo (i => Console.WriteLine (i));
	}
}");
		}

		/// <summary>
		/// Bug 15868 - Wrong context for Anonymous method can be simplified to method group
		/// </summary>
		[Test]
		public void TestBug15868 ()
		{
			Analyze<ConvertClosureToMethodGroupIssue>(@"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		Func<bool> b = () => bar ();
		Console.WriteLine (b());
	}
}
");
		}
		[Test]
		public void TestBug15868Case2 ()
		{
			Test<ConvertClosureToMethodGroupIssue>(@"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		FooBar b = () => bar ();
		Console.WriteLine (b());
	}
}
", @"
using System;

delegate bool FooBar ();

public class MyClass
{
	public static void Main ()
	{
		FooBar bar = () => true;
		FooBar b = bar;
		Console.WriteLine (b());
	}
}
");
		}

	}
}

