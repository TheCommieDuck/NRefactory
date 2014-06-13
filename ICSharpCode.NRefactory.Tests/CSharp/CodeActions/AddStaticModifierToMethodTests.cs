//
// AddStaticModifierToMethodTests.cs
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
    public class AddStaticModifierToMethodTests : ContextActionTestBase
    {
        //simple method
        //parameters
        //does not reference containing class
        //references containing class

        [Test]
        public void SimpleMethod()
        {
            Test<AddStaticModifierToMethodAction>(@"
class Foo
{
    public int $Bar()
    {
        return 4;
    }
}", @"
class Foo
{
    public static int Bar()
    {
        return 4;
    }
}
");
        }

        [Test]
        public void TestMethodWithParameters()
        {
            Test<AddStaticModifierToMethodAction>(@"
class Foo
{
    public int $Bar(int bar)
    {
        return bar;
    }
}", @"
class Foo
{
    public static int Bar(int bar)
    {
        return bar;
    }
}
");
        }

        [Test]
        public void ContainingClassNotReferenced()
        {
            Test<AddStaticModifierToMethodAction>(@"
class Foo
{
    public int $Bar()
    {
        return 4;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        foo.Bar();
    }
}
", @"
class Foo
{
    public static int $Bar()
    {
        return 4;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        Foo.Bar();
    }
}
");
        }

        [Test]
        public void AddInstanceVariablesAsParameters()
        {
            Test<AddStaticModifierToMethodAction>(@"
class Foo
{
    public int something = 4;

    public int $Bar()
    {
        return something;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        foo.Bar();
    }
}
", @"
class Foo
{
    public static int Bar()
    {
        return 4;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        Foo.Bar();
    }
}
");
        }

        [Test]
        public void ContainingClassReferenced()
        {
            Test<AddStaticModifierToMethodAction>(@"
class Foo
{
    public int something = 4;

    public int $Bar()
    {
        return something;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        foo.Bar();
    }
}
", @"
class Foo
{
    public int something = 4;

    public static int Bar(Foo foo, int something)
    {
        return something;
    }
}

class Bar
{
    public void Baz()
    {
        Foo foo = new Foo();
        Foo.Bar(foo, foo.something);
    }
}
");
        }
    }
}
