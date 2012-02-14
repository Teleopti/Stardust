using System;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction.Commands
{
    [TestFixture]
    public class AddStudentAvailabilityCommandModelTest
    {
        private IPerson _person;
        private DateTime _utcDate;
        private DateTimePeriod _period;
        private RestrictionEditorViewModel _commandTarget;
        private IScheduleDay _partForTest;
        private IScheduleDictionary _scheduleDictionary;
        private IScenario _scenario;
        private TesterForCommandModels _models;
        private RestrictionEditorViewModel _modelWithoutSchedulePart;

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _person = PersonFactory.CreatePerson("Test");
            _utcDate = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(_utcDate, _utcDate.AddDays(10));
            _scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(_period));
            _partForTest = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(2001,1,1));
            _commandTarget = new RestrictionEditorViewModel(_partForTest,new List<IActivity>(),new List<IDayOffTemplate>(),new List<IShiftCategory>());
            _modelWithoutSchedulePart = new RestrictionEditorViewModel(null, new List<IActivity>(), new List<IDayOffTemplate>(), new List<IShiftCategory>());

            _models = new TesterForCommandModels();
        }

        [Test]
        public void VerifyApplicationFunction()
        {
            var applicationCommandModel = _commandTarget.AddStudentAvailabilityCommand as ApplicationCommandModel;
            Assert.IsNotNull(applicationCommandModel);
            Assert.AreEqual(applicationCommandModel.FunctionPath, DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
        }

        [Test]
        public void VerifyCanExecute()
        {
            CanExecuteRoutedEventArgs argsWithPart = _models.CreateCanExecuteRoutedEventArgs();
            CanExecuteRoutedEventArgs argsWithoutPart = _models.CreateCanExecuteRoutedEventArgs();
            var command = new AddStudentAvailabilityCommandModel(_commandTarget);
            command.OnQueryEnabled(null,argsWithPart);
            Assert.IsTrue(argsWithPart.CanExecute);
            command = new AddStudentAvailabilityCommandModel(_modelWithoutSchedulePart);
            command.OnQueryEnabled(null, argsWithoutPart);
            Assert.IsFalse(argsWithoutPart.CanExecute);
        }

        [Test]
        public void VerifyText()
        {
            Assert.AreEqual(UserTexts.Resources.AddStudentAvailability, _commandTarget.AddStudentAvailabilityCommand.Text);
        }

        [Test]
        public void VerifyExecute()
        {
            Assert.AreEqual(0, _commandTarget.RestrictionModels.Count);
            CommandModel command = _commandTarget.AddStudentAvailabilityCommand;
            command.OnExecute(_commandTarget, null);
            
            var createdpersonRestriction = (StudentAvailabilityDay)_commandTarget.RestrictionModels[0].Restriction.Parent;
            Assert.IsFalse(createdpersonRestriction.NotAvailable);
            Assert.AreEqual(_person, createdpersonRestriction.Person);
            Assert.AreEqual(_partForTest.Period.StartDateTime.Date, createdpersonRestriction.RestrictionDate.Date);
            Assert.AreEqual(1, _commandTarget.RestrictionModels.Count, "ViewModel created");
        }

        [Test]
        public void VerifyCommandModelUsesAddStudentAvailabilityCommand()
        {
            Assert.AreSame(_commandTarget.AddStudentAvailabilityCommand.Command,RestrictionEditorRoutedCommands.AddStudentAvailability);
        }
    }
}
