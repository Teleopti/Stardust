using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    public abstract class LayerViewModelTest
    {
        private LayerViewModel target;
        private MockRepository mocks;
        private ILayer layerWithPayload;
        private IPayload payload;
        private IScheduleDay scheduleDay;
        private CrossThreadTestRunner testRunner;
        private PropertyChangedListener _listener;
    	private IPerson person;
    	protected TesterForCommandModels TesterForCommandModels { get; private set; }
        protected DateTimePeriod Period { get; private set; }
        [SetUp]
        public void Setup()
        {
            _listener = new PropertyChangedListener();
            TesterForCommandModels = new TesterForCommandModels();
            mocks = new MockRepository();
            layerWithPayload = Mocks.StrictMock<ILayer<IPayload>>();
            payload = ActivityFactory.CreateActivity("dfsdf");
            scheduleDay = Mocks.StrictMock<IScheduleDay>();
        	person = PersonFactory.CreatePerson();
            Period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
            Expect.Call(layerWithPayload.Payload).Return(payload).Repeat.Any();
            Expect.Call(layerWithPayload.Period).PropertyBehavior().Return(Period).IgnoreArguments().Repeat.Any();
            Expect.Call(scheduleDay.Person).Return(person).Repeat.Any();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008,12,5), TimeZoneHelper.CurrentSessionTimeZone)).Repeat.Any();

            Mocks.ReplayAll();

            testRunner = new CrossThreadTestRunner();
        }

        protected abstract string LayerModelDescription { get; }
        protected abstract LayerViewModel CreateTestInstance(ILayer layer);
        protected abstract bool ExpectMovePermitted { get; }
        protected abstract bool ExpectIsPayloadChangePermitted { get; }
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
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            Assert.AreEqual(LayerModelDescription,target.LayerDescription);
        }


        [Test]
        public void VerifyProperties()
        {
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            target.SchedulePart = scheduleDay;

        	var payloadFromLayer = (IPayload) layerWithPayload.Payload;

			Assert.AreEqual(payloadFromLayer.ConfidentialDisplayColor(person, new DateOnly(2008, 12, 5)), target.DisplayColor);
			Assert.AreEqual(payloadFromLayer.ConfidentialDescription(person, new DateOnly(2008, 12, 5)).Name, target.Description);
            Assert.AreEqual(layerWithPayload.Period, target.Period);
            Assert.AreEqual(TimeSpan.FromMinutes(15),target.Interval);
            Assert.IsNull(target.Parent);
            Assert.IsFalse(target.IsChanged);
            Assert.AreEqual(layerWithPayload, target.Layer);
            Assert.AreSame(scheduleDay, target.SchedulePart);
            Assert.IsFalse(target.CanMoveAll);
            Assert.AreEqual(Opaque,target.Opaque);
            Mocks.BackToRecordAll();

            ILayer testLayer = Mocks.StrictMock<ILayer>();

            Mocks.ReplayAll();

            target.SchedulePart = scheduleDay;
            target.Layer = testLayer;
            target.Interval = TimeSpan.FromHours(1);

            Assert.AreEqual(scheduleDay,target.SchedulePart);
            Assert.AreEqual(testLayer,target.Layer);
            Assert.AreEqual(TimeSpan.FromHours(1),target.Interval);
        }

        [Test]
        public void VerifyCanMoveAll()
        {
            layerWithPayload.Period = Period;
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
            layerWithPayload.Period = Period;
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
            layerWithPayload.Period = Period;
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
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            Mocks.BackToRecord(layerWithPayload);
            layerWithPayload.Period = Period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            Mocks.ReplayAll();

            _listener.ListenTo(target);
           
            target.Period = Period.ChangeStartTime(TimeSpan.FromMinutes(-5));           
            Assert.IsTrue(_listener.HasFired("Period"));
        }

        [Test]
        public void VerifyUpdatePeriod()
        {
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            Mocks.BackToRecord(layerWithPayload);
            layerWithPayload.Period = Period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            LastCall.Repeat.Twice();
            Mocks.ReplayAll();

            target.IsChanged = true;
            target.Period = Period.ChangeStartTime(TimeSpan.FromMinutes(-5));
            target.UpdatePeriod();
            Assert.IsFalse(target.IsChanged);
        }

        [Test]
        public void VerifyStartTimeChangedWithSchedulePart()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        layerWithPayload.Period = Period;
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
            DateTimePeriodPanel.SetDateTimePeriod(panel, Period);
            return panel;
        }

        [Test]
        public void VerifyEndTimeChangedWithSchedulePart()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        layerWithPayload.Period = Period;
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
                        layerWithPayload.Period = Period;
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
                        layerWithPayload.Period = Period;
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
                        layerWithPayload.Period = Period;
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
                        layerWithPayload.Period = Period;
                        target = CreateTestInstance(layerWithPayload);
                        DateTimePeriodPanel panel = GetPanel();
                        target.TimeChanged(panel, 1);
                        Assert.AreEqual(ExpectMovePermitted, target.IsChanged);
                    });
        }

        [Test]
        public void VerifyMoveUpDownReturnsFalseIfParentCollectionIsNull()
       {
           layerWithPayload.Period = Period;
           target = CreateTestInstance(layerWithPayload);
           Assert.IsFalse(target.CanMoveUp);
           Assert.IsFalse(target.CanMoveDown);
       }

        [Test]
        public void VerifyCanExecuteDeleteCommand()
        {
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            Assert.AreEqual(TesterForCommandModels.CanExecute(target.DeleteCommand), target.IsMovePermitted());
            Assert.AreEqual(target.DeleteCommand.Text,UserTexts.Resources.Delete);
        }

        [Test]
        public void VerifyPeriodElapsedTime()
        {
            layerWithPayload.Period = Period;
            target = CreateTestInstance(layerWithPayload);
            PropertyChangedListener listener = new PropertyChangedListener().ListenTo(target);
            target.Period = target.Period.MovePeriod(TimeSpan.FromHours(2));
            Assert.IsTrue(listener.HasFired("ElapsedTime"));
            Assert.AreEqual(target.Period.ElapsedTime(),target.ElapsedTime);

        }

        [TearDown]
        public void Teardown()
        {
            Mocks.VerifyAll();
        }
    }

}
