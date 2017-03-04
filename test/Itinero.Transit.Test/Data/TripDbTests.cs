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
using System.IO;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the trips db.
    /// </summary>
    [TestFixture]
    public class TripDbTests
    {
        /// <summary>
        /// Tests adding.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new TripsDb(3);

            Assert.AreEqual(0, db.Add(1, 2, 124));

            var enumerator = db.GetEnumerator();
            enumerator.MoveTo(0);

            Assert.AreEqual(1, enumerator.ScheduleId);
            Assert.AreEqual(2, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            Assert.AreEqual(1, db.Add(2, 22, 128));
            Assert.AreEqual(2, db.Add(3, 32, 132));
            Assert.AreEqual(3, db.Add(4, 42, 136));
            Assert.AreEqual(4, db.Add(5, 52, 140));
            Assert.AreEqual(5, db.Add(6, 62, 144));

            Assert.AreEqual(6, db.Count);

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.ScheduleId);
            Assert.AreEqual(2, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveTo(1);
            Assert.AreEqual(2, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveTo(2);
            Assert.AreEqual(3, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveTo(3);
            Assert.AreEqual(4, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveTo(4);
            Assert.AreEqual(5, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveTo(5);
            Assert.AreEqual(6, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);
        }

        /// <summary>
        /// Tests the enumerator.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            var db = new TripsDb();

            db.Add(11, 12, 124);
            db.Add(21, 22, 128);
            db.Add(31, 32, 132);
            db.Add(41, 42, 136);
            db.Add(51, 52, 140);
            db.Add(61, 62, 144);

            var enumerator = db.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual(11, enumerator.ScheduleId);
            Assert.AreEqual(12, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(21, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(31, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(41, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(51, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(61, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            enumerator.MoveNext();
            Assert.AreEqual(11, enumerator.ScheduleId);
            Assert.AreEqual(12, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(21, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(31, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(41, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(51, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(61, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var db = new TripsDb();

            db.Add(1, 2, 3);
            db.Add(4, 5, 6);
            db.Add(7, 8, 9);
            db.Add(10, 11, 12);
            db.Add(13, 14, 15);

            var size = 1 + 8 + (5 * 3 * 4);
            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(size, db.SizeInBytes);
                Assert.AreEqual(size, db.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests deserializing.
        /// </summary>
        [Test]
        public void TestDeserialization()
        {
            var db = new TripsDb();

            db.Add(1, 2, 3);
            db.Add(4, 5, 6);
            db.Add(7, 8, 9);
            db.Add(10, 11, 12);
            db.Add(13, 14, 15);

            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);
                var db1 = TripsDb.Deserialize(stream);

                Assert.AreEqual(db.Count, db1.Count);
                Assert.AreEqual(db.SizeInBytes, db1.SizeInBytes);

                var enumerator = db.GetEnumerator();
                var enumerator1 = db1.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsTrue(enumerator1.MoveNext());

                    Assert.AreEqual(enumerator.Id, enumerator1.Id);
                    Assert.AreEqual(enumerator.AgencyId, enumerator1.AgencyId);
                    Assert.AreEqual(enumerator.MetaId, enumerator1.MetaId);
                    Assert.AreEqual(enumerator.ScheduleId, enumerator1.ScheduleId);
                }
            }
        }
    }
}
