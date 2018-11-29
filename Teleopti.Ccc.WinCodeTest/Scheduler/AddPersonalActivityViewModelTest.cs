using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AddPersonalActivityViewModelTest
    {
        private List<IActivity> _activities;
     
        private AddPersonalActivityViewModel _target;
        private DateTimePeriod _defaultPeriod;

        [SetUp]
        public void Setup()
        {

            _defaultPeriod = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
     
         
            _activities = new List<IActivity> { ActivityFactory.CreateActivity("a1"), ActivityFactory.CreateActivity("a1") };
            _target = new AddPersonalActivityViewModel(_activities, _defaultPeriod,TimeSpan.FromMinutes(15));
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(_defaultPeriod, _target.SelectedPeriod, "sets the default period as the selected");
            Assert.Contains(_target.SelectedItem, _activities, "returns an absence from the selctable absences");
            Assert.AreEqual(UserTexts.Resources.AddPersonalActivity, _target.Title);
            Assert.IsFalse(_target.PeriodViewModel.AutoUpdate);
        }
    }
}
