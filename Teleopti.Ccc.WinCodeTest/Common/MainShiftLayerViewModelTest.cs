using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class MainShiftLayerViewModelTest 
    {
		private MainShiftLayerViewModel target;
		private MockRepository mocks;
		private ILayer layerWithPayload;
		private IPayload payload;
		private IScheduleDay scheduleDay;
		private CrossThreadTestRunner testRunner;
		private PropertyChangedListener _listener;
		private IPerson person;
	    private TesterForCommandModels _testerForCommandModels;
	    private DateTimePeriod _period;
		
		[SetUp]
		public void Setup()
		{
			_listener = new PropertyChangedListener();
			_testerForCommandModels = new TesterForCommandModels();
			mocks = new MockRepository();
			layerWithPayload = Mocks.StrictMock<ILayer<IPayload>>();
			payload = ActivityFactory.CreateActivity("dfsdf");
			scheduleDay = Mocks.StrictMock<IScheduleDay>();
			person = PersonFactory.CreatePerson();
			_period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
			Expect.Call(layerWithPayload.Payload).Return(payload).Repeat.Any();
			Expect.Call(layerWithPayload.Period).PropertyBehavior().Return(_period).IgnoreArguments().Repeat.Any();
			Expect.Call(scheduleDay.Person).Return(person).Repeat.Any();
			Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 12, 5), TimeZoneHelper.CurrentSessionTimeZone)).Repeat.Any();

			Mocks.ReplayAll();

			testRunner = new CrossThreadTestRunner();
		}

		private MainShiftLayerViewModel CreateTestInstance(ILayer layer)
		{
			return new MainShiftLayerViewModel(layer, null);
		}

		private bool ExpectMovePermitted
		{
			get { return true; }
		}

		private bool ExpectIsPayloadChangePermitted
		{
			get { return true; }
		}


		
		protected virtual bool Opaque
		{
			get { return false; }
		}

		protected MockRepository Mocks
		{
			get { return mocks; }
		}

		[Test]
		public void VerifyCorrectDescription()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Assert.AreEqual(UserTexts.Resources.Activity, target.LayerDescription);
		}


		[Test]
		public void VerifyProperties()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			target.SchedulePart = scheduleDay;

			var payloadFromLayer = (IPayload)layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(person, new DateOnly(2008, 12, 5)), target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(person, new DateOnly(2008, 12, 5)).Name, target.Description);
			Assert.AreEqual(layerWithPayload.Period, target.Period);
			Assert.AreEqual(TimeSpan.FromMinutes(15), target.Interval);
			Assert.IsNull(target.Parent);
			Assert.IsFalse(target.IsChanged);
			Assert.AreEqual(layerWithPayload, target.Layer);
			Assert.AreSame(scheduleDay, target.SchedulePart);
			Assert.IsFalse(target.CanMoveAll);
			Assert.AreEqual(Opaque, target.Opaque);
			Mocks.BackToRecordAll();

			ILayer testLayer = Mocks.StrictMock<ILayer>();

			Mocks.ReplayAll();

			target.SchedulePart = scheduleDay;
			target.Layer = testLayer;
			target.Interval = TimeSpan.FromHours(1);

			Assert.AreEqual(scheduleDay, target.SchedulePart);
			Assert.AreEqual(testLayer, target.Layer);
			Assert.AreEqual(TimeSpan.FromHours(1), target.Interval);
		}

		[Test]
		public void VerifyCanMoveAll()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			bool succeeded = false;
			target.PropertyChanged += (x, y) =>
			{
				succeeded = true;
				Assert.AreEqual("CanMoveAll", y.PropertyName);
			};
			target.CanMoveAll = true;
			Assert.IsTrue(succeeded);
		}

		[Test]
		public void VerifyIsSelected()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Assert.IsFalse(target.IsSelected);

			_listener.ListenTo(target);
			target.IsSelected = true;
			Assert.IsTrue(_listener.HasFired("IsSelected"));
			Assert.IsTrue(target.IsSelected);

		}

		[Test]
		public void VerifyCanSetIsChanged()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			bool succeeded = false;
			target.PropertyChanged += (x, y) =>
			{
				succeeded = true;
				Assert.AreEqual("IsChanged", y.PropertyName);
			};
			target.IsChanged = true;
			Assert.IsTrue(succeeded);
		}

		[Test]
		public void VerifyCanSetPeriod()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Mocks.BackToRecord(layerWithPayload);
			layerWithPayload.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			Mocks.ReplayAll();

			_listener.ListenTo(target);

			target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			Assert.IsTrue(_listener.HasFired("Period"));
		}

		[Test]
		public void VerifyUpdatePeriod()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Mocks.BackToRecord(layerWithPayload);
			layerWithPayload.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			LastCall.Repeat.Twice();
			Mocks.ReplayAll();

			target.IsChanged = true;
			target.Period = _period.ChangeStartTime(TimeSpan.FromMinutes(-5));
			target.UpdatePeriod();
			Assert.IsFalse(target.IsChanged);
		}

		[Test]
		public void VerifyStartTimeChangedWithSchedulePart()
		{
			testRunner.RunInSTA(
				delegate
				{
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.SchedulePart = scheduleDay;
					target.StartTimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
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
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.SchedulePart = scheduleDay;
					target.EndTimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithSchedulePart()
		{
			testRunner.RunInSTA(
				delegate
				{
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.SchedulePart = scheduleDay;
					target.TimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
				});
		}

		[Test]
		public void VerifyStartTimeChangedWithoutSchedulePart()
		{
			testRunner.RunInSTA(
				delegate
				{
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.StartTimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
				});
		}

		[Test]
		public void VerifyEndTimeChangedWithoutSchedulePart()
		{
			testRunner.RunInSTA(
				delegate
				{
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.EndTimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
				});
		}

		[Test]
		public void VerifyTimeChangedWithoutSchedulePart()
		{
			testRunner.RunInSTA(
				delegate
				{
					layerWithPayload.Period = _period;
					target = CreateTestInstance(layerWithPayload);
					DateTimePeriodPanel panel = GetPanel();
					target.TimeChanged(panel, 1);
					Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
				});
		}

		[Test]
		public void VerifyMoveUpDownReturnsFalseIfParentCollectionIsNull()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Assert.IsFalse(target.CanMoveUp);
			Assert.IsFalse(target.CanMoveDown);
		}

		[Test]
		public void VerifyCanExecuteDeleteCommand()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			Assert.AreEqual(_testerForCommandModels.CanExecute(target.DeleteCommand), target.IsMovePermitted());
			Assert.AreEqual(target.DeleteCommand.Text, UserTexts.Resources.Delete);
		}

		[Test]
		public void VerifyPeriodElapsedTime()
		{
			layerWithPayload.Period = _period;
			target = CreateTestInstance(layerWithPayload);
			PropertyChangedListener listener = new PropertyChangedListener().ListenTo(target);
			target.Period = target.Period.MovePeriod(TimeSpan.FromHours(2));
			Assert.IsTrue(listener.HasFired("ElapsedTime"));
			Assert.AreEqual(target.Period.ElapsedTime(), target.ElapsedTime);

		}

		[TearDown]
		public void Teardown()
		{
			Mocks.VerifyAll();
		}

        

    }
}
