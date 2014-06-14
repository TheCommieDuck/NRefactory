using ICSharpCode.NRefactory6.CSharp.Refactoring;
//
// ConvertPropertyToMethodTests.cs
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
using NUnit.Framework;
namespace ICSharpCode.NRefactory6.CSharp.CodeActions
{
    [TestFixture]
    public class ConvertPropertyToMethodTests : ContextActionTestBase
    {
        //getter
        //setter
        //getter and setter
        //ignore autoproperty
        [Test]
                public void TestGet()
        {
            Test<ConvertPropertyToMethodAction>(@"
class Foo
{
    private int bar;

    public int $Bar
    {
        get
        {
            return bar+5;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        int bar = foo.Bar;
    }
}
", @"
class Foo
{
    public int GetBar(int index)
    {
        return bar+index;
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        int bar = foo.GetBar(4);
    }
}
");
        }

        [Test]
        public void TestSet()
        {
            Test<ConvertPropertyToMethodAction>(@"
class Foo
{
    private int bar;

    public int $Bar
    {
        set
        {
            bar = value;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.Bar = 5;
    }
}
", @"
class Foo
{
    public void SetBar(int value)
    {
        bar = value;
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.SetBar(5);
    }
}
");
        }

        [Test]
        public void TestGetAndSetIndexer()
        {
            Test<ConvertPropertyToMethodAction>(@"
class Foo
{
    private int bar;

    public int $Bar
    {
        get
        {
            return bar;
        }

        set
        {
            bar = value;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.Bar = 5;
        int baz = foo.Bar;
    }
}
", @"
class Foo
{
    public int GetBar()
    {
        return bar;
    }

    public void SetBar(int value)
    {
        bar = value;
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.SetBar(5);
        int baz = foo.GetBar();
    }
}
");
        }

        [Test]
        public void IgnoreAutoProperty()
        {
            TestWrongContext<ConvertPropertyToMethodAction>(@"
class Foo
{
    public int $Bar { get; set; }
}
");
        }
    }
}
