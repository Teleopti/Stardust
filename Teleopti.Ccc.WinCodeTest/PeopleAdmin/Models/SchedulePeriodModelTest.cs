using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class SchedulePeriodModelTest
    {
        private SchedulePeriodModel _target;
        private ISchedulePeriod _schedulePeriod1, _schedulePeriod2, _schedulePeriod3;
        private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3;
        private ISkill _skill1, _skill2, _skill3;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Mama Hawa");

            DateOnly from1 = new DateOnly(2008, 1, 3);
            DateOnly from2 = new DateOnly(2009, 1, 3);
            DateOnly from3 = new DateOnly(2010, 1, 3);

            _schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(from1);
            _schedulePeriod2 = SchedulePeriodFactory.CreateSchedulePeriod(from2);
            _schedulePeriod3 = SchedulePeriodFactory.CreateSchedulePeriod(from3);

            _skill1 = SkillFactory.CreateSkill("_skill1");
            _skill2 = SkillFactory.CreateSkill("_skill2");
            _skill3 = SkillFactory.CreateSkill("_skill3");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(from1, _skill1);
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriodWithSkills(from2, _skill2);
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriodWithSkills(from3, _skill3);

            _personPeriod1.RuleSetBag = new RuleSetBag();
            _personPeriod2.RuleSetBag = new RuleSetBag();
            _personPeriod3.RuleSetBag = new RuleSetBag();

            _person.AddPersonPeriod(_personPeriod1);
            _person.AddPersonPeriod(_personPeriod2);
            _person.AddPersonPeriod(_personPeriod3);

            _person.AddSchedulePeriod(_schedulePeriod1);
            _person.AddSchedulePeriod(_schedulePeriod2);
            _person.AddSchedulePeriod(_schedulePeriod3);


            _target = new SchedulePeriodModel(new DateOnly(2008, 5, 2), _person, null);
        }

        [Test]
        public void VerifyPropertiesNotNullOrEmpty()
        {
            Assert.IsNotNull(_target.Parent);
            Assert.IsNotEmpty(_target.FullName);
            Assert.IsNotNull(_target.PeriodDate);
            Assert.IsNotNull(_target.Number);
            Assert.IsNotNull(_target.PeriodType);
            Assert.IsNotNull(_target.DaysOff);
            Assert.IsNotNull(_target.AverageWorkTimePerDay);
            Assert.IsNotNull(_target.PeriodCount);
            Assert.IsNotNull(_target.OverrideDaysOff);
            Assert.IsFalse(_target.CanGray);
            Assert.IsFalse(_target.CanBold);
            Assert.IsFalse(_target.IsAverageWorkTimePerDayOverride);
            Assert.IsFalse(_target.IsDaysOffOverride);
            Assert.IsNotNull(_target.SchedulePeriod);

        }

        [Test]
        public void VerifyExpandStateCanSet()


        {
            Assert.AreEqual(false, _target.ExpandState);
            _target.ExpandState = true;
            Assert.AreEqual(true, _target.ExpandState);

        }

        [Test]
        public void VerifyGridControlCanSet()
        {
            using (GridControl grid = new GridControl())
            {
                _target.GridControl = grid;
                Assert.IsNotNull(_target.GridControl);
            }
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
            TimeSpan value = new TimeSpan(1,1,1);
			_target.AverageWorkTimePerDayOverride = value;

            Assert.AreEqual(value, _target.AverageWorkTimePerDay);
        }

        [Test]
        public void VerifyBalanceIn()
        {
            TimeSpan value = new TimeSpan(1, 1, 0);
            _target.BalanceIn = value;

            Assert.AreEqual(value, _target.BalanceIn);
        }
        
        [Test]
        public void VerifyBalanceInWithNoSchedulePeriod()
        {
            _target = new SchedulePeriodModel(new DateOnly(2006, 5, 2), _person, null);
            Assert.AreEqual(TimeSpan.MinValue, _target.BalanceIn);
        }

        [Test]
        public void VerifyBalanceOutWithNoSchedulePeriod()
        {
            _target = new SchedulePeriodModel(new DateOnly(2006, 5, 2), _person, null);
            Assert.AreEqual(TimeSpan.MinValue, _target.BalanceOut);
        }

        [Test]
        public void VerifyExtraWithNoSchedulePeriod()
        {
            _target = new SchedulePeriodModel(new DateOnly(2006, 5, 2), _person, null);
            Assert.AreEqual(TimeSpan.MinValue, _target.Extra);
        }

        [Test]
        public void VerifyBalanceOut()
        {
            TimeSpan value = new TimeSpan(1, 1, 0);
            _target.BalanceOut = value;

            Assert.AreEqual(value, _target.BalanceOut);
        }

        [Test]
        public void VerifyExtra()
        {
            TimeSpan value = new TimeSpan(1, 1, 0);
            _target.Extra = value;

            Assert.AreEqual(value, _target.Extra);
        }

        [Test]
        public void VerifyOverrideDatesCanSet()
        {
            _target.OverrideDaysOff = 8;
            Assert.AreEqual(8, _target.OverrideDaysOff);
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
            DateOnly dateOnly = DateOnly.MinValue.AddDays(10);
            _target.Parent.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(dateOnly));
            _target.PeriodDate = dateOnly;

            Assert.AreEqual(dateOnly.AddDays(1), _target.PeriodDate);
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
            SchedulePeriodModel schedulePeriodModel = new SchedulePeriodModel
                (new DateOnly(2008, 5, 2), _person, null);

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
        public void VerifyResetCanBoldPropertyOfChildAdapters()
        {
            using (GridControl grid = new GridControl())
            {
                SchedulePeriodChildModel adapter1 = EntityConverter.ConvertToOther<ISchedulePeriod,
                    SchedulePeriodChildModel>(_schedulePeriod1);

                SchedulePeriodChildModel adapter2 = EntityConverter.ConvertToOther<ISchedulePeriod,
                    SchedulePeriodChildModel>(_schedulePeriod2);

                adapter1.CanBold = true;
                adapter2.CanBold = true;

                IList<SchedulePeriodChildModel> adapterCollection = new
                    List<SchedulePeriodChildModel>();
                adapterCollection.Add(adapter1);
                adapterCollection.Add(adapter2);

                grid.Tag = adapterCollection;

                _target.GridControl = grid;

                _target.ResetCanBoldPropertyOfChildAdapters();

                IList<SchedulePeriodChildModel> childAdapters = _target.GridControl.Tag as
                                                                IList<SchedulePeriodChildModel>;

                Assert.IsNotNull(childAdapters);
                Assert.AreEqual(2, childAdapters.Count);
                Assert.IsFalse(childAdapters[0].CanBold);
                Assert.IsFalse(childAdapters[1].CanBold);
            }
        }

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenChildCanBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(_schedulePeriod1);
				adapter1.CanBold = true;
				IList<SchedulePeriodChildModel> adapterCollection = new List<SchedulePeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;

				_target.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenParentCanBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(_schedulePeriod1);
				IList<SchedulePeriodChildModel> adapterCollection = new List<SchedulePeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;
				_target.CanBold = true;
				_target.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldNotGetCanBoldOnAdapterAndChildAdaptersWhenParentOrChildCantBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(_schedulePeriod1);
				IList<SchedulePeriodChildModel> adapterCollection = new List<SchedulePeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;
				_target.AdapterOrChildCanBold().Should().Be.False();
			}
		}
	}
}