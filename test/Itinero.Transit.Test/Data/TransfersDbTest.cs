// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NUnit.Framework;
using Itinero.Transit.Data;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the transfers db.
    /// </summary>
    [TestFixture]
    public class TransfersDbTest
    {
        /// <summary>
        /// Tests adding transfers.
        /// </summary>
        [Test]
        public void TestAddTransfer()
        {
            var db = new TransfersDb(256);

            db.AddTransfer(0, 1, 60);

            var enumeration = db.GetTransferEnumerator();
            Assert.IsTrue(enumeration.MoveTo(0));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);
            Assert.IsTrue(enumeration.MoveTo(1));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            db.AddTransfer(0, 2, 61);
            db.AddTransfer(1, 2, 62);
            db.AddTransfer(2, 4, 63);
            db.AddTransfer(2, 3, 64);
            db.AddTransfer(4, 5, 65);

            enumeration = db.GetTransferEnumerator();

            Assert.IsTrue(enumeration.MoveTo(0));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(61, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(1));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(62, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(2));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(61, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(62, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(63, enumeration.Seconds);
            Assert.AreEqual(4, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(64, enumeration.Seconds);
            Assert.AreEqual(3, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(4));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(63, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(65, enumeration.Seconds);
            Assert.AreEqual(5, enumeration.Stop);
        }
    }
}
