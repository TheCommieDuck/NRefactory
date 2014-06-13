using ICSharpCode.NRefactory6.CSharp.Refactoring;
//
// ConvertIndexerToMethodTests.cs
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
    public class ConvertIndexerToMethodTests : ContextActionTestBase
    {
        //get
        //set
        //get AND set

        [Test]
        //TODO: MOCKUP
        public void TestGetIndexer()
        {
            Test<ConvertIndexerToMethodAction>(@"
class Foo
{
    private int bar;

    public int $this[int index]
    {
        get
        {
            return bar+index;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        int bar = foo[4];
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
        public void TestSetIndexer()
        {
            Test<ConvertIndexerToMethodAction>(@"
class Foo
{
    private int bar;

    public int $this[int index]
    {
        set
        {
            bar = index;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo[4] = 5;
    }
}
", @"
class Foo
{
    private int bar;

    public int SetBar(int index, int value)
    {
        bar = index;
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.SetBar(4, 5);
    }
}
");
        }

        [Test]
        public void TestGetAndSetIndexer()
        {
            Test<ConvertIndexerToMethodAction>(@"
class Foo
{
    private int bar;

    public int $this[int index]
    {
        get
        {
            return bar+index;
        }

        set
        {
            bar = index;
        }
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo[4] = 5;
        int bar = foo[4];
    }
}
", @"
class Foo
{
    public int GetBar(int index)
    {
        return bar+index;
    }

    public int SetBar(int index, int value)
    {
        bar = index;
    }
}

class Bar
{
    public void Foo()
    {
        Foo foo = new Foo();
        foo.SetBar(4, 5);
        int bar = foo.GetBar(4);
    }
}
");
        }
    }
}
