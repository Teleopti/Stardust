using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class AbsenceLayerViewModelTest
    {
        private readonly SchedulePartFactoryForDomain _factory = new SchedulePartFactoryForDomain();
        private readonly TesterForCommandModels _models = new TesterForCommandModels();
        private readonly IEventAggregator _eventAggregator = new EventAggregator();
		private AbsenceLayerViewModel _target;
		private IPersonAbsence _personAbsence;
		private IScheduleDay scheduleDay;
		private PropertyChangedListener _listener;
		private IPerson person;
	    private TesterForCommandModels _testerForCommandModels;
	    private DateTimePeriod _period;
	    private bool _expectMovePermitted = true;
	    private bool Opaque => false;

	    [SetUp]
	    public void Setup()
	    {
		    _listener = new PropertyChangedListener();
		    _testerForCommandModels = new TesterForCommandModels();
		    scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
		    person = PersonFactory.CreatePerson();
		    _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
		    var layerWithPayload = new AbsenceLayer(new Absence(), _period);
			_personAbsence = new PersonAbsence(person, new Scenario(), layerWithPayload);
		    scheduleDay.Stub(x => x.Person).Return(person);
		    var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
		    scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);

		    _target = new AbsenceLayerViewModel(MockRepository.GenerateMock<ILayerViewModelObserver>(), _personAbsence, null, new FullPermission());
	    }

		[Test]
        public void VerifyCannotMoveAbsenceLayerVertical()
        {
	        LayerViewModelCollection collection = new LayerViewModelCollection(_eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission());
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            collection.AddFromSchedulePart(part);

	        //Get the first absencelayer:
            ILayerViewModel model = collection.First(l => l is AbsenceLayerViewModel);
            Assert.IsFalse(model.CanMoveUp);
            Assert.IsFalse(model.CanMoveDown);
        }

        [Test]
        public void VerifyDeleteCommandCallsObserver()
        {
	        var observer = MockRepository.GenerateMock<ILayerViewModelObserver>();
         
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            var absenceLayer = part.PersonAbsenceCollection().First().Layer;
            var model = new AbsenceLayerViewModel(observer, new PersonAbsence(part.Person, part.Scenario, absenceLayer), _eventAggregator);

	        _models.ExecuteCommandModel(model.DeleteCommand);

	        observer.AssertWasCalled(x => x.RemoveAbsence(model, absenceLayer, model.SchedulePart));
		}
		
	    [Test]
		public void VerifyCorrectDescription()
		{
			Assert.AreEqual(UserTexts.Resources.Absence, _target.LayerDescription);
		}
		
		[Test]
		public void VerifyProperties()
		{
			_target.SchedulePart = scheduleDay;

			var payloadFromLayer = _personAbsence.Layer.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(person), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(person).Name, _target.Description);
			Assert.AreEqual(_personAbsence.Layer.Period, _target.Period);
			Assert.AreEqual(TimeSpan.FromMinutes(15), _target.Interval);
			Assert.IsFalse(_target.IsChanged);
			Assert.AreSame(scheduleDay, _target.SchedulePart);
			Assert.IsFalse(_target.CanMoveAll);
			Assert.AreEqual(Opaque, _target.Opaque);

			_target.SchedulePart = scheduleDay;
			_target.Interval = TimeSpan.FromHours(1);

			Assert.AreEqual(scheduleDay, _target.SchedulePart);
			Assert.AreEqual(TimeSpan.FromHours(1), _target.Interval);
		}

		[Test]
		public void VerifyCanMoveAll()
		{
			bool succeeded = false;
			_target.PropertyChanged += (x, y) =>
			{
				succeeded = true;
				Assert.AreEqual("CanMoveAll", y.PropertyName);
			};
			_target.CanMoveAll = true;
			Assert.IsTrue(succeeded);
		}

		[Test]
		public void VerifyIsSelected()
		{
			Assert.IsFalse(_target.IsSelected);

			_listener.ListenTo(_target);
			_target.IsSelected = true;
			Assert.IsTrue(_listener.HasFired("IsSelected"));
			Assert.IsTrue(_target.IsSelected);

		}

		[Test]
		public void VerifyCanSetIsChanged()
		{
			bool succeeded = false;
			_target.PropertyChanged += (x, y) =>
			{
				succeeded = true;
				Assert.AreEqual("IsChanged", y.PropertyName);
			};
			_target.IsChanged = true;
			Assert.IsTrue(succeeded);
		}


		[Test]
		public void UpdatePeriod_WhenCalled_ShouldSetIsChangedToFalse()
		{
			_target.IsChanged = true;
			_target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			_target.UpdatePeriod();

			Assert.IsFalse(_target.IsChanged);
		}

		[Test]
		public void UpdatePeriod_WhenCalled_ShouldCallUpdateAllMovedLayersOnObserver()
		{
			var layerObserver = MockRepository.GenerateMock<ILayerViewModelObserver>();

			_target = new AbsenceLayerViewModel(layerObserver, _personAbsence, new EventAggregator()) {IsChanged = true};

			_target.UpdatePeriod();

            layerObserver.AssertWasCalled(l => l.UpdateAllMovedLayers());
		}

		[Test]
		public void VerifyStartTimeChangedWithSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = GetPanel();
					_target.SchedulePart = scheduleDay;
					_target.StartTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		private DateTimePeriodPanel GetPanel()
		{
			DateTimePeriodPanel panel = new DateTimePeriodPanel();
			FieldInfo fieldSize = typeof(UIElement).GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			fieldSize.SetValue(panel, new Size(10, 2));
			DateTimePeriodPanel.SetClipPeriod(panel, false);
			DateTimePeriodPanel.SetDateTimePeriod(panel, _period);
			return panel;
		}

		[Test]
		public void VerifyEndTimeChangedWithSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = GetPanel();
					_target.SchedulePart = scheduleDay;
					_target.EndTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = GetPanel();
					_target.SchedulePart = scheduleDay;
					_target.TimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyStartTimeChangedWithoutSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
					
					DateTimePeriodPanel panel = GetPanel();
					_target.StartTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyEndTimeChangedWithoutSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
				
					DateTimePeriodPanel panel = GetPanel();
					_target.EndTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithoutSchedulePart()
		{
			var testRunner = new CrossThreadTestRunner();
			testRunner.RunInSTA(
				delegate
				{
					
					DateTimePeriodPanel panel = GetPanel();
					_target.TimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyMoveUpDownReturnsFalseIfParentCollectionIsNull()
		{
			Assert.IsFalse(_target.CanMoveUp);
			Assert.IsFalse(_target.CanMoveDown);
		}

		[Test]
		public void VerifyCanExecuteDeleteCommand()
		{
			Assert.AreEqual(_testerForCommandModels.CanExecute(_target.DeleteCommand), _target.IsMovePermitted());
			Assert.AreEqual(_target.DeleteCommand.Text, UserTexts.Resources.Delete);
		}

		[Test]
		public void VerifyPeriodElapsedTime()
		{
			PropertyChangedListener listener = new PropertyChangedListener().ListenTo(_target);
			_target.Period = _target.Period.MovePeriod(TimeSpan.FromHours(2));
			Assert.IsTrue(listener.HasFired("ElapsedTime"));
			Assert.AreEqual(_target.Period.ElapsedTime(), _target.ElapsedTime);
		}
    }
}
