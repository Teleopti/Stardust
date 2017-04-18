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
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class AbsenceLayerViewModelTest
    {
        private SchedulePartFactoryForDomain _factory = new SchedulePartFactoryForDomain();
        private MockRepository _mocks = new MockRepository();
        private TesterForCommandModels _models = new TesterForCommandModels();
        private IEventAggregator _eventAggregator = new EventAggregator();
		private AbsenceLayerViewModel _target;
		private MockRepository mocks;
		private ILayer<IAbsence> _layerWithPayload;
		private IScheduleDay scheduleDay;
		private CrossThreadTestRunner testRunner;
		private PropertyChangedListener _listener;
		private IPerson person;
	    private TesterForCommandModels _testerForCommandModels;
	    private DateTimePeriod _period;
	    private bool _expectMovePermitted = true;
     
        [Test]
        public void VerifyCannotMoveAbsenceLayerVertical()
        {
            #region setup
			LayerViewModelCollection collection = new LayerViewModelCollection(_eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null);
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            collection.AddFromSchedulePart(part);
            #endregion

            //Get the first absencelayer:
            ILayerViewModel model = collection.First(l => l is AbsenceLayerViewModel);
            Assert.IsFalse(model.CanMoveUp);
            Assert.IsFalse(model.CanMoveDown);
        }

        [Test]
        public void VerifyDeleteCommandCallsObserver()
        {
            #region setup
            var observer = _mocks.StrictMock<ILayerViewModelObserver>();
         
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            var absenceLayer = part.PersonAbsenceCollection().First().Layer;
            var model = new AbsenceLayerViewModel(observer, absenceLayer, _eventAggregator);

            #endregion
            #region expectations
            using (_mocks.Record())
            {
                Expect.Call(() => observer.RemoveAbsence(model,absenceLayer,model.SchedulePart));            
               
            }
            #endregion

            using (_mocks.Playback())
            {
               
                _models.ExecuteCommandModel(model.DeleteCommand);
            }
        }

	
		[SetUp]
		public void Setup()
		{
			_listener = new PropertyChangedListener();
			_testerForCommandModels = new TesterForCommandModels();
			mocks = new MockRepository();
			scheduleDay = Mocks.StrictMock<IScheduleDay>();
			person = PersonFactory.CreatePerson();
			_period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
			_layerWithPayload = new AbsenceLayer(new Absence(), _period);
			Expect.Call(scheduleDay.Person).Return(person).Repeat.Any();
			Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TimeZoneHelper.CurrentSessionTimeZone)).Repeat.Any();

			Mocks.ReplayAll();

			_target = new AbsenceLayerViewModel(MockRepository.GenerateMock<ILayerViewModelObserver>(),_layerWithPayload,null);

			testRunner = new CrossThreadTestRunner();
		}

	
		private bool Opaque
		{
			get { return false; }
		}

		private MockRepository Mocks
		{
			get { return mocks; }
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

			var payloadFromLayer = _layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(person), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(person).Name, _target.Description);
			Assert.AreEqual(_layerWithPayload.Period, _target.Period);
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

			_target = new AbsenceLayerViewModel(layerObserver, _layerWithPayload, new EventAggregator());
			
			_target.IsChanged = true;
			_target.UpdatePeriod();

            layerObserver.AssertWasCalled(l => l.UpdateAllMovedLayers());
		}

		[Test]
		public void VerifyStartTimeChangedWithSchedulePart()
		{
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
			fieldSize.SetValue(panel, new System.Windows.Size(10, 2));
			DateTimePeriodPanel.SetClipPeriod(panel, false);
			DateTimePeriodPanel.SetDateTimePeriod(panel, _period);
			return panel;
		}

		[Test]
		public void VerifyEndTimeChangedWithSchedulePart()
		{
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

		[TearDown]
		public void Teardown()
		{
			Mocks.VerifyAll();
		}

    }
}
