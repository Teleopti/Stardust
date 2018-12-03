using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class AddLayerViewModelTest
    {
        private AddLayerViewModel<IAbsence> _target;
        private string _title;
        private DateTimePeriod _period;
        private List<IAbsence> _absences;
        private TimeSpan _interval;

        [SetUp]
        public void Setup()
        {
            _interval = TimeSpan.FromMinutes(15);
            _absences = new List<IAbsence>() { AbsenceFactory.CreateAbsence("a1"), AbsenceFactory.CreateAbsence("a2"), AbsenceFactory.CreateAbsence("a3") };
            _period = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
            _title = "Title";

            _target = new AddLayerViewModel<IAbsence>(_absences, _period, _title, _interval, null);
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.AreEqual(_period, _target.SelectedPeriod, "sets the default period as the selected");
            Assert.Contains(_target.SelectedItem, _absences, "returns an absence from the selctable absences");
            Assert.AreEqual(_title, _target.Title);
            Assert.IsFalse(_target.PeriodViewModel.AutoUpdate);
            Assert.AreEqual(_target.PeriodViewModel.Interval, _interval);
            Assert.IsTrue(_target.CanOk);

        }

        [Test]
        public void VerifyShowDetailsPropertiesIsOffByDefault()
        {
            Assert.IsFalse(_target.ShowDetails);
        }

        [Test]
        public void VerifyCanExecuteToggleShowDetailsCommand()
        {
            var commandTester = new TesterForCommandModels();
            Assert.IsTrue(commandTester.CanExecute(_target.ShowDetailsToggleCommand), "By default we should be able to execute ToggleAdvancedCommand");
        }

        [Test]
        public void VerifyRoutedUICommandForToggleShowDetails()
        {
            Assert.AreSame(CommonRoutedCommands.ShowDetails, _target.ShowDetailsToggleCommand.Command);
        }

        [Test]
        public void VerifyExecuteToggleShowDetailsCommand()
        {
            bool details = _target.ShowDetails;
            var commandTester = new TesterForCommandModels();

            commandTester.ExecuteCommandModel(_target.ShowDetailsToggleCommand);
            Assert.AreNotEqual(details, _target.ShowDetails, "ShowAdvanced should have been changed after executing ToggleAdvancedCommand");

            commandTester.ExecuteCommandModel(_target.ShowDetailsToggleCommand);
            Assert.AreEqual(details, _target.ShowDetails, "ShowAdvanced should have been changed after executing ToggleAdvancedCommand");

        }

        [Test]
        public void VerifySetupFromPeriod()
        {

            var setupForPeriod = new TestSetup(_period);
            _target = new AddLayerViewModel<IAbsence>(_absences, setupForPeriod, _title, _interval);
            Assert.AreEqual(_period.StartDateTime,_target.PeriodViewModel.Start);
            Assert.AreEqual(_period.EndDateTime,_target.PeriodViewModel.End);
        }


        internal class TestSetup : ISetupDateTimePeriod
        {
            private readonly DateTimePeriod _period;

            public TestSetup(DateTimePeriod period)
            {
                _period = period;
            }

            public DateTimePeriod Period
            {
                get { return _period; }
            }

        
        }




    }
}