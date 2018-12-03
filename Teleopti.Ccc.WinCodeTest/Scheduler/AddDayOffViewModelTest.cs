using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AddDayOffViewModelTest
    {
        private List<IDayOffTemplate> _selectables;
        private AddDayOffViewModel _target;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _selectables = new List<IDayOffTemplate> { 
                                                         DayOffFactory.CreateDayOff(new Description("Day Off", "DO")), 
                                                         DayOffFactory.CreateDayOff(new Description("Illness", "IL")) };
            _period = new DateTimePeriod(2001, 1, 1, 2001, 2, 2);
            _target = new AddDayOffViewModel(_selectables, _period);
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(TimeSpan.FromDays(1),_target.PeriodViewModel.Interval,"The default interval is one day");
            Assert.IsNotNull(_target.PeriodViewModel,"just anything");
            Assert.Contains(_target.SelectedItem,_selectables,"returns an dayofftemplate from the selctable absences");
            Assert.AreEqual(UserTexts.Resources.AddDayOff, _target.Title);
            Assert.AreEqual(_period, _target.PeriodViewModel.DateTimePeriod);
        }
    }
}