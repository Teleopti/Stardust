using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class OvertimeLayerViewModelTest 
    {
	    private bool _expectMovePermitted;
		private OvertimeLayerViewModel _target;
		private MockRepository _mocks;
		private OvertimeShiftLayer _layerWithPayload;
		private IActivity _payload;
		private IScheduleDay _scheduleDay;
		private CrossThreadTestRunner _testRunner;
		private PropertyChangedListener _listener;
		private IPerson _person;
	    private TesterForCommandModels _testerForCommandModels;
	    private DateTimePeriod _period;

		[SetUp]
		public void Setup()
		{
			_expectMovePermitted = true;
			_listener = new PropertyChangedListener();
			_testerForCommandModels = new TesterForCommandModels();
			_mocks = new MockRepository();
			_payload = ActivityFactory.CreateActivity("dfsdf");
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson();
			_period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
			_layerWithPayload = new OvertimeShiftLayer(_payload, _period, new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime));
			Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone)).Repeat.Any();

			_mocks.ReplayAll();
			_target = new OvertimeLayerViewModel(MockRepository.GenerateMock<ILayerViewModelObserver>(), _layerWithPayload, new PersonAssignment(_person, new Scenario(), DateOnly.Today), null, new FullPermission());
			_testRunner = new CrossThreadTestRunner();
		}

		[Test]
		public void ShouldMoveUp()
		{
			var observer = MockRepository.GenerateMock<ILayerViewModelObserver>();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var lastLayer = ass.OvertimeActivities().Last();
			var expectedLastAfterMove = ass.OvertimeActivities().Last(l => l != lastLayer);
			var target = new OvertimeLayerViewModel(observer, lastLayer, ass, stubbedEventAggregator());
			target.MoveUp();

			ass.OvertimeActivities().Last().Should().Be.EqualTo(expectedLastAfterMove);
			observer.AssertWasCalled(x => x.LayerMovedVertically(target));
		}

		[Test]
		public void ShouldMoveDown()
		{
			var observer = MockRepository.GenerateMock<ILayerViewModelObserver>();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var firstLayer = ass.OvertimeActivities().First();
			var expectedFirstAfterMove = ass.OvertimeActivities().Skip(1).First();
			var target = new OvertimeLayerViewModel(observer, firstLayer, ass, stubbedEventAggregator());
			target.MoveDown();

			observer.AssertWasCalled(x => x.LayerMovedVertically(target));
			ass.OvertimeActivities().First().Should().Be.EqualTo(expectedFirstAfterMove);
		}

		private static IEventAggregator stubbedEventAggregator()
		{
			var eventAgg = MockRepository.GenerateMock<IEventAggregator>();
			eventAgg.Expect(x => x.GetEvent<GenericEvent<TriggerShiftEditorUpdate>>()).Return(MockRepository.GenerateMock<GenericEvent<TriggerShiftEditorUpdate>>());
			return eventAgg;
		}


		[Test]
		public void VerifyCorrectDescription()
		{
			Assert.AreEqual(_layerWithPayload.DefinitionSet.Name, _target.LayerDescription);
		}

		[Test]
		public void VerifyCanGetDescriptionFromMultiplicatorSet()
		{
			IActivity activity = _mocks.StrictMock<IActivity>();
			var period = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet = _mocks.StrictMock<IMultiplicatorDefinitionSet>();
			var overtimeLayer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);

			Expect.Call(multiplicatorDefinitionSet.Name).Return("Qualified overtime");

			_mocks.ReplayAll();

			var target = new OvertimeLayerViewModel(null, overtimeLayer, new PersonAssignment(_person, new Scenario(), DateOnly.Today), null);
			Assert.AreEqual("Qualified overtime", target.LayerDescription);
		}


		[Test]
		public void VerifyProperties()
		{
			_target.SchedulePart = _scheduleDay;

			var payloadFromLayer = (IPayload)_layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor_DONTUSE(_person), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription_DONTUSE(_person).Name, _target.Description);
			Assert.AreEqual(_layerWithPayload.Period, _target.Period);
			Assert.AreEqual(TimeSpan.FromMinutes(15), _target.Interval);
			Assert.IsFalse(_target.IsChanged);
			Assert.AreSame(_scheduleDay, _target.SchedulePart);
			Assert.IsFalse(_target.CanMoveAll);
			Assert.AreEqual(true, _target.Opaque);

			_target.SchedulePart = _scheduleDay;
			_target.Interval = TimeSpan.FromHours(1);

			Assert.AreEqual(_scheduleDay, _target.SchedulePart);
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

			_target = new OvertimeLayerViewModel(layerObserver, _layerWithPayload, new PersonAssignment(_person, new Scenario(), DateOnly.Today), null, new FullPermission());
			
			_target.IsChanged = true;
			_target.UpdatePeriod();

            layerObserver.AssertWasCalled(l => l.UpdateAllMovedLayers());
		}

		[Test]
		public void VerifyStartTimeChangedWithSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.SchedulePart = _scheduleDay;
					_target.StartTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		private DateTimePeriodPanel getPanel()
		{
			var panel = new DateTimePeriodPanel();
			FieldInfo fieldSize = typeof(UIElement).GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			fieldSize.SetValue(panel, new System.Windows.Size(10, 2));
			DateTimePeriodPanel.SetClipPeriod(panel, false);
			DateTimePeriodPanel.SetDateTimePeriod(panel, _period);
			return panel;
		}

		[Test]
		public void VerifyEndTimeChangedWithSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.SchedulePart = _scheduleDay;
					_target.EndTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.SchedulePart = _scheduleDay;
					_target.TimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyStartTimeChangedWithoutSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.StartTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyEndTimeChangedWithoutSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.EndTimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithoutSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = getPanel();
					_target.TimeChanged(panel, 1);
					Assert.AreEqual(_expectMovePermitted, _target.IsChanged);
				});
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
			_mocks.VerifyAll();
		}
    }
}
