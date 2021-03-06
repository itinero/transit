﻿// The MIT License (MIT)

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
using System.IO;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the stop links db.
    /// </summary>
    [TestFixture]
    public class StopLinksDbTests
    {
        /// <summary>
        /// Tests adding a link.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new StopLinksDb(1, new RouterDb(), string.Empty);

            db.Add(0, new RouterPoint(0, 0, 0, 0));

            var enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            db.Add(1, new RouterPoint(0, 0, 1, ushort.MaxValue));

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            enumerator.MoveTo(1);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue, enumerator.Offset);

            db.Add(1, new RouterPoint(0, 0, 2, ushort.MaxValue / 2));

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            enumerator.MoveTo(1);
            Assert.AreEqual(2, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue, enumerator.Offset);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue / 2, enumerator.Offset);
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var db = new StopLinksDb(5, new RouterDb(), "pedestrian");
            
            db.Add(0, new RouterPoint(0, 1, 0, 0));
            db.Add(0, new RouterPoint(2, 3, 0, 16));
            db.Add(0, new RouterPoint(4, 5, 0, 64));
            db.Add(0, new RouterPoint(6, 7, 0, 256));
            db.Add(0, new RouterPoint(8, 9, 0, 1024));

            var profileBytes = System.Text.Encoding.Unicode.GetByteCount("pedestrian");

            var size = 1 + 8 + 8 + 16 + profileBytes + 8 + 5 * 2 * 4 + 5 * 2 * 4;
            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(size, db.SizeInBytes);
                Assert.AreEqual(size, db.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var db = new StopLinksDb(5, new RouterDb(), "pedestrian");

            db.Add(0, new RouterPoint(0, 1, 0, 0));
            db.Add(0, new RouterPoint(2, 3, 0, 16));
            db.Add(0, new RouterPoint(4, 5, 0, 64));
            db.Add(0, new RouterPoint(6, 7, 0, 256));
            db.Add(0, new RouterPoint(8, 9, 0, 1024));

            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);
                var db1 = StopLinksDb.Deserialize(stream);
                
                Assert.AreEqual(db.SizeInBytes, db1.SizeInBytes);

                var enumerator = db.GetEnumerator();
                var enumerator1 = db1.GetEnumerator();

                enumerator.MoveTo(0);
                enumerator1.MoveTo(0);
                while (enumerator.MoveNext())
                {
                    Assert.IsTrue(enumerator1.MoveNext());

                    Assert.AreEqual(enumerator.EdgeId, enumerator1.EdgeId);
                    Assert.AreEqual(enumerator.Offset, enumerator1.Offset);
                }
            }
        }
    }
}
