using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class SchedulePeriodChildModelTest
    {
        private SchedulePeriodChildModel _target;

        private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3;
        private ISkill _skill2, _skill3, _skill1;
        private ISchedulePeriod _schedulePeriod1;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Mama Hawa");

            DateOnly from1 = new DateOnly(2008, 1, 3);

            _schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(from1);

            _person.AddSchedulePeriod(_schedulePeriod1);

            _skill1 = SkillFactory.CreateSkill("_skill1");
            _skill2 = SkillFactory.CreateSkill("_skill2");
            _skill3 = SkillFactory.CreateSkill("_skill3");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(DateTime.MinValue.AddYears(2)), _skill1);
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(DateTime.Now.AddYears(50)), _skill2);
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly.Today, _skill3);

            _personPeriod1.RuleSetBag = new RuleSetBag();
            _personPeriod2.RuleSetBag = new RuleSetBag();
            _personPeriod3.RuleSetBag = new RuleSetBag();

            _person.AddPersonPeriod(_personPeriod1);
            _person.AddPersonPeriod(_personPeriod2);
            _person.AddPersonPeriod(_personPeriod3);

            _target = EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(_schedulePeriod1);
            _target.FullName = "Mama Hawa";
        }

        [Test]
        public void VerifyPropertiesNotNullOrEmpty()
        {
            Assert.IsNotEmpty(_target.FullName);
            Assert.IsNotNull(_target.PeriodDate);
            Assert.IsNotNull(_target.Number);
            Assert.IsNotNull(_target.PeriodType);
            Assert.IsNotNull(_target.DaysOff);
            Assert.IsNotNull(_target.AverageWorkTimePerDay);
            Assert.IsFalse(_target.CanGray);
            Assert.IsFalse(_target.IsAverageWorkTimePerDayOverride);
            Assert.IsFalse(_target.IsDaysOffOverride);
			Assert.IsFalse(_target.IsPeriodTimeOverride);
            Assert.IsNotNull(_target.SchedulePeriod);
        }

        [Test]
        public void VerifyCanGetSetOverrideDaysOff()
        {
            Assert.IsFalse(_target.IsDaysOffOverride);
            _target.OverrideDaysOff = 2;
			Assert.IsTrue(_target.IsDaysOffOverride);
            Assert.AreEqual(2, _target.OverrideDaysOff);
        }

        [Test]
        public void CheckDaysOffSetter()
        {
            int value = 5;
            _target.DaysOff = value;

            Assert.AreEqual(value, _target.DaysOff);
        }

        [Test]
        public void CheckAverageWorkTimePerDaySetter()
        {
            TimeSpan value = new TimeSpan(1, 1, 1);
			_target.AverageWorkTimePerDayOverride = value;

            Assert.AreEqual(value, _target.AverageWorkTimePerDay);
        }

        [Test]
        public void VerifyFromDateCanSet()
        {
            _target.PeriodDate = DateOnly.MinValue;
            Assert.AreEqual(DateOnly.MinValue, _target.PeriodDate);
        }

        [Test]
        public void VerifyPeriodDateCannotBeSetToSameAsOtherPeriod()
        {
            _person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(DateOnly.MinValue));
            _target.PeriodDate = DateOnly.MinValue;

            Assert.AreEqual(DateOnly.MinValue.AddDays(1), _target.PeriodDate);
        }

        [Test]
        public void VerifyNumberCanSet()
        {
            _target.Number = 10;
            Assert.AreEqual(10, _target.Number);
        }

        [Test]
        public void VerifyPeriodTypeCanSet()
        {
			_target.PeriodType = SchedulePeriodType.Month;
			Assert.AreEqual(SchedulePeriodType.Month, _target.PeriodType);
        }
        [Test]
        public void VerifyMustHave()
        {
            _target.MustHavePreference = 3;
            Assert.AreEqual(3, _target.MustHavePreference);
        }

        [Test]
        public void VerifyIsOverridableWorks()
        {
            SchedulePeriodChildModel schedulePeriodModel = EntityConverter.ConvertToOther
                <ISchedulePeriod, SchedulePeriodChildModel>(_schedulePeriod1);

            Assert.IsFalse(schedulePeriodModel.IsAverageWorkTimePerDayOverride);
            Assert.IsFalse(schedulePeriodModel.IsDaysOffOverride);

			schedulePeriodModel.AverageWorkTimePerDayOverride = TimeSpan.Zero;
            schedulePeriodModel.DaysOff = 5;

            Assert.IsTrue(schedulePeriodModel.IsAverageWorkTimePerDayOverride);
            Assert.IsTrue(schedulePeriodModel.IsDaysOffOverride);
        }

        [Test]
        public void VerifyCanSetCanBold()
        {
            Assert.IsFalse(_target.CanBold);
            _target.CanBold = true;
            Assert.IsTrue(_target.CanBold);
        }

        [Test]
        public void ShouldSetBalanceIn()
        {
            _target.BalanceIn = TimeSpan.FromDays(3);
            Assert.That(_target.BalanceIn.Equals(TimeSpan.FromDays(3)));
        }

        [Test]
        public void ShouldSetExtra()
        {
            _target.Extra = TimeSpan.FromDays(3);
            Assert.That(_target.Extra.Equals(TimeSpan.FromDays(3)));
        }

		[Test]
		public void VerifySeasonality()
		{
			_target.Seasonality = new Percent(0.3);
			Assert.That(_target.Seasonality.Equals(new Percent(0.3)));
		}

		[Test]
		public void VerifyPeriodTime()
		{
			_target.PeriodTime = TimeSpan.FromDays(3);
			Assert.That(_target.PeriodTime.Equals(TimeSpan.FromDays(3)));
		}

		[Test]
		public void VerifyAverageWorkTimePerDayOverride()
		{
			_target.AverageWorkTimePerDayOverride = TimeSpan.FromDays(3);
			Assert.That(_target.AverageWorkTimePerDayOverride.Equals(TimeSpan.FromDays(3)));
		}
		

        [Test]
        public void ShouldSetBalanceOut()
        {
            _target.BalanceOut = TimeSpan.FromDays(3);
            Assert.That(_target.BalanceOut.Equals(TimeSpan.FromDays(3)));
        }
    }
}
