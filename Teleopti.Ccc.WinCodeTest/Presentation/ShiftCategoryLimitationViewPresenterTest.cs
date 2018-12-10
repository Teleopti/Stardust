using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ShiftCategoryLimitationViewPresenterTest
    {
        private MockRepository _mocks;
        private ShiftCategoryLimitationViewPresenter _target;
        private IList<ISchedulePeriod> _schedulePeriods;
        private IShiftCategoryLimitation _limitationDayWeeklyMax2;
        private IShiftCategoryLimitation _limitationNightPeriodMax2;
        private IShiftCategoryLimitationView _view;
        private IShiftCategory _dayCategory;
        private IShiftCategory _nightCategory;
        private ISchedulePeriod _period1;
        private ISchedulePeriod _period2;


        [SetUp]
        public void Setup()
        {
            Guid _dayGuid = Guid.NewGuid();
            Guid _nightGuid = Guid.NewGuid();
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IShiftCategoryLimitationView>();
            DateOnly dateOnly = new DateOnly(2010, 1, 1);
            _target = new ShiftCategoryLimitationViewPresenter(_view);
            _period1 = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
            _period2 = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly.AddDays(10));
            _dayCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
            _nightCategory = ShiftCategoryFactory.CreateShiftCategory("Night");
            _dayCategory.SetId(_dayGuid);
            _nightCategory.SetId(_nightGuid);
            _limitationDayWeeklyMax2 = new ShiftCategoryLimitation(_dayCategory);
            _limitationNightPeriodMax2 = new ShiftCategoryLimitation(_nightCategory);
            _limitationDayWeeklyMax2.MaxNumberOf = 2;
            _limitationDayWeeklyMax2.Weekly = true;
            _limitationNightPeriodMax2.MaxNumberOf = 2;
            _limitationNightPeriodMax2.Weekly = false;
            _period1.AddShiftCategoryLimitation(_limitationDayWeeklyMax2);
            _period2.AddShiftCategoryLimitation(_limitationNightPeriodMax2);
            _schedulePeriods = new List<ISchedulePeriod> { _period1, _period2 };
            _target.SetSchedulePeriodList(_schedulePeriods);
        }

        [Test]
        public void VerifyInitialize()
        {
            using(_mocks.Record())
            {
                Expect.Call(_view.ShiftCategories).Return(new List<IShiftCategory>());
                _view.SetupGrid();
            }
            _target.Initialize();
        }

        [Test]
        public void VerifyCloneIsUsedWhenAdding()
        {
            IShiftCategoryLimitation addedLimitation = new ShiftCategoryLimitation(_dayCategory);
            _target.AddLimitation(addedLimitation);
            Assert.AreNotSame(_period1.ShiftCategoryLimitationCollection()[0], _period2.ShiftCategoryLimitationCollection()[1]);
        }

        [Test]
        public void VerifyCanAddShiftCategoryLimitationOnAllSchedulePeriods()
        {
            IShiftCategoryLimitation addedLimitation = new ShiftCategoryLimitation(_dayCategory);
            _target.AddLimitation(addedLimitation);
            Assert.AreEqual(1, _period1.ShiftCategoryLimitationCollection().Count);
            Assert.AreEqual(2, _period2.ShiftCategoryLimitationCollection().Count);
        }

        [Test]
        public void VerifyCanRemoveShiftCategoryLimitationOnAllSchedulePeriods()
        {
            _target.RemoveLimitation(_dayCategory);
            Assert.AreEqual(0, _period1.ShiftCategoryLimitationCollection().Count);
            Assert.AreEqual(1, _period2.ShiftCategoryLimitationCollection().Count);
        }

        [Test]
        public void VerifyCanSetNumberOfOnAllSchedulePeriods()
        {
            _target.SetNumberOf(_dayCategory, 77);
            Assert.AreEqual(77, _limitationDayWeeklyMax2.MaxNumberOf);
            Assert.AreEqual(2, _limitationNightPeriodMax2.MaxNumberOf);
        }

        [Test]
        public void VerifyCanSetWeekPropertyOnAllSchedulePeriods()
        {
            _target.SetWeekProperty(_nightCategory, true);
            Assert.IsTrue(_limitationNightPeriodMax2.Weekly);
        }

        [Test]
        public void CanSetSchedulePeriodList()
        {

            Assert.IsNotNull(_target.SchedulePeriods);
            _target.SetSchedulePeriodList(_schedulePeriods);
            Assert.AreEqual(_schedulePeriods.Count, _target.SchedulePeriods.Count);
        }

        [Test]
        public void VerifySetShiftCategories()
        {
            IList<IShiftCategory> list = new List<IShiftCategory>();
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");
            list.Add(shiftCategory);
            _target.SetShiftCategories(list);

            Assert.AreEqual(1, _target.CombinedLimitations.Count);
        }

        [Test]
        public void VerifyCreateCombined()
        {
            IList<IShiftCategory> list = new List<IShiftCategory>();
            list.Add(_dayCategory);
            list.Add(_nightCategory);
            IShiftCategory another = ShiftCategoryFactory.CreateShiftCategory("xx");
            list.Add(another);
            _target.SetShiftCategories(list);

            _target.SetSchedulePeriodList(_schedulePeriods);
            _target.CreateCombined();
            Assert.AreEqual(2, _target.GetCurrentMaxNumberOf(_dayCategory));

            
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(another);
            _period1.AddShiftCategoryLimitation(limitation);
            _target.CreateCombined();
            Assert.AreEqual(0, _target.GetCurrentMaxNumberOf(another));

        }

        [Test]
        public void VerifyOnQueryShiftCategoryCellInfo()
        {
            IShiftCategory day = ShiftCategoryFactory.CreateShiftCategory("Day");
            IShiftCategory night = ShiftCategoryFactory.CreateShiftCategory("Night");
            _target.AddShiftCategory(day);
            _target.AddShiftCategory(night);

            Assert.AreEqual(day, _target.OnQueryShiftCategoryCellInfo(1));
            Assert.AreEqual(night, _target.OnQueryShiftCategoryCellInfo(2));
        }
        [Test]
        public void VerifyAddShiftCategory()
        {
            IShiftCategory day = ShiftCategoryFactory.CreateShiftCategory("Day");
            _target.AddShiftCategory(day);
            Assert.AreEqual(1, _target.ShiftCategories.Count);
            _target.AddShiftCategory(day);
            Assert.AreEqual(1, _target.ShiftCategories.Count);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyOnQueryHeaderInfo()
        {
            Assert.AreEqual(Resources.ShiftCategory, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(0, 0));
            Assert.AreEqual(Resources.Limit, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(0, 1));
            Assert.AreEqual(Resources.PerPeriod, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(0, 2));
            Assert.AreEqual(Resources.PerWeek, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(0, 3));
            Assert.AreEqual(Resources.Max, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(0, 4));
            Assert.AreEqual(string.Empty, ShiftCategoryLimitationViewPresenter.OnQueryHeaderInfo(1, 0));
        }
        [Test]
        public void VerifyOnQueryLimitationCellInfo()
        {
            IList<IShiftCategory> list = new List<IShiftCategory>();
            list.Add(_dayCategory);
            list.Add(_nightCategory);
            _target.SetShiftCategories(list);

            _target.SetSchedulePeriodList(new List<ISchedulePeriod>());

            Assert.AreEqual("false", _target.OnQueryLimitationCellInfo(_dayCategory, 1));
            Assert.AreEqual("false", _target.OnQueryLimitationCellInfo(_dayCategory, 2));
            Assert.AreEqual("false", _target.OnQueryLimitationCellInfo(_dayCategory, 3));

            _target.SetSchedulePeriodList(new List<ISchedulePeriod>{_period1});
            Assert.AreEqual("true", _target.OnQueryLimitationCellInfo(_dayCategory, 1));
            Assert.AreEqual("false", _target.OnQueryLimitationCellInfo(_dayCategory, 2));
            Assert.AreEqual("true", _target.OnQueryLimitationCellInfo(_dayCategory, 3));

            _target.SetSchedulePeriodList(new List<ISchedulePeriod> { _period2 });
            Assert.AreEqual("true", _target.OnQueryLimitationCellInfo(_nightCategory, 1));
            Assert.AreEqual("true", _target.OnQueryLimitationCellInfo(_nightCategory, 2));
            Assert.AreEqual("false", _target.OnQueryLimitationCellInfo(_nightCategory, 3));

            IShiftCategoryLimitation limitationPeriodMax1 = new ShiftCategoryLimitation(_dayCategory);
            limitationPeriodMax1.Weekly = false;
            limitationPeriodMax1.MaxNumberOf = 1;
            _period2.AddShiftCategoryLimitation(limitationPeriodMax1);
            _target.SetSchedulePeriodList(new List<ISchedulePeriod> { _period1, _period2 });
            Assert.AreEqual("true", _target.OnQueryLimitationCellInfo(_dayCategory, 1));
            Assert.AreEqual("", _target.OnQueryLimitationCellInfo(_dayCategory, 2));
            Assert.AreEqual("", _target.OnQueryLimitationCellInfo(_dayCategory, 3));

            ISchedulePeriod anotherPeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly());
            _target.SetSchedulePeriodList(new List<ISchedulePeriod> { _period1, _period2, anotherPeriod });

            Assert.AreEqual("", _target.OnQueryLimitationCellInfo(_dayCategory, 1));
            Assert.AreEqual("", _target.OnQueryLimitationCellInfo(_dayCategory, 2));
            Assert.AreEqual("", _target.OnQueryLimitationCellInfo(_dayCategory, 3));

        }

		[Test]
		public void ShouldHaveASortedListOfShiftCategories()
		{
			IList<IShiftCategory> list = new List<IShiftCategory> { _nightCategory, _dayCategory };
			_target.SetShiftCategories(list);

			Assert.That(_target.ShiftCategories[0], Is.EqualTo(_dayCategory));
		}
        
    }
}
