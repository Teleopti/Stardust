using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixProTest
    {
        private ScheduleMatrixPro _target;
        private IPerson _person;
        private DateOnlyPeriod _period;
        private IVirtualSchedulePeriod _schedulePeriod;

        [SetUp]
        public void Setup()
        {
			_person = PersonFactory.CreatePerson("Testor");
			_person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1053));

            DateTimePeriod wholePeriod = new DateTimePeriod(1999, 12, 15, 2000, 01, 14);
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(wholePeriod);
            IScenario scenario = new Scenario("Scenario");
            var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());

            DateTimePeriod dayPeriod = new DateTimePeriod(2000, 01, 01, 2000, 01, 10);
            IScheduleParameters parameters = new ScheduleParameters(scenario, _person, dayPeriod);
            IScheduleRange range = new ScheduleRange(scheduleDictionary, parameters, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());

            scheduleDictionary.AddTestItem(_person, range);


            _period = new DateOnlyPeriod(2010, 1, 8, 2010, 1, 14);
            var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
            _schedulePeriod = new VirtualSchedulePeriod(_person, _period.StartDate , splitChecker);
            _target = ScheduleMatrixProFactory.Create(_period, scheduleDictionary, _person, _schedulePeriod);
        }

        [Test]
        public void TestConstructorsWithDateOnlyPeriod()
        {
            Assert.AreSame(_person, _target.Person);
            Assert.AreEqual(7, _target.EffectivePeriodDays.Length);
            Assert.AreSame(_schedulePeriod, _target.SchedulePeriod);
        }

		[Test]
		public void VerifyAllDaysAreLockedWhenCreated()
		{
			Assert.AreEqual(0, _target.UnlockedDays.Length);
		}

		[Test]
		public void VerifyPeriodCouldBeUnlocked()
		{
			_target.UnlockPeriod(_period);
			Assert.AreEqual(7, _target.UnlockedDays.Length);
		}

        [Test]
        public void VerifyFullWeeksPeriodIsSet()
        {
            Assert.IsTrue(_target.FullWeeksPeriodDays.Length >= _target.EffectivePeriodDays.Length);
            Assert.IsTrue(_target.FullWeeksPeriodDays.Length >= 7);
            Assert.IsTrue(_target.FullWeeksPeriodDays.Length % 7 == 0);
            Assert.AreSame(_target.EffectivePeriodDays[0], _target.FullWeeksPeriodDays[4]);
        }

        [Test]
        public void VerifyOuterWeeksPeriodIsSet()
        {
            Assert.AreEqual(_target.FullWeeksPeriodDays.Length + 14, _target.OuterWeeksPeriodDays.Length);
            Assert.AreSame(_target.FullWeeksPeriodDays[0], _target.OuterWeeksPeriodDays[7]);
        }

        [Test]
        public void VerifyWeekBeforeOuterPeriodIsSet()
        {
            Assert.AreEqual(_target.FullWeeksPeriodDays.Length + 7, _target.WeekBeforeOuterPeriodDays.Length);
            Assert.AreSame(_target.OuterWeeksPeriodDays[0], _target.WeekBeforeOuterPeriodDays[0]);
        }

        [Test]
        public void VerifyWeekAfterOuterPeriodIsSet()
        {
            Assert.AreEqual(_target.FullWeeksPeriodDays.Length + 7, _target.WeekBeforeOuterPeriodDays.Length);
            Assert.AreSame(_target.OuterWeeksPeriodDays[7], _target.WeekAfterOuterPeriodDays[0]);
            Assert.AreSame(_target.OuterWeeksPeriodDays[_target.OuterWeeksPeriodDays.Length - 1], _target.WeekAfterOuterPeriodDays[_target.WeekAfterOuterPeriodDays.Length - 1]);
        }

        [Test]
        public void VerifyCanGetScheduleDayByKey()
        {
            IScheduleDayPro day = _target.GetScheduleDayByKey(new DateOnly(2010, 1, 10));
            Assert.AreEqual(new DateOnly(2010, 1, 10), day.Day);
        }

        [Test]
        public void VerifyOuterWeeksPeriodDictionary()
        {
            Assert.AreEqual(_target.OuterWeeksPeriodDays.Length, _target.OuterWeeksPeriodDictionary.Values.Count);
        }

		[Test]
		public void VerifyUnlockingOutsidePeriod()
		{
			DateOnlyPeriod partlyOutsidePeriod = new DateOnlyPeriod(2010, 1, 9, 2010, 1, 15);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.UnlockPeriod(partlyOutsidePeriod));
		}
    }
}
