﻿//
// AddNewModifierToMethodTests.cs
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
    public class AddNewModifierToMethodTests : ContextActionTestBase
    {
        //ignore non-derived classes
        //ignore non-hiding methods (so where it's not needed)
        //simple method
        //method with parameters

        [Test]
        public void IgnoreNonDerivedClass()
        {
            TestWrongContext<AddNewModifierToMethodAction>(@"
class Foo
{
    public void $Bar()
    {
    }
}");
        }

        [Test]
        public void IgnoreNonHidingMethod()
        {
            TestWrongContext<AddNewModifierToMethodAction>(@"
class Foo
{
    public virtual void Bar()
    {
    }
}

class Baz : Foo
{
    public void $Bar_()
    {
    }
}");
        }

        [Test]
        public void SimpleMethod()
        {
            Test<AddNewModifierToMethodAction>(@"
class Foo
{
    public virtual void Bar()
    {
    }
}

class Baz : Foo
{
    public void $Bar()
    {
    }
}", @"
class Foo
{
    public virtual void Bar()
    {
    }
}

class Baz : Foo
{
    public new void Bar()
    {
    }
}");
        }

        [Test]
        public void MethodWithParameters()
        {
            Test<AddNewModifierToMethodAction>(@"
class Foo
{
    public virtual void Bar(int testParam)
    {
    }
}

class Baz : Foo
{
    public void $Bar(int testParam)
    {
    }
}", @"
class Foo
{
    public virtual void Bar(int testParam)
    {
    }
}

class Baz : Foo
{
    public new void Bar(int testParam)
    {
    }
}");
        }

        [Test]
        public void FurtherUpInheritanceTree()
        {
            Test<AddNewModifierToMethodAction>(@"
class Foo
{
    public virtual void Bar(int testParam)
    {
    }
}

class Bar : Foo
{
}

class Baz : Bar
{
    public void $Bar(int testParam)
    {
        int i = 5 + testParam;
    }
}", @"
class Foo
{
    public virtual void Bar(int testParam)
    {
    }
}

class Bar : Foo
{
}

class Baz : Bar
{
    public new void Bar(int testParam)
    {
        int i = 5 + testParam;
    }
}");
        }

    }
}
