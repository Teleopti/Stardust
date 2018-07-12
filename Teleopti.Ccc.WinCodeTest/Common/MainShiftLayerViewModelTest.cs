﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class MainShiftLayerViewModelTest 
	{

	    private bool _expectMovePermitted;
		private MainShiftLayerViewModel _target;
		private MockRepository _mocks;
		private MainShiftLayer _layerWithPayload;
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
			_layerWithPayload = new MainShiftLayer(_payload, _period);
			Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TimeZoneHelper.CurrentSessionTimeZone)).Repeat.Any();

			_mocks.ReplayAll();

			_target = new MainShiftLayerViewModel(MockRepository.GenerateMock<ILayerViewModelObserver>(), _layerWithPayload, new PersonAssignment(_person, new Scenario(), DateOnly.Today), null);
			_testRunner = new CrossThreadTestRunner();
		}

		[Test]
		public void ShouldNotCrashOnDescription()
		{
			var visualLayer = new VisualLayer(_payload, _period, _payload);
			var mainShiftLayerViewModel = new MainShiftLayerViewModel(visualLayer, _person);

			mainShiftLayerViewModel.Description.Should().Be.EqualTo(_payload.Description.Name);
		}

		[Test]
		public void VerifyDeleteCommandForMainShiftCallsObserver()
		{
			#region setup
			var observer = _mocks.StrictMock<ILayerViewModelObserver>();
		    var commandTester = new TesterForCommandModels();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var firstLayer = ass.MainActivities().First();
			var model = new MainShiftLayerViewModel(observer, firstLayer, ass, new EventAggregator());


			#endregion
			#region expectations
			using (_mocks.Record())
			{
				Expect.Call(() => observer.RemoveActivity(model,firstLayer,model.SchedulePart));
			}
			#endregion

			using (_mocks.Playback())
			{
				//Execute Delete
				commandTester.ExecuteCommandModel(model.DeleteCommand);
			}
		}

		[Test]
		public void VerifyDeleteCommandForPersonalShiftCallsObserver()
		{
			var observer = _mocks.StrictMock<ILayerViewModelObserver>();
			var commandTester = new TesterForCommandModels();
			var period = new DateTimePeriod(2017, 1, 1, 8, 2017, 1, 1, 9);
			var ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(new Person(), period);
			var firstLayer = ass.PersonalActivities().Single();
			var model = new PersonalShiftLayerViewModel(observer, firstLayer, ass, new EventAggregator());

			using (_mocks.Record())
			{
				Expect.Call(() => observer.RemoveActivity(model, firstLayer, model.SchedulePart));
			}

			using (_mocks.Playback())
			{
				//Execute Delete
				commandTester.ExecuteCommandModel(model.DeleteCommand);
			}
		}

		[Test]
		public void VerifyCorrectDescription()
		{
			Assert.AreEqual(UserTexts.Resources.Activity, _target.LayerDescription);
		}

			[Test]
			public void VerifyCanMoveUp()
			{
				var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000,1,1));
				var period = new DateTimePeriod(2000, 09, 12, 8, 2000, 09, 12, 9);
				ass.AddOvertimeActivity(new Activity("d"), period, null);
				ass.AddActivity(new Activity("d"), period);
				ass.AddActivity(new Activity("d"), period);

				var first = new MainShiftLayerViewModel(null, ass.MainActivities().First(), ass, null);
				var last = new MainShiftLayerViewModel(null, ass.MainActivities().Last(), ass, null);

				first.CanMoveUp.Should().Be.False();
				last.CanMoveUp.Should().Be.True();
			}

			[Test]
			public void VerifyCanMoveDown()
			{
				var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
				var period = new DateTimePeriod(2000, 09, 12, 8, 2000, 09, 12, 9);
				ass.AddOvertimeActivity(new Activity("d"), period, null);
				ass.AddActivity(new Activity("d"), period);
				ass.AddActivity(new Activity("d"), period);

				var first = new MainShiftLayerViewModel(null, ass.MainActivities().First(), ass, null);
				var last = new MainShiftLayerViewModel(null, ass.MainActivities().Last(), ass, null);

				first.CanMoveDown.Should().Be.True();
				last.CanMoveDown.Should().Be.False();
			}

			[Test]
			public void ShouldMoveUp()
			{
				var observer = MockRepository.GenerateStub<ILayerViewModelObserver>();
				var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
				var lastLayer = ass.MainActivities().Last();
				var expectedLastAfterMove = ass.MainActivities().Last(l => l != lastLayer);
				var target = new MainShiftLayerViewModel(observer, lastLayer, ass, stubbedEventAggregator());
				target.MoveUp();

				ass.MainActivities().Last().Should().Be.EqualTo(expectedLastAfterMove);

				observer.AssertWasCalled(x => x.LayerMovedVertically(target));
			}

			[Test]
			public void ShouldMoveDown()
			{
				var observer = MockRepository.GenerateMock<ILayerViewModelObserver>();
				var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
				var firstLayer = ass.MainActivities().First();
				var expectedFirstAfterMove = ass.MainActivities().Skip(1).First();
				var target = new MainShiftLayerViewModel(observer, firstLayer, ass, stubbedEventAggregator());
				target.MoveDown();

				ass.MainActivities().First().Should().Be.EqualTo(expectedFirstAfterMove);
				observer.AssertWasCalled(x => x.LayerMovedVertically(target));
			}

			private static IEventAggregator stubbedEventAggregator()
			{
				var eventAgg = MockRepository.GenerateMock<IEventAggregator>();
				eventAgg.Expect(x => x.GetEvent<GenericEvent<TriggerShiftEditorUpdate>>()).Return(MockRepository.GenerateMock<GenericEvent<TriggerShiftEditorUpdate>>());
				return eventAgg;
			}

		[Test]
		public void VerifyProperties()
		{
			_target.SchedulePart = _scheduleDay;

			var payloadFromLayer = (IPayload)_layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(_person), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(_person).Name, _target.Description);
			Assert.AreEqual(_layerWithPayload.Period, _target.Period);
			Assert.AreEqual(TimeSpan.FromMinutes(15), _target.Interval);
			Assert.IsFalse(_target.IsChanged);
			Assert.AreSame(_scheduleDay, _target.SchedulePart);
			Assert.IsFalse(_target.CanMoveAll);
			Assert.AreEqual(false, _target.Opaque);

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
		public void VerifyUpdatePeriod()
		{
			_target.IsChanged = true;
			_target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			_target.UpdatePeriod();
			Assert.IsFalse(_target.IsChanged);
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

			_target = new MainShiftLayerViewModel(layerObserver, _layerWithPayload, new PersonAssignment(_person, new Scenario(), DateOnly.Today), null);
			
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
			fieldSize.SetValue(panel, new Size(10, 2));
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

		[Test]
		public void MovePeriod_WhenIntervalDoesNotmatch_ShouldOnlySnapStartTime()
		{
			_testRunner.RunInSTA(
				delegate
				{

					DateTimePeriodPanel panel = getPanel();

					_target.Interval = TimeSpan.FromMinutes(1);
					_target.EndTimeChanged(panel, -0.03);

					var endTimeMinutes = _target.Period.EndDateTime.Minute;

					_target.Interval = TimeSpan.FromMinutes(60);
					_target.TimeChanged(panel, 10);
					Assert.AreEqual(_target.Period.EndDateTime.Minute, endTimeMinutes, "Since we moved the layer one hour, the minutes should be intact, ie not snapped to 30");

				});
		}

		[TearDown]
		public void Teardown()
		{
			_mocks.VerifyAll();
		}



    }
}
