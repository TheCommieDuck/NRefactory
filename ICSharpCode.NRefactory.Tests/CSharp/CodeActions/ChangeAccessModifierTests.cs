// 
// ChangeAccessModifiersTests.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2013 Luís Reis
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

namespace ICSharpCode.NRefactory6.CSharp.CodeActions
{
	[TestFixture]
	public class ChangeAccessModifierTests : ContextActionTestBase
	{
		[Test]
		public void TestNoEnumMember()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
enum Test
{
	$Foo
}");
		}

		[Test]
		public void TestNoInterfaceMember()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
interface Test
{
	void $Foo ();
}");
		}

		[Test]
		public void TestNoExplicitInterfaceImplementationMember()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
interface Test
{
	void Foo ();
}
class TestClass : Test
{
	void $Test.Foo () {}
}");
		}

		[Test]
		public void TestNoOverrideMember()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
class TestClass : Test
{
	public override string $ToString()
	{
		return ""Test"";
	}
		
}");
		}

		[Test]
		public void TestType ()
		{
			Test<ChangeAccessModifierAction>(@"
class $Foo
{
}", @"
public class Foo
{
}");
		}

		[Test]
		public void TestMethodToProtected ()
		{
			Test<ChangeAccessModifierAction>(@"
class Foo
{
	void $Bar ()
	{
	}
}", @"
class Foo
{
	protected void Bar ()
	{
	}
}");
		}

		[Test]
		public void TestPrivateMethodToProtected ()
		{
			Test<ChangeAccessModifierAction>(@"
class Foo
{
	$private void Bar ()
	{
	}
}", @"
class Foo
{
	protected void Bar ()
	{
	}
}");
		}

		[Test]
		public void TestMethodToProtectedInternal ()
		{
			Test<ChangeAccessModifierAction>(@"
class Foo
{
	void $Bar ()
	{
	}
}", @"
class Foo
{
	protected internal void Bar ()
	{
	}
}", 1);
		}

		[Test]
		public void TestAccessor ()
		{
			Test<ChangeAccessModifierAction>(@"
class Foo
{
	public int Bar
	{
		get; $set;
	}
}", @"
class Foo
{
	public int Bar
	{
		get; private set;
	}
}");
		}

		[Test]
		public void TestStrictAccessor ()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
class Foo
{
	private int Bar
	{
		get; $set;
	}
}");
		}

		[Test]
		public void TestChangeAccessor ()
		{
			Test<ChangeAccessModifierAction>(@"
class Foo
{
	public int Bar
	{
		get; private $set;
	}
}", @"
class Foo
{
	public int Bar
	{
		get; protected set;
	}
}");
		}
	
		[Test]
		public void TestReturnTypeWrongContext()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
class Test
{
	public $void Foo () {}
}");
		}

		[Test]
		public void TestWrongModiferContext()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"
class Test
{
	public $virtual void Foo () {}
}");
		}

		[Test]
		public void TestMethodImplementingInterface()
		{
			TestWrongContext<ChangeAccessModifierAction>(@"using System;

class BaseClass : IDisposable
{
	public void $Dispose()
	{
	}
}");
		}


	}
}

