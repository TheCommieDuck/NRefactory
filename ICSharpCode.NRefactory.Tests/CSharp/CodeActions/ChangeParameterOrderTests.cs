//
// ChangeParameterOrderTests.cs
//
// Author:
//       Mark Garnett <mg10g13@soton.ac.uk>
//
// Copyright (c) 2014 Mark Garnett (mg10g13@soton.ac.uk)
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

using ICSharpCode.NRefactory6.CSharp.Refactoring;
using NUnit.Framework;

namespace ICSharpCode.NRefactory6.CSharp.CodeActions
{
    [TestFixture]
    public class ChangeParameterOrderTests : ContextActionTestBase
    {
        //not firing on parameterless methods
        //not firing on 1 parameter
        //works with 2 parameters
        //multiple (many) parameters

        [Test]
        public void IgnoreParameterlessMethod()
        {
            TestWrongContext<ChangeParameterOrderAction>(@"
class Foo
{
    public $Bar()
    {
    }
}
");
        }

        [Test]
        public void IgnoreSingleParameterMethod()
        {
            TestWrongContext<ChangeParameterOrderAction>(@"
class Foo
{
    public $Bar(int foo)
    {
    }
}
");
        }

        [Test]
        //TODO: consider using some kind of mockup here?
        public void TestSimpleMethod()
        {
            Test<ChangeParameterOrderAction>(@"
class Foo
{
    public $Bar(int foo, System.String bar)
    {
    }
}

class Bar
{
    public Bar()
    {
        Foo foo = new Foo();
        foo(30, ""string"");
    }
}
", @"
class Foo
{
    public Bar(System.String bar, int foo)
    {
    }
}

class Bar
{
    public Bar()
    {
        Foo foo = new Foo();
        foo(""string"", 30);
    }
}
");
        }

        [Test]

        public void TestMultipleParameterMethod()
        {
            //TODO: consider mockup
            Test<ChangeParameterOrderAction>(@"
class Foo
{
    public $Bar(int foo, System.String bar, bool bar)
    {
    }
}

class Bar
{
    public Bar()
    {
        Foo foo = new Foo();
        foo(30, ""string"", true);
    }
}
", @"
class Foo
{
    public Bar(System.String bar, bool bar, int foo)
    {
    }
}

class Bar
{
    public Bar()
    {
        Foo foo = new Foo();
        foo(""string"", true, 30);
    }
}
");
        }
    }
}
