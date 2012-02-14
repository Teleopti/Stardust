using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class AvailabilityMapperTest : MapperTest<DataRow>
    {
        private MappedObjectPair _mappedObjectPair;
        ObjectPairCollection<DataRow, IAvailabilityRotation> _availabilities;
        ObjectPairCollection<Agent, IPerson> _persons;
        private AvailabilityMapper _availabilityMapper;
        private DataRow _oldRow;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }

        private void SetupObjectPairs()
        {
            _mappedObjectPair = new MappedObjectPair();
            _availabilities = new ObjectPairCollection<DataRow, IAvailabilityRotation>();
            _persons = new ObjectPairCollection<Agent, IPerson>();
            _mappedObjectPair.Availability = _availabilities;
            _mappedObjectPair.Agent = _persons;
        }

        [SetUp]
        public void Setup()
        {
            SetupObjectPairs();
            var t = new DataTable("table1") {Locale = CultureInfo.CurrentCulture};
            t.Columns.Add("core_id", typeof(int));
            t.Columns.Add("periods", typeof(int));
            t.Columns.Add("date_from", typeof(DateTime));
            t.Columns.Add("emp_id", typeof(int));
            _oldRow = t.NewRow();
            _oldRow["core_id"] = 10;
            _oldRow["periods"] = 2;
            _oldRow["date_from"] = new DateTime(2009, 12, 24);
            _oldRow["emp_id"] = 666;

            _availabilityMapper = new AvailabilityMapper(_mappedObjectPair, GetAvailabilityDays(20, 50));
        }

        private static IList<DataRow> GetAvailabilityDays(int? startMinute, int? endMinute)
        {
            var t2 = new DataTable("table2") {Locale = CultureInfo.CurrentCulture};
            t2.Columns.Add("core_id", typeof(int));
            t2.Columns.Add("core_day", typeof(int));
            t2.Columns.Add("time_from", typeof(int));
            t2.Columns.Add("time_to", typeof(int));
            t2.Columns.Add("available", typeof(bool));
            DataRow availabilityDayRow = t2.NewRow();
            availabilityDayRow["core_id"] = 10;
            availabilityDayRow["core_day"] = 1;
            availabilityDayRow["available"] = false;
            if (endMinute != null)
                availabilityDayRow["time_to"] = endMinute;
            if (startMinute != null)
                availabilityDayRow["time_from"] = startMinute;
            IList<DataRow> availabilityDays = new List<DataRow> { availabilityDayRow };
            return availabilityDays;
        }
        //private static IList<DataRow> GetRotationDays(int? startInterval, int? endInterval, string dayOffShortName)
        //{
        //    return GetRotationDays(startInterval, endInterval, 1, 1, dayOffShortName);
        //}

        //[Test]
        //public void VerifyMapperWhenHaveCategory()
        //{
        //    ShiftCategory oldCat = new ShiftCategory(1, "dwd", "adf", Color.DeepPink, true, true, 0);
        //    IShiftCategory newCat = new Domain.Scheduling.ShiftCategory("asfa");
        //    _cats.Add(oldCat, newCat);
        //    mappedObjectPair.ShiftCategory = _cats;
        //    rotationsMapper = new RotationsMapper(mappedObjectPair, GetRotationDays(20, 50, 1, null, null), 15, _dayOffList);

        //    IRotation newRule = rotationsMapper.Map(oldRow);

        //    IRotationRestriction rest = newRule.RotationDays[0].RotationRestrictions[0];

        //    Assert.AreEqual(newCat, rest.ShiftCategory);

        //}

        [Test]
        public void VerifyMapperWhenNotAvailable()
        {
            _availabilityMapper = new AvailabilityMapper(_mappedObjectPair, GetAvailabilityDays(20, 50));

            IAvailabilityRotation rule = _availabilityMapper.Map(_oldRow);

            IAvailabilityRestriction rest = rule.AvailabilityDays[0].Restriction;

            Assert.IsTrue(rest.NotAvailable);

        }

        [Test]
        public void VerifyMapperWhenEndOnNextDay()
        {
            _availabilityMapper = new AvailabilityMapper(_mappedObjectPair, GetAvailabilityDays(1020, 480));
            IAvailabilityRotation rule = _availabilityMapper.Map(_oldRow);

            IAvailabilityRestriction rest = rule.AvailabilityDays[0].Restriction;

            Assert.AreEqual(new TimeSpan(1, 8, 0, 0), rest.EndTimeLimitation.EndTime);
            Assert.That(rest.EndTimeLimitation.StartTime, Is.Null);
        }

        [Test]
        public void VerifyMapperWhenStartIsZero()
        {
            _availabilityMapper = new AvailabilityMapper(_mappedObjectPair, GetAvailabilityDays(0, 1020));
            IAvailabilityRotation rule = _availabilityMapper.Map(_oldRow);

            IAvailabilityRestriction rest = rule.AvailabilityDays[0].Restriction;

            Assert.IsNull(rest.StartTimeLimitation.StartTime);
            Assert.IsNull(rest.StartTimeLimitation.EndTime);
        }

        [Test]
        public void VerifyMapperWhenEndIsZero()
        {

            _availabilityMapper = new AvailabilityMapper(_mappedObjectPair, GetAvailabilityDays(480, 0));
            IAvailabilityRotation rule = _availabilityMapper.Map(_oldRow);

            IAvailabilityRestriction rest = rule.AvailabilityDays[0].Restriction;

            Assert.IsNull(rest.EndTimeLimitation.StartTime);
            Assert.IsNull(rest.EndTimeLimitation.EndTime);
        }
        [Test]
        public void VerifyGetObjectPairCollection()
        {
            Assert.AreSame(_availabilities, _mappedObjectPair.Availability);
        }

        [Test]
        public void VerifyConversion()
        {
            IAvailabilityRotation rule = _availabilityMapper.Map(_oldRow);
            Assert.AreEqual(14, rule.DaysCount);

            IAvailabilityRestriction rest = rule.AvailabilityDays[0].Restriction;

            Assert.That(rest.StartTimeLimitation, Is.EqualTo(new StartTimeLimitation(new TimeSpan(0, 20, 0), null)));
            Assert.That(rest.EndTimeLimitation, Is.EqualTo(new EndTimeLimitation(null, new TimeSpan(0, 50, 0))));

        }
    }
}
