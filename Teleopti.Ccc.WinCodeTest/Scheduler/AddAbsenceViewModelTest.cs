using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{

    [TestFixture]
    public class AddAbsenceViewModelTest
    {
        private List<IAbsence> _selectables;
        private AddAbsenceViewModel _target;
        private DateTimePeriod _defaultPeriod;
        private IAbsence _absence1;
        private IAbsence _absence2;
        private IAbsence _absence3;
        private ISetupDateTimePeriod _setupDateTimePeriodToSelectedSchedules;

        [SetUp]
        public void Setup()
        {
            _absence1 = AbsenceFactory.CreateAbsence("a1");
            _absence2= AbsenceFactory.CreateAbsence("a2");
            _absence3 = AbsenceFactory.CreateAbsence("a3");
            _defaultPeriod = new DateTimePeriod(2001,1,1,2001,1,3);
            _selectables = new List<IAbsence> {_absence1,_absence2,_absence3};
            _setupDateTimePeriodToSelectedSchedules = new SetupDateTimePeriodToDefaultPeriod(_defaultPeriod);
            _target = new AddAbsenceViewModel(_selectables, _setupDateTimePeriodToSelectedSchedules, TimeSpan.FromMinutes(15));
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(_setupDateTimePeriodToSelectedSchedules.Period, _target.SelectedPeriod, "sets the default period as the selected");
            Assert.Contains(_target.SelectedItem,_selectables,"returns an absence from the selctable absences");
            Assert.AreEqual(UserTexts.Resources.AddAbsence, _target.Title);
            Assert.IsFalse(_target.PeriodViewModel.AutoUpdate);
        }

        [Test]
        public void VerifyCanSelectAbsence()
        {
            _target.Payloads.MoveCurrentTo(_absence2);
            Assert.AreEqual(_absence2,_target.SelectedItem);
        }

        [Test]
        public void VerifyDefaultPayloadIsSelected()
        {
            Assert.AreEqual(_absence1,_target.SelectedItem);
        }
    }
}