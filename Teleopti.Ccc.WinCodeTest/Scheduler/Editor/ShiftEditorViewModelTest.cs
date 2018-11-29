using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Is = Rhino.Mocks.Constraints.Is;
using System;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;



namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    [TestFixture]
    public class ShiftEditorViewModelTest
    {
        private ShiftEditorViewModel _target;
        private MockRepository _mocker;
        private IScheduleDay _mockedPart;
        private IScheduleDay _partForTest;
        private IShiftEditorObserver _observer1;
        private IShiftEditorObserver _observer2;
        private DateTimePeriod _selectedPeriod;
        Microsoft.Practices.Composite.Events.IEventAggregator _eventAggregator;
		private IEditableShiftMapper _editableShiftMapper;
		private IDisposable auth;

		[SetUp]
        public void Setup()
        {
            _selectedPeriod = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
            
            _mocker = new MockRepository();
            _mockedPart = _mocker.StrictMock<IScheduleDay>();
            _eventAggregator = new Microsoft.Practices.Composite.Events.EventAggregator();
	        _editableShiftMapper = _mocker.StrictMock<IEditableShiftMapper>();
            _target = new ShiftEditorViewModel(_eventAggregator, new CreateLayerViewModelService(), true, _editableShiftMapper);
            _partForTest = new SchedulePartFactoryForDomain().CreatePartWithMainShift();

			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
		}

		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
			;
		}

        [Test]
        public void VerifyPropertiesOnAreasAreSet()
        {
            //For now, its only one area
            Assert.AreEqual(_target.Layers, _target.AllLayers.Layers);
        }

		[Test]
		public void ShouldNotSetShiftCategoryIfValueIsNull()
		{
			var shiftCategory = _mocker.StrictMock<IShiftCategory>();
			_target.Category = shiftCategory;
			_target.Category = null;
			Assert.AreEqual(shiftCategory, _target.Category);
		}
 
        [Test]
        public void VerifyObservers()
        {
            IShiftEditorObserver observer = _mocker.StrictMock<IShiftEditorObserver>();
            _target.AddObserver(observer);
            Assert.AreEqual(_target.Observers.Count,1);
            _target.AddObserver(observer);
            Assert.AreEqual(_target.Observers.Count, 1);
            _target.RemoveObserver(observer);
            _target.RemoveObserver(observer);
            Assert.AreEqual(_target.Observers.Count, 0);
        }

        #region commands

    
        [Test]
        public void VerifyAddActivityCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddActivityCommand));
            Assert.AreEqual(_target.AddActivityCommand.Text,UserTexts.Resources.AddActivityThreeDots);
            
            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddActivity(_mockedPart, null, null)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorAddActivity(_mockedPart, null, null)).IgnoreArguments();
            }
            using(_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddActivityCommand);
            }
        }

        [Test]
        public void VerifyAddOvertimeCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddOvertimeCommand));
            Assert.AreEqual(_target.AddOvertimeCommand.Text, UserTexts.Resources.AddOvertime);

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddOvertime(_mockedPart, null)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorAddOvertime(null, null)).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddOvertimeCommand);
            }
        }

        [Test]
        public void VerifyAddAbsenceCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddAbsenceCommand));
            Assert.AreEqual(_target.AddAbsenceCommand.Text, UserTexts.Resources.AddAbsenceThreeDots);

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddAbsence(_mockedPart, null)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorAddAbsence(_mockedPart, null)).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddAbsenceCommand);
            }
        }

        [Test]
        public void VerifyAddPersonalShiftCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddPersonalShiftCommand));
            Assert.AreEqual(_target.AddPersonalShiftCommand.Text, UserTexts.Resources.AddPersonalActivityThreeDots);

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddPersonalShift(_mockedPart, null)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorAddPersonalShift(_mockedPart, null)).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddPersonalShiftCommand);
            }
        }

        [Test]
        public void VerifyUpdateCommand()
        {
            
            TesterForCommandModels models = SetupForCommandTest();
            Assert.AreEqual(UserTexts.Resources.Update,_target.UpdateCommand.Text);
            Assert.IsTrue(models.CanExecute(_target.UpdateCommand));

            //UpdateCommand should call observers
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorUpdateCommandExecuted(_mockedPart)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorUpdateCommandExecuted(_mockedPart)).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.UpdateCommand);
            }

        }

        [Test]
        public void VerifyCommitChangesCommand()
        {

            TesterForCommandModels models = SetupForCommandTest();
            Assert.AreEqual(UserTexts.Resources.Update, _target.CommitChangesCommand.Text);
            Assert.IsTrue(models.CanExecute(_target.CommitChangesCommand));

            //UpdateCommand should call observers
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorCommitChangesExecuted(_mockedPart)).IgnoreArguments();
                Expect.Call(() => _observer2.EditorCommitChangesExecuted(_mockedPart)).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.CommitChangesCommand);
            }

        }

        [Test]
        public void VerifyCreateMeetingCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            VerifyApplicationCommandModel(_target.CreateMeetingCommand,
                                          DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            Assert.AreEqual(UserTexts.Resources.CreateMeeting, _target.CreateMeetingCommand.Text);
            Assert.AreEqual(ShiftEditorRoutedCommands.CreateMeeting,_target.CreateMeetingCommand.Command);
            
           

            Assert.IsTrue(models.CanExecute(_target.CreateMeetingCommand),
                          "should always be able to execute (we dont have to check authotization here because its an app-command)");
            
           // UpdateCommand should call observers
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorCreateMeetingExecuted(null));
                Expect.Call(() => _observer2.EditorCreateMeetingExecuted(null));
            }
            using (_mocker.Playback())
            {
					models.ExecuteCommandModel(_target.CreateMeetingCommand);
            }
        }

        [Test]
        public void VerifyDeleteMeetingCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.AreEqual(UserTexts.Resources.DeleteMeeting, _target.DeleteMeetingCommand.Text);
            Assert.AreEqual(ShiftEditorRoutedCommands.DeleteMeeting, _target.DeleteMeetingCommand.Command);

            VerifyApplicationCommandModel(_target.DeleteMeetingCommand,DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            VerifyCanOnlyExecuteIfMeetingIsSelected(_target.DeleteMeetingCommand);

            IPersonMeeting selectedMeeting = CreateAndSelectAMeeting();
            // DeleteMeetingCommand should call observers with the selected meeting
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorDeleteMeetingExecuted(selectedMeeting));
                Expect.Call(() => _observer2.EditorDeleteMeetingExecuted(selectedMeeting));
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.DeleteMeetingCommand);
            }
        }

        [Test]
        public void VerifyRemoveParticipantCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.AreEqual(UserTexts.Resources.RemoveParticipant, _target.RemoveParticipantCommand.Text);
            Assert.AreEqual(ShiftEditorRoutedCommands.RemoveParticipant, _target.RemoveParticipantCommand.Command);
            VerifyApplicationCommandModel(_target.RemoveParticipantCommand,
                                          DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            VerifyCanOnlyExecuteIfMeetingIsSelected(_target.RemoveParticipantCommand);

            IPersonMeeting selectedMeeting = CreateAndSelectAMeeting();
            // RemoveParticipantCommand should call observers with the selected meeting
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorRemoveParticipantsFromMeetingExecuted(selectedMeeting));
                Expect.Call(() => _observer2.EditorRemoveParticipantsFromMeetingExecuted(selectedMeeting));
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.RemoveParticipantCommand);
            }
        }

        [Test]
        public void VerifyEditMeetingCommand()
        {
            TesterForCommandModels models = SetupForCommandTest();
            Assert.AreEqual(UserTexts.Resources.EditMeeting, _target.EditMeetingCommand.Text);
            Assert.AreEqual(ShiftEditorRoutedCommands.EditMeeting, _target.EditMeetingCommand.Command);
            VerifyApplicationCommandModel(_target.EditMeetingCommand,
                                          DefinedRaptorApplicationFunctionPaths.ModifyMeetings);
            VerifyCanOnlyExecuteIfMeetingIsSelected(_target.EditMeetingCommand);

            IPersonMeeting selectedMeeting = CreateAndSelectAMeeting();
            // EditMeetingCommand should call observers with the selected meeting
           
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorEditMeetingExecuted(selectedMeeting));
                Expect.Call(() => _observer2.EditorEditMeetingExecuted(selectedMeeting));
            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.EditMeetingCommand);
            }
        }

        [Test]
        public void VerifyMoveUpCommand()
        {
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            ILayerViewModel model = _mocker.StrictMock<ILayerViewModel>();
            _target.SelectedLayer = model;
           
            using(_mocker.Record())
            {
                Expect.Call(model.CanMoveUp).Return(false);
                Expect.Call(model.MoveUp);
                
            }

            using(_mocker.Playback())
            {
               Assert.IsFalse(testerForCommandModels.CanExecute(_target.MoveUpCommand));
                testerForCommandModels.ExecuteCommandModel(_target.MoveUpCommand);
            }

            Assert.AreEqual(_target.MoveUpCommand.Command, ShiftEditorRoutedCommands.MoveUp);
        }

        [Test]
        public void VerifyMoveDownCommand()
        {
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            ILayerViewModel model = _mocker.StrictMock<ILayerViewModel>();
            _target.SelectedLayer = model;

            using (_mocker.Record())
            {
                Expect.Call(model.CanMoveDown).Return(false);
                Expect.Call(model.MoveDown);
            }

            using (_mocker.Playback())
            {
                Assert.IsFalse(testerForCommandModels.CanExecute(_target.MoveDownCommand));
                testerForCommandModels.ExecuteCommandModel(_target.MoveDownCommand);
            }

            Assert.AreEqual(_target.MoveUpCommand.Command,ShiftEditorRoutedCommands.MoveUp);
        }


        [Test]
        public void VerifyCannotMoveIsFalseIfNoLayerIsSelected()
        {
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            Assert.IsNull(_target.SelectedLayer);
            Assert.IsFalse(testerForCommandModels.CanExecute(_target.MoveUpCommand));
            Assert.IsFalse(testerForCommandModels.CanExecute(_target.MoveDownCommand));
        }

        #endregion //commands

        [Test]
        public void VerifyCurrentLayer()
        {
            #region setup stub
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            IActivity activity = new Activity("test");
            DateTimePeriod period= new DateTimePeriod(2001,1,1,2001,1,2);
            var layer = new MainShiftLayer(activity, period);
						ILayerViewModel model = new MainShiftLayerViewModel(null, layer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
            
            #endregion

            _target.SelectLayer(model);
                Assert.AreEqual(_target.SelectedLayer, model, "The currentlayer has been set");
                Assert.IsTrue(listener.HasFired("SelectedLayer"), "PropertyChanged has fired");
                Assert.AreEqual(_target.EditLayer.Layer, model, "The EditLayer-layer has also been set");

           
        }

        [Test]
        public void VerifyDeleteCommandDeletesCurrentLayer()
        {
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
           
            ILayerViewModel model = CreateLayerViewModel();

            Assert.IsNull(_target.SelectedLayer);
            
            //Should not be able to execute because there is no Layer selected
            Assert.IsFalse(testerForCommandModels.CanExecute(_target.DeleteCommand));

            _target.SelectLayer(model);

            Assert.IsTrue(testerForCommandModels.CanExecute(_target.DeleteCommand));
            testerForCommandModels.ExecuteCommandModel(_target.DeleteCommand);

            //Model is removed
            Assert.IsFalse(_target.Layers.Contains(model));
        }

        [Test]
        public void VerifySettings()
        {
            TimeSpan threeMinutes = TimeSpan.FromMinutes(3);
            Assert.AreEqual(_target.Settings.Target,_target,"Just make sure that the target is set to the viewmodel");
            _target.Settings.Interval = threeMinutes;
            Assert.AreEqual(_target.Layers.Interval,threeMinutes);
            Assert.AreEqual(_target.EditLayer.Period.Interval, threeMinutes, "and that the interval of the editlayerviewmodel is changed as well");


            //Test target prop
            _target.Interval = TimeSpan.FromMinutes(2);
            Assert.AreEqual(TimeSpan.FromMinutes(2), _target.Interval);

        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifySurroundingTimeIsAddedToLoadedPart()
        {
            //When a new part is loaded, the timeline should show extra time before and after:
            _target.LoadSchedulePart(_partForTest);
            DateTime start =
                _target.Layers.TotalDateTimePeriod().StartDateTime.Subtract(_target.SurroundingTime);
            DateTime end = 
                _target.Layers.TotalDateTimePeriod().EndDateTime.Add(_target.SurroundingTime);
            Assert.AreEqual(start,_target.Timeline.Period.StartDateTime);
            Assert.AreEqual(end,_target.Timeline.Period.EndDateTime);
        }

        private TesterForCommandModels SetupForPeriodCommandTest()
        {
            _target.Timeline.SelectedPeriod = _selectedPeriod;
            _observer1 = _mocker.StrictMock<IShiftEditorObserver>();
            _target.AddObserver(_observer1);
            return new TesterForCommandModels();
        }


        private TesterForCommandModels SetupForCommandTest()
        {
            _observer1 = _mocker.StrictMock<IShiftEditorObserver>();
            _observer2 = _mocker.StrictMock<IShiftEditorObserver>();
            _target.AddObserver(_observer1);
            _target.AddObserver(_observer2);
            return new TesterForCommandModels();
        }

        private static ILayerViewModel CreateLayerViewModel()
        {
            IActivity activity = new Activity("test");
            DateTimePeriod period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
            var layer = new MainShiftLayer(activity, period);
						return new MainShiftLayerViewModel(null, layer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
        }

        private static void VerifyApplicationCommandModel(CommandModel commandModel,string appFunction)
        {
            ApplicationCommandModel applicationCommandModel = commandModel as ApplicationCommandModel;
            Assert.IsNotNull(applicationCommandModel,commandModel +"is not ApplicationCommand");
            Assert.AreEqual(applicationCommandModel.FunctionPath, appFunction, appFunction + " != " + applicationCommandModel.FunctionPath);
        }

        private void VerifyCanOnlyExecuteIfMeetingIsSelected(CommandModel model)
        {
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            var layer = new MainShiftLayer(new Activity("asfdgh"), new DateTimePeriod(2001, 1, 1, 2001, 2, 2));
						MainShiftLayerViewModel mainShiftLayerViewModel = new MainShiftLayerViewModel(null, layer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
            _target.SelectLayer(mainShiftLayerViewModel);
            Assert.IsFalse(testerForCommandModels.CanExecute(model), "Should not be able to execute if the selected layer isnt a meeting");
            CreateAndSelectAMeeting();
            Assert.IsTrue(testerForCommandModels.CanExecute(_target.EditMeetingCommand));            
        }

        private IPersonMeeting CreateAndSelectAMeeting()
        {   
            IPerson person = PersonFactory.CreatePerson();
            MeetingPerson meetingPerson = new MeetingPerson(person, false);
            List<IMeetingPerson> meetingPeople = new List<IMeetingPerson>();
            meetingPeople.Add(meetingPerson);
            Meeting _meeting = new Meeting(person, meetingPeople, "subject", "location", "description", ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());
            PersonMeeting _personMeeting = new PersonMeeting(_meeting, meetingPerson, new DateTimePeriod(2001,1,1,2001,1,2));
            MeetingLayerViewModel meetingLayerViewModel = new MeetingLayerViewModel(null, _personMeeting, null);
           
            _target.SelectLayer(meetingLayerViewModel);
            return _personMeeting;
        }


        #region add with period
        //    AddActivityWithPeriod,
        //AddOvertimeWithPeriod,
        //AddAbsenceWithPeriod,
        //AddPersonalWithPeriod
        [Test]
        public void VerifyAddActivityWithPeriodCommand()
        {
           
            TesterForCommandModels models = SetupForPeriodCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddActivityWithPeriodCommand));
   
            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddActivity(_mockedPart, _selectedPeriod, null)).IgnoreArguments()
                     .Constraints(Is.Anything(),Is.Equal(_selectedPeriod), Is.Null());

            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddActivityWithPeriodCommand);
            }
        }

        [Test]
        public void VerifyAddOvertimeWithPeriodCommand()
        {

            TesterForCommandModels models = SetupForPeriodCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddOvertimeWithPeriodCommand));

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddOvertime(_mockedPart, _selectedPeriod)).IgnoreArguments()
                     .Constraints(Is.Anything(), Is.Equal(_selectedPeriod));

            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddOvertimeWithPeriodCommand);
            }
        }

        [Test]
        public void VerifyAddAbsenceWithPeriodCommand()
        {
            TesterForCommandModels models = SetupForPeriodCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddAbsenceWithPeriodCommand));

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddAbsence(_mockedPart, _selectedPeriod)).IgnoreArguments()
                     .Constraints(Is.Anything(), Is.Equal(_selectedPeriod));

            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddAbsenceWithPeriodCommand);
            }
        }

        [Test]
        public void VerifyAddPersonalWithPeriodCommand()
        {
            TesterForCommandModels models = SetupForPeriodCommandTest();
            Assert.IsTrue(models.CanExecute(_target.AddPersonalWithPeriodShiftCommand));

            //Execute:
            using (_mocker.Record())
            {
                Expect.Call(() => _observer1.EditorAddPersonalShift(_mockedPart, _selectedPeriod)).IgnoreArguments()
                     .Constraints(Is.Anything(), Is.Equal(_selectedPeriod));

            }
            using (_mocker.Playback())
            {
                models.ExecuteCommandModel(_target.AddPersonalWithPeriodShiftCommand);
            }
        }

        #endregion //add with period

    }
}
