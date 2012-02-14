using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for DateTimePeriodMapper
    /// </summary>
    [TestFixture]
    public class DateTimePeriodMapperTest : MapperTest<global::Domain.DatePeriod>
    {
        private ICccTimeZoneInfo timeZone;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            timeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
        }

        /// <summary>
        /// Determines whether this instance [can map time period6x and date].
        /// </summary>
        [Test]
        public void CanMapTimePeriod6XAndDate()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(8, 9, 0), new TimeSpan(16, 16, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 8, 9, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 16, 16, 0, DateTimeKind.Utc));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(timeZone, new DateTime(2007, 1, 1));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance [can map time period6x and date starts yeserday].
        /// </summary>
        [Test]
        public void CanMapTimePeriod6XAndDateStartsYesterday()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(-2, 0, 0), new TimeSpan(6, 0, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2006, 12, 31, 22, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 6, 0, 0, DateTimeKind.Utc));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(timeZone, new DateTime(2007, 1, 1));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance can map time period6x and date if ends tomorrow.
        /// </summary>
        [Test]
        public void CanMapTimePeriod6XAndDateEndsTomorrow()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(22, 0, 0), new TimeSpan(1, 6, 0, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 6, 0, 0, DateTimeKind.Utc));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(timeZone, new DateTime(2007, 1, 1));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance [can map time period with start in DST change].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-13
        /// </remarks>
        [Test]
        public void CanMapTimePeriodWithStartInDstChange()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(2, 30, 0), new TimeSpan(3, 30, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 3, 25, 1, 30, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 3, 25, 1, 30, 0, DateTimeKind.Utc));

            ICccTimeZoneInfo currentTimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(currentTimeZone, new DateTime(2007, 3, 25));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance [can map time period with end in DST change].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-13
        /// </remarks>
        [Test]
        public void CanMapTimePeriodWithEndInDstChange()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(1, 30, 0), new TimeSpan(2, 30, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 3, 25, 0, 30, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 3, 25, 1, 30, 0, DateTimeKind.Utc));

            ICccTimeZoneInfo currentTimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(currentTimeZone, new DateTime(2007, 3, 25));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance [can map time period with end equal to DST change].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-13
        /// </remarks>
        [Test]
        public void CanMapTimePeriodWithEndEqualToDstChange()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(2, 30, 0), new TimeSpan(3, 00, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 3, 25, 1, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 3, 25, 1, 0, 0, DateTimeKind.Utc));

            ICccTimeZoneInfo currentTimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(currentTimeZone, new DateTime(2007, 3, 25));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        /// <summary>
        /// Determines whether this instance [can map time period start and end as DST change].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-13
        /// </remarks>
        [Test]
        public void CanMapTimePeriodStartAndEndAsDstChange()
        {
            global::Domain.TimePeriod p1 = new global::Domain.TimePeriod(new TimeSpan(2, 0, 0), new TimeSpan(3, 0, 0));
            DateTimePeriod dtP1 =
                new DateTimePeriod(new DateTime(2007, 3, 25, 1, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 3, 25, 1, 0, 0, DateTimeKind.Utc));

            ICccTimeZoneInfo currentTimeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTimePeriodMapper dtMapper = new DateTimePeriodMapper(currentTimeZone, new DateTime(2007, 3, 25));
            DateTimePeriod newObj = dtMapper.Map(p1);
            Assert.AreEqual(dtP1, newObj);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 2; }
        }
    }
}