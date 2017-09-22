using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.WinCodeTest.Settings
{

    /// <summary>
    /// Represents a .
    /// </summary>
    [TestFixture]
    public class RotationRestrictionViewTest
    {
        private RotationRestrictionView _targetView;

        IRotationRestriction _containedEntity;
        private EndTimeLimitation _endTimeLimit = new EndTimeLimitation();
        private StartTimeLimitation _startTimeLimit = new StartTimeLimitation();
        private readonly TimeSpan _minTimeWork = new TimeSpan(7, 0, 0);
        private readonly TimeSpan _maxTimeWork = new TimeSpan(8, 30, 0);
        private WorkTimeLimitation _workTimePeriod;
        private IDayOffTemplate _dayOff;
        private const int DayCount = 4;

        [SetUp]
        public void Init()
        {
            _dayOff = new DayOffTemplate(new Description("FreeDay"));
            _dayOff.SetId(Guid.NewGuid());
            _workTimePeriod = new WorkTimeLimitation(_minTimeWork, _maxTimeWork);
            _containedEntity = new RotationRestriction
                                   {
                                       EndTimeLimitation = _endTimeLimit,
                                       StartTimeLimitation = _startTimeLimit,
                                       WorkTimeLimitation = _workTimePeriod,
                                       DayOffTemplate = _dayOff
                                   };

            int weekNumber = ScheduleRestrictionBaseView.GetWeek(DayCount);

            _targetView = new RotationRestrictionView(_containedEntity, weekNumber, DayCount);
        }

        [Test]
        public void VerifyPropertyValues()
        {
            Assert.IsNotNull(_targetView);

            Assert.AreEqual(_targetView.DayOffTemplate, _dayOff);
            Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTime);
            Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTime);
            Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTime);
            Assert.AreEqual(_targetView.MaximumWorkTime, _workTimePeriod.EndTime);
            Assert.IsNotNull(_targetView.ShiftCategory);
            Assert.AreEqual(_targetView.Week, ScheduleRestrictionBaseView.GetWeek(DayCount));

            string dayName = UserTexts.Resources.Day + " " + DayCount.ToString(CultureInfo.CurrentUICulture);
            Assert.AreEqual(_targetView.Day, dayName);
        }

        [Test]
        public void VerifyDayOffChange()
        {
            Assert.IsNotNull(_targetView);
            Assert.AreEqual(_targetView.DayOffTemplate, _dayOff);

            _targetView.DayOffTemplate = null;

            Assert.AreEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);

            _targetView.DayOffTemplate = _dayOff;
            Assert.AreEqual(_targetView.DayOffTemplate, _dayOff);
            Assert.AreNotEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);
        }

        [Test]
        public void VerifyDayOffChangeWhenTimeChanged()
        {
            Assert.IsNotNull(_targetView);
            IDayOffTemplate dayOff = new DayOffTemplate(new Description("LeisureDay"));

            _targetView.DayOffTemplate = dayOff;
            _targetView.EarlyStartTime = new TimeSpan(8,25,0);
            Assert.AreEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);

            _targetView.DayOffTemplate = null;
            _targetView.LateStartTime = new TimeSpan(8, 25,0);
            Assert.AreEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);

            _targetView.DayOffTemplate = dayOff;
            _targetView.EarlyEndTime = new TimeSpan(8, 25,0);
            Assert.AreEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);

            _targetView.DayOffTemplate = null;
            _targetView.LateEndTime = new TimeSpan(8, 25,0);
            Assert.AreEqual(_targetView.DayOffTemplate, RotationRestrictionView.DefaultDayOff);
        }

        [Test]
        public void VerifyShiftCategoryNotNull()
        {
            Assert.IsNotNull(_targetView);

            _targetView.ShiftCategory = null;
            Assert.IsNotNull(_targetView.ShiftCategory);

            IShiftCategory defaultCategory = _targetView.ShiftCategory;

            _targetView.ShiftCategory = null;
            Assert.IsNotNull(_targetView.ShiftCategory);
            Assert.AreSame(_targetView.ShiftCategory, defaultCategory);
            _targetView.ShiftCategory = defaultCategory;
            Assert.AreSame(_targetView.ShiftCategory, defaultCategory);
        }

        [Test]
        public void VerifyShiftCategoryChange()
        {
            Assert.IsNotNull(_targetView);

            _targetView.ShiftCategory = null;
            IShiftCategory defaultCategory = _targetView.ShiftCategory;
            IShiftCategory newCategory = new ShiftCategory("Some Category");

            _targetView.ShiftCategory = newCategory;

            Assert.AreNotSame(_targetView.ShiftCategory, defaultCategory);
            Assert.AreSame(_targetView.ShiftCategory, newCategory);
        }

        [Test]
        public void VerifyDayOffs()
        {
            Assert.IsNotNull(_targetView);

            _targetView.DayOffTemplate = null;
            IDayOffTemplate defaultDayOff = _targetView.DayOffTemplate;
            IDayOffTemplate newDayOff = new DayOffTemplate(new Description("Free day"));
            _targetView.DayOffTemplate = newDayOff;

            Assert.AreNotSame(_targetView.DayOffTemplate, defaultDayOff);
            Assert.AreSame(_targetView.DayOffTemplate, newDayOff);

            IDayOffTemplate newInstance = RotationRestrictionView.DefaultDayOff;
            Assert.AreEqual(newInstance, RotationRestrictionView.DefaultDayOff);
        }

        [Test]
        public void ShouldAssignTimeValuesToDomainEntity()
        {
            _containedEntity.DayOffTemplate = null;
            _containedEntity.ShiftCategory = null;
            var rotationRestrictionView = new RotationRestrictionView(_containedEntity, 10, 1)
                                              {
                                                  EarlyStartTime = TimeSpan.FromHours(8),
                                                  LateStartTime = TimeSpan.FromHours(12),
                                                  EarlyEndTime = TimeSpan.FromHours(20),
                                                  LateEndTime = TimeSpan.FromHours(23),
                                                  MinimumWorkTime = TimeSpan.FromHours(7),
                                                  MaximumWorkTime = TimeSpan.FromHours(9)
                                              };

            rotationRestrictionView.AssignValuesToDomainObject();

            Assert.IsNull(_containedEntity.DayOffTemplate);
            Assert.IsNull(_containedEntity.ShiftCategory);
            Assert.AreEqual(TimeSpan.FromHours(8), _containedEntity.StartTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(12), _containedEntity.StartTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(20), _containedEntity.EndTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(23), _containedEntity.EndTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(7), _containedEntity.WorkTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(9), _containedEntity.WorkTimeLimitation.EndTime);
        }

        [Test]
        public void ShouldAssignShiftCategoryAndDayOffToDomainEntity()
        {
            IShiftCategory testCategory = new ShiftCategory("TestCategory");
            testCategory.SetId(Guid.NewGuid());

            var rotationRestrictionView = new RotationRestrictionView(_containedEntity, 10, 1)
                                              {
                                                  DayOffTemplate = _dayOff,
                                                  ShiftCategory = testCategory
                                              };

            rotationRestrictionView.AssignValuesToDomainObject();

            Assert.AreEqual(rotationRestrictionView.DayOffTemplate, _containedEntity.DayOffTemplate);
            Assert.AreEqual(rotationRestrictionView.ShiftCategory, _containedEntity.ShiftCategory);
        }
    }

}
