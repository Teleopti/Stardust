using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PersonalShiftLayerViewModelTest
    {
	    private bool _expectMovePermitted;
		private LayerViewModel _target;
		private MockRepository _mocks;
		private PersonalShiftLayer _layerWithPayload;
		private IActivity _payload;
		private IScheduleDay _scheduleDay;
		private CrossThreadTestRunner _testRunner;
		private PropertyChangedListener _listener;
		private IPerson person;
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
			person = PersonFactory.CreatePerson();
			_period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
			_layerWithPayload = new PersonalShiftLayer(_payload, _period);
			Expect.Call(_scheduleDay.Person).Return(person).Repeat.Any();
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone)).Repeat.Any();

			_mocks.ReplayAll();

			_target = new PersonalShiftLayerViewModel(MockRepository.GenerateMock<ILayerViewModelObserver>(), _layerWithPayload, new PersonAssignment(person, new Scenario(), DateOnly.Today), null, new FullPermission());

			_testRunner = new CrossThreadTestRunner();
		}
		
		[Test]
		public void VerifyCorrectDescription()
		{
			Assert.AreEqual(UserTexts.Resources.PersonalShifts, _target.LayerDescription);
		}
		
		[Test]
		public void VerifyProperties()
		{
			_target.SchedulePart = _scheduleDay;

			var payloadFromLayer = (IPayload)_layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(person), _target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(person).Name, _target.Description);
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
			_target.IsChanged.Should().Be.EqualTo(false);
		}

		[Test]
		public void VerifyStartTimeChangedWithSchedulePart()
		{
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = GetPanel();
					_target.SchedulePart = _scheduleDay;
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
			_testRunner.RunInSTA(
				delegate
				{
					DateTimePeriodPanel panel = GetPanel();
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
					DateTimePeriodPanel panel = GetPanel();
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
					DateTimePeriodPanel panel = GetPanel();
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
					DateTimePeriodPanel panel = GetPanel();
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
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldMoveUp()
		{
			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 12, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("activity");

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
			personAssignment.AddPersonalActivity(activity, period);
			personAssignment.AddPersonalActivity(activity, period2);


			_target = new PersonalShiftLayerViewModel(null, personAssignment.PersonalActivities().Last(), personAssignment, null);
			_target.MoveUp();

			personAssignment.PersonalActivities().First().Period.Should().Be.EqualTo(period2);
		}
    }
}
