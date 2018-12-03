using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;

using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AddActivityViewModelTest
    {
        private List<IActivity> _activities;
        private List<IShiftCategory> _shiftCategories;
        private IShiftCategory _category1;
        private IShiftCategory _category2;
        private AddActivityViewModel _target;
        private DateTimePeriod _defaultPeriod;

        [SetUp]
        public void Setup()
        {

            _defaultPeriod = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
            _category1 = ShiftCategoryFactory.CreateShiftCategory("cat1");
            _category2 = ShiftCategoryFactory.CreateShiftCategory("cat2");
            _shiftCategories = new List<IShiftCategory>()
                                   {    _category1,
                                        _category2             
                                   };
            _activities = new List<IActivity> { ActivityFactory.CreateActivity("a1"), ActivityFactory.CreateActivity("a1") };
            _target = new AddActivityViewModel(_activities,_shiftCategories,_defaultPeriod,TimeSpan.FromMinutes(12), null);
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(_defaultPeriod, _target.SelectedPeriod, "sets the default period as the selected");
            Assert.Contains(_target.SelectedItem, _activities, "returns an absence from the selctable absences");
            Assert.AreEqual(UserTexts.Resources.AddActivity, _target.Title);
            Assert.IsFalse(_target.PeriodViewModel.AutoUpdate);
        }

        [Test]
        public void VerifySelectShiftCategories()
        {
            Assert.IsTrue(_target.ShiftCategories.MoveCurrentTo(_category1), "selects the first category");
            Assert.AreEqual(_target.SelectedShiftCategory,_category1);
            Assert.IsTrue(_target.ShiftCategories.MoveCurrentTo(_category2), "selects the second category");
            Assert.AreEqual(_target.SelectedShiftCategory, _category2);
        }

        [Test]
        public void VerifyMinDateIsSetToStartDate()
        {
            Assert.AreEqual(_defaultPeriod.StartDateTime.Date,_target.PeriodViewModel.Min.Date);

        }
    }
}
