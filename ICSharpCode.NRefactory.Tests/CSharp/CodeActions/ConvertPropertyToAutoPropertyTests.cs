//
// ConvertPropertyToAutoPropertyTests.cs
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
    public class ConvertPropertyToAutoPropertyTests : ContextActionTestBase
    {
        //ignore properties with logic in them
        //get only
        //set only
        //get and set
        //member variable referenced elsewhere in class
        [Test]
        public void IgnorePropertiesWithAdditionalLogic()
        {
            TestWrongContext<ConvertPropertyToAutoPropertyAction>(@"
class Foo
{
    public int $Bar
    {
        get
        {
            int bar = 5;
            return baz+bar;
        }
    }
    private int bar;
}
");
        }

        [Test]
        public void TestGetter()
        {
            Test<ConvertPropertyToAutoPropertyAction>(@"
class Foo
{
    public int $Bar
    {
        get
        {
            return bar;
        }
    }

    private int bar;
}
", @"
class Foo
{
    public int Bar { get; }
}");
        }

        [Test]
        public void TestSetter()
        {
            Test<ConvertPropertyToAutoPropertyAction>(@"
class Foo
{
    public int $Bar
    {
        set
        {
            bar = value;
        }
    }

    private int bar;
}
", @"
class Foo
{
    public int Bar { set; }
}");
        }

        [Test]
        public void TestGetterAndSetter()
        {
            Test<ConvertPropertyToAutoPropertyAction>(@"
class Foo
{
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

    private int bar;
}
", @"
class Foo
{
    public int Bar { get; set; }
}");
        }

        [Test]
        public void TestUnderlyingMemberReferenced()
        {
            Test<ConvertPropertyToAutoPropertyAction>(@"
class Foo
{
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

    private int bar;

    public void Baz()
    {
        int b = bar;
    }
}
", @"
class Foo
{
    public int Bar { get; set; }

    public void Baz()
    {
        int b = Bar;
    }
}");
        }
    }
}
