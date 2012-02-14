using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using ShiftCategory = Domain.ShiftCategory;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class RotationsMapperTest : MapperTest<DataRow>
    {
        private MappedObjectPair _mappedObjectPair;
        ObjectPairCollection<DataRow, IRotation> _rotations;
        private RotationsMapper _rotationsMapper;
        private DataRow _oldRow;
        private ObjectPairCollection<ShiftCategory, IShiftCategory> _cats;
        private readonly IList<IDayOffTemplate> _dayOffList = new List<IDayOffTemplate>();
        private IDayOffTemplate _dayoff;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }


        private void setupObjectPairs()
        {
            _mappedObjectPair = new MappedObjectPair();
            _rotations = new ObjectPairCollection<DataRow, IRotation>();
            _cats = new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            _mappedObjectPair.Rotations = _rotations;
            _mappedObjectPair.ShiftCategory = _cats;
        }

        [SetUp]
        public void Setup()
        {
            setupObjectPairs();
            var t = new DataTable("table1") {Locale = CultureInfo.CurrentCulture};
        	t.Columns.Add("rotation_id", typeof(int));
            t.Columns.Add("rotation_name", typeof(string));
            t.Columns.Add("weeks", typeof(int));
            t.Columns.Add("abs_short_desc", typeof(string));
            _oldRow = t.NewRow();
            _oldRow["rotation_id"] = 10;
            _oldRow["rotation_name"] = "Ola";
            _oldRow["weeks"] = 2;
            _oldRow["abs_short_desc"] = "DO";

            _dayoff = new DayOffTemplate(new Description("Day Off", "DO"));
            _dayoff.SetId(Guid.NewGuid());
            IDayOffTemplate dayoff1 = new DayOffTemplate(new Description("Leisure Day", "LD"));
            _dayOffList.Add(_dayoff);
            _dayOffList.Add(dayoff1);

            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(20, 50, "DO"), 15, _dayOffList);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private static IList<DataRow> getRotationDays(int? startInterval, int? endInterval, int? shiftCat, int? absId, string dayOffShortName)
        {
            var t2 = new DataTable("table2") {Locale = CultureInfo.CurrentCulture};
        	t2.Columns.Add("rotation_id", typeof(int));
            t2.Columns.Add("rotation_day", typeof(int));
            t2.Columns.Add("start_interval", typeof(int));
            t2.Columns.Add("end_interval", typeof(int));
            t2.Columns.Add("shift_cat_id", typeof(int));
            t2.Columns.Add("abs_id", typeof(int));
            t2.Columns.Add("abs_short_desc", typeof(string));
        	DataRow rotationDayRow = t2.NewRow();
            rotationDayRow["rotation_id"] = 10;
            rotationDayRow["rotation_day"] = 1;
            if (endInterval != null)
                rotationDayRow["end_interval"] = endInterval;
            if (startInterval != null)
                rotationDayRow["start_interval"] = startInterval;

            if (shiftCat != null)
                rotationDayRow["shift_cat_id"] = shiftCat;
            if (absId != null)
                rotationDayRow["abs_id"] = absId;
            rotationDayRow["abs_short_desc"] = dayOffShortName;

            IList<DataRow> rotationDays = new List<DataRow> { rotationDayRow };
            return rotationDays;
        }
        private static IList<DataRow> getRotationDays(int? startInterval, int? endInterval, string dayOffShortName)
        {
            return getRotationDays(startInterval, endInterval, 1, 1, dayOffShortName);
        }

        [Test]
        public void VerifyMapperWhenHaveCategory()
        {
            var oldCat = new ShiftCategory(1, "dwd", "adf", Color.DeepPink, true, true, 0);
            IShiftCategory newCat = new Domain.Scheduling.ShiftCategory("asfa");
            _cats.Add(oldCat, newCat);
            _mappedObjectPair.ShiftCategory = _cats;
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(20, 50, 1, null, null), 15, _dayOffList);

            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.AreEqual(newCat, rest.ShiftCategory);
        }

        [Test]
        public void ShouldTreatUnknownShiftCategoryAsEmptyRotationDay()
        {
            var oldCat = new ShiftCategory(-1, "Unknown", "UNKNOWN", Color.DeepPink, true, true, 0);
            IShiftCategory newCat = new Domain.Scheduling.ShiftCategory("Unknown");
            _cats.Add(oldCat, newCat);
            _mappedObjectPair.ShiftCategory = _cats;
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(20, 50, -1, null, null), 15, _dayOffList);

            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.IsNull(rest.ShiftCategory);
        }

        [Test]
        public void VerifyMapperWhenDayOff()
        {
            var oldCat = new ShiftCategory(1, "dwd", "adf", Color.DeepPink, true, true, 0);
            IShiftCategory newCat = new Domain.Scheduling.ShiftCategory("asfa");
            _cats.Add(oldCat, newCat);
            _mappedObjectPair.ShiftCategory = _cats;
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(20, 50, 1, 1, "DO"), 15, _dayOffList);

            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.IsNull(rest.ShiftCategory);

        }

        [Test]
        public void VerifyMapperWhenEndOnNextDay()
        {
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(80, 19, null), 15, _dayOffList);
            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.AreEqual(new TimeSpan(1, 5, 0, 0), rest.EndTimeLimitation.StartTime);

        }

        [Test]
        public void VerifyMapperWhenStartIsZero()
        {
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(0, 19, null), 15, _dayOffList);
            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.IsNull(rest.StartTimeLimitation.StartTime);
            Assert.IsNull(rest.StartTimeLimitation.EndTime);
        }

        [Test]
        public void VerifyMapperWhenEndIsZero()
        {
            _rotationsMapper = new RotationsMapper(_mappedObjectPair, getRotationDays(8, null, null), 15, _dayOffList);
            IRotation newRule = _rotationsMapper.Map(_oldRow);

            IRotationRestriction rest = newRule.RotationDays[0].RestrictionCollection[0];

            Assert.IsNull(rest.EndTimeLimitation.StartTime);
            Assert.IsNull(rest.EndTimeLimitation.EndTime);
        }
        [Test]
        public void VerifyGetObjectPairCollection()
        {
            Assert.AreSame(_rotations, _mappedObjectPair.Rotations);
        }
    }
}
