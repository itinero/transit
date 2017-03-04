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
using System;
using System.IO;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the schedules db.
    /// </summary>
    [TestFixture]
    public class SchedulesDbTests
    {
        /// <summary>
        /// Tests adding a schedule and entries.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new SchedulesDb();

            var id = db.Add();
            var day = new DateTime(2015, 11, 23);
            db.AddEntry(id, day, day.AddDays(14), DayOfWeek.Monday, DayOfWeek.Saturday);

            var enumerator = db.GetEnumerator();

            enumerator.MoveTo(id);
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 23)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 28)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 24)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 16)));
        }

        /// <summary>
        /// Tests an enumerator.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            var db = new SchedulesDb();

            var id1 = db.Add();
            var day11 = new DateTime(2015, 11, 23);
            var day12 = new DateTime(2015, 11, 29);
            var day13 = new DateTime(2015, 11, 30);
            var day14 = new DateTime(2015, 12, 06);
            db.AddEntry(id1, day11, day12, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id1, day13, day14, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            var id2 = db.Add();
            var day21 = new DateTime(2015, 09, 29);
            var day22 = new DateTime(2015, 10, 05);
            var day23 = new DateTime(2015, 10, 06);
            var day24 = new DateTime(2015, 10, 06);
            db.AddEntry(id2, day21, day22, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id2, day23, day24, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            var enumerator = db.GetEnumerator();

            enumerator.MoveTo(id1);
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 22)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 23)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 24)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 25)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 26)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 27)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 28)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 29)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 30)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 01)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 02)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 03)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 04)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 05)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 06)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 07)));

            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 28)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 29)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 30)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 01)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 02)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 03)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 04)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 05)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 06)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 07)));

            enumerator.MoveTo(id2);
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 22)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 23)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 24)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 25)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 26)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 27)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 28)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 29)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 30)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 01)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 02)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 03)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 04)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 05)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 06)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 07)));

            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 28)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 29)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 09, 30)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 01)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 02)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 10, 03)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 04)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 10, 05)));
            Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 10, 06)));
            Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 10, 07)));
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var db = new SchedulesDb();

            var id1 = db.Add();
            var day11 = new DateTime(2015, 11, 23);
            var day12 = new DateTime(2015, 11, 29);
            var day13 = new DateTime(2015, 11, 30);
            var day14 = new DateTime(2015, 12, 06);
            db.AddEntry(id1, day11, day12, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id1, day13, day14, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            var id2 = db.Add();
            var day21 = new DateTime(2015, 09, 29);
            var day22 = new DateTime(2015, 10, 05);
            var day23 = new DateTime(2015, 10, 06);
            var day24 = new DateTime(2015, 10, 06);
            db.AddEntry(id2, day21, day22, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id2, day23, day24, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            var size = 1 + 8 + (6 * 4);
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
            var db = new SchedulesDb();

            var id1 = db.Add();
            var day11 = new DateTime(2015, 11, 23);
            var day12 = new DateTime(2015, 11, 29);
            var day13 = new DateTime(2015, 11, 30);
            var day14 = new DateTime(2015, 12, 06);
            db.AddEntry(id1, day11, day12, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id1, day13, day14, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            var id2 = db.Add();
            var day21 = new DateTime(2015, 09, 29);
            var day22 = new DateTime(2015, 10, 05);
            var day23 = new DateTime(2015, 10, 06);
            var day24 = new DateTime(2015, 10, 06);
            db.AddEntry(id2, day21, day22, DayOfWeek.Monday, DayOfWeek.Saturday);
            db.AddEntry(id2, day23, day24, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday);

            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);
                db = SchedulesDb.Deserialize(stream);
                
                var enumerator = db.GetEnumerator();
                Assert.IsTrue(enumerator.MoveTo(id1));

                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 22)));
                Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 23)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 24)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 25)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 26)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 27)));
                Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 11, 28)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 29)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 11, 30)));
                Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 01)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 02)));
                Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 03)));
                Assert.IsTrue(enumerator.DateIsSet(new DateTime(2015, 12, 04)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 05)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 06)));
                Assert.IsFalse(enumerator.DateIsSet(new DateTime(2015, 12, 07)));
            }
        }
    }
}
