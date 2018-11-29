using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TotalDayItemTest
    {
        private TotalDayItem _target;
        private ITaskOwner _taskOwner;
        private double _taskIndex;
        private double _talkTimeIndex;
        private double _afterTalkTimeIndex;
        private DateOnly _date;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _taskIndex = 1.2;
            _talkTimeIndex = 1.8;
            _afterTalkTimeIndex = 0.8;

            _date = new DateOnly(2008, 3, 31);
            _taskOwner = mocks.StrictMock<ITaskOwner>();

            Expect.Call(_taskOwner.OpenForWork).Return(new OpenForWork(true,true)).Repeat.Any();
            Expect.Call(_taskOwner.Tasks).PropertyBehavior().Return(10).Repeat.Any();
            Expect.Call(_taskOwner.AverageTaskTime).PropertyBehavior().Return(TimeSpan.Zero).Repeat.Any();
            Expect.Call(_taskOwner.AverageAfterTaskTime).PropertyBehavior().Return(TimeSpan.Zero).Repeat.Any();
            Expect.Call(_taskOwner.CurrentDate).Return(_date).Repeat.Any();

            mocks.ReplayAll();

            _taskOwner.Tasks = 10;
            _taskOwner.AverageTaskTime = TimeSpan.FromSeconds(10);
            _taskOwner.AverageAfterTaskTime= TimeSpan.FromSeconds(20);

            _target = new TotalDayItem();
            _target.SetComparisonValues(_taskOwner, _taskIndex, _talkTimeIndex, _afterTalkTimeIndex, 1);
        }

        [Test]
        public void VerifyCanSetAndGetIndexProperties()
        {
            _target.TaskIndex = 1.23;
            _target.TalkTimeIndex = 0.88;
            _target.AfterTalkTimeIndex = 1.12;

            Assert.AreEqual(1.23, _target.TaskIndex);
            Assert.AreEqual(0.88, _target.TalkTimeIndex);
            Assert.AreEqual(1.12, _target.AfterTalkTimeIndex);
            Assert.IsFalse(_target.WorkloadDayIsClosed);
        }

        [Test]
        public void VerifyCanSetAndGetProperties()
        {
            _target.Tasks = 586.55d;
            _target.TalkTime = new TimeSpan(0, 0, 3, 2);
            _target.AfterTalkTime = new TimeSpan(0, 0, 1, 2);

            Assert.AreEqual(586.55d, Math.Round(_target.Tasks,4));
            Assert.AreEqual(new TimeSpan(0, 0, 3, 2).TotalSeconds, Math.Round(_target.TalkTime.TotalSeconds,4));
            Assert.AreEqual(new TimeSpan(0, 0, 1, 2).TotalSeconds, Math.Round(_target.AfterTalkTime.TotalSeconds,4));
        }

        [Test]
        public void VerifyCanCalculateNewTasks()
        {
            _target.TaskIndex = 1;
            double tasks = _target.Tasks;

            _target.TaskIndex = 2;

            Assert.AreEqual(tasks*2,_target.Tasks);

            _target.TaskIndex = 1;

            Assert.AreEqual(tasks,_target.Tasks);

        }

        [Test]
        public void VerifyCanCalculateNewTaskIndex()
        {

            double tasks = _target.Tasks;
            double index = _target.TaskIndex;
            _target.Tasks = tasks*1.5;

            Assert.AreEqual(index*1.5,_target.TaskIndex);

            _target.Tasks = tasks;

            Assert.AreEqual(index,_target.TaskIndex);

            _target.TaskIndex = 1;
            double indexOneTasks = _target.Tasks;
            _target.TaskIndex = 2399;

            _target.Tasks = indexOneTasks;

            Assert.AreEqual(1,_target.TaskIndex);
        }

        [Test]
        public void VerifyCanCalculateNewTalkTime()
        {
            _target.TalkTimeIndex = 1;
            double talkTime = _target.TalkTime.TotalSeconds;

            _target.TalkTimeIndex = 2;

            Assert.AreEqual(Math.Round((talkTime * 2),4), Math.Round(_target.TalkTime.TotalSeconds,4));

            _target.TalkTimeIndex = 1;

            Assert.AreEqual(Math.Round(talkTime,4), Math.Round(_target.TalkTime.TotalSeconds,4));
        }

        [Test]
        public void VerifyCanCalculateNewTalkTimeIndex()
        {
            TimeSpan talkTime = _target.TalkTime;

            double index = _target.TalkTimeIndex;
            _target.TalkTime = new TimeSpan((long)(talkTime.Ticks * 1.5));

            Assert.AreEqual(Math.Round((index * 1.5),4), Math.Round(_target.TalkTimeIndex,4));

            _target.TalkTime = talkTime;

            Assert.AreEqual(Math.Round(index,4), Math.Round(_target.TalkTimeIndex,4));

            _target.TalkTimeIndex = 1;
            TimeSpan indexOneTalkTime = _target.TalkTime;
            _target.TalkTimeIndex = 2399;

            _target.TalkTime = indexOneTalkTime;

            Assert.AreEqual(1, Math.Round(_target.TalkTimeIndex,4));
        }

        [Test]
        public void VerifyCanCalculateNewAfterTalkTimeIndex()
        {
            TimeSpan afterTalkTime = _target.AfterTalkTime;

            double index = _target.AfterTalkTimeIndex;
            _target.AfterTalkTime = new TimeSpan((long)(afterTalkTime.Ticks * 1.5));

            Assert.AreEqual(Math.Round((index * 1.5), 4), Math.Round(_target.AfterTalkTimeIndex, 4));

            _target.AfterTalkTime = afterTalkTime;

            Assert.AreEqual(Math.Round(index, 4), Math.Round(_target.AfterTalkTimeIndex, 4));

            _target.AfterTalkTimeIndex = 1;
            TimeSpan indexOneAfterTalkTime = _target.AfterTalkTime;
            _target.AfterTalkTimeIndex = 2399;

            _target.AfterTalkTime = indexOneAfterTalkTime;

            Assert.AreEqual(1, Math.Round(_target.AfterTalkTimeIndex, 4));
        }

        /// <summary>
        /// Verifies the current date.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        [Test]
        public void VerifyCurrentDate()
        {
            Assert.AreEqual(_date, _target.CurrentDate);
        }

		[Test]
		public void ShouldNotAllowNegativeValuesInAfterTalkTimeIndex()
		{
			_target.AfterTalkTimeIndex = 1;
			Assert.AreEqual(1, _target.AfterTalkTimeIndex);

			_target.AfterTalkTimeIndex = -2;
			Assert.AreEqual(1, _target.AfterTalkTimeIndex);
		}

		[Test]
		public void ShouldNotAllowNegativeValuesInTalkTimeIndex()
		{
			_target.TalkTimeIndex = 1;
			Assert.AreEqual(1, _target.TalkTimeIndex);

			_target.TalkTimeIndex = -2;
			Assert.AreEqual(1, _target.TalkTimeIndex);
		}

		[Test]
		public void ShouldNotAllowNegativeValuesInTaskIndex()
		{
			_target.TaskIndex = 1;
			Assert.AreEqual(1, _target.TaskIndex);

			_target.TaskIndex = -2;
			Assert.AreEqual(1, _target.TaskIndex);
		}
    }
}
