using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for StatisticTask
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-02-11
    /// </remarks>
    [TestFixture]
    public class StatisticTaskTest
    {
        private double _statCalculatedTasks;
        private double _statAbandonedTasks;
        private double _statAnsweredTasks;
        private double _statAbandonedShortTasks;//
        private double _statAbandonedTasksWithinSL;
        private double _statAnsweredTasksWithinSL;
        private double _statOverflowOutTasks;
        private double _statOverflowInTasks;
        private double _statOfferedTasks;
        private TimeSpan _statAverageTaskTime;
        private TimeSpan _statAverageAfterTaskTime;
        private int _statAverageTaskTimeSeconds;
        private int _statAverageAfterTaskTimeSeconds;
        private int _statAverageQueueTimeSeconds;
        private int _statAverageHandleTimeSeconds;
        private int _statAverageTimeToAbandonSeconds;
        private int _statAverageTimeLongestInQueueAnsweredSeconds;
        private int _statAverageTimeLongestInQueueAbandonedSeconds;
        private DateTime _interval;
        private StatisticTask _target;
        private StatisticTask _target2;

        [SetUp]
        public void Setup()
        {
            _target = new StatisticTask();
            _statCalculatedTasks = 25;
            _statAbandonedTasks = 5;
            _statAnsweredTasks = 20;
            _statAverageTaskTimeSeconds = 45;
            _statAverageAfterTaskTimeSeconds = 120;
            _statAbandonedShortTasks = 10;
            _statAbandonedTasksWithinSL = 1;
            _statAnsweredTasksWithinSL = 11;
            _statOverflowOutTasks = 5;
            _statOverflowInTasks = 4;
            _statOfferedTasks = 37;
            _statAverageQueueTimeSeconds = 12;
            _statAverageHandleTimeSeconds = 18;
            _statAverageTimeToAbandonSeconds = 27;
            _statAverageTimeLongestInQueueAnsweredSeconds = 30;
            _statAverageTimeLongestInQueueAbandonedSeconds = 120;
            _interval = new DateTime(2008, 1, 2, 10, 30, 0);

            _target2 = new StatisticTask();
            _statAverageTaskTime = new TimeSpan(0, 0, 45);
            _statAverageAfterTaskTime = new TimeSpan(0, 2, 0);
        }

        [Test]
        public void VerifyPublicConstructorWorks()
        {
            _target = null;
            _target = new StatisticTask();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyPropertiesCanBeSetAndRead()
        {
            _target.StatAbandonedTasks = _statAbandonedTasks;
            _target.StatAnsweredTasks = _statAnsweredTasks;
            _target.StatAverageAfterTaskTimeSeconds = _statAverageAfterTaskTimeSeconds;
            _target.StatAverageTaskTimeSeconds = _statAverageTaskTimeSeconds;
            _target.StatCalculatedTasks = _statCalculatedTasks;
            _target.StatOfferedTasks = _statOfferedTasks;

            _target.StatAbandonedShortTasks = _statAbandonedShortTasks;
            _target.StatAbandonedTasksWithinSL = _statAbandonedTasksWithinSL;
            _target.StatAnsweredTasksWithinSL = _statAnsweredTasksWithinSL;
            _target.StatOverflowOutTasks = _statOverflowOutTasks;
            _target.StatOverflowInTasks = _statOverflowInTasks;
            _target.StatAverageQueueTimeSeconds = _statAverageQueueTimeSeconds;
            _target.StatAverageHandleTimeSeconds = _statAverageHandleTimeSeconds;
            _target.StatAverageTimeToAbandonSeconds = _statAverageTimeToAbandonSeconds;
            _target.StatAverageTimeLongestInQueueAnsweredSeconds = _statAverageTimeLongestInQueueAnsweredSeconds;
            _target.StatAverageTimeLongestInQueueAbandonedSeconds = _statAverageTimeLongestInQueueAbandonedSeconds;

            _target.Interval = _interval;

            Assert.AreEqual(_statAbandonedTasks, _target.StatAbandonedTasks);
            Assert.AreEqual(_statAnsweredTasks, _target.StatAnsweredTasks);
            Assert.AreEqual(_statAverageAfterTaskTime, _target.StatAverageAfterTaskTime);
            Assert.AreEqual(_statAverageTaskTime, _target.StatAverageTaskTime);
            Assert.AreEqual(_statCalculatedTasks, _target.StatCalculatedTasks);
            Assert.AreEqual(_statOfferedTasks,_target.StatOfferedTasks);
            Assert.AreEqual(_interval, _target.Interval);

            Assert.AreEqual(_statAbandonedShortTasks, _target.StatAbandonedShortTasks);
            Assert.AreEqual(_statAbandonedTasksWithinSL, _target.StatAbandonedTasksWithinSL);
            Assert.AreEqual(_statAnsweredTasksWithinSL, _target.StatAnsweredTasksWithinSL);
            Assert.AreEqual(_statOverflowOutTasks, _target.StatOverflowOutTasks);
            Assert.AreEqual(_statOverflowInTasks, _target.StatOverflowInTasks);
            Assert.AreEqual(_statOverflowInTasks, _target.StatOverflowInTasks);
            Assert.AreEqual(_statAverageQueueTimeSeconds, _target.StatAverageQueueTimeSeconds);
            Assert.AreEqual(_statAverageHandleTimeSeconds, _target.StatAverageHandleTimeSeconds);
            Assert.AreEqual(_statAverageTimeToAbandonSeconds, _target.StatAverageTimeToAbandonSeconds);
            Assert.AreEqual(_statAverageTimeLongestInQueueAnsweredSeconds, _target.StatAverageTimeLongestInQueueAnsweredSeconds);
            Assert.AreEqual(_statAverageTimeLongestInQueueAbandonedSeconds, _target.StatAverageTimeLongestInQueueAbandonedSeconds);

            Assert.AreEqual(_statAverageQueueTimeSeconds, _target.StatAverageQueueTime.TotalSeconds);
            Assert.AreEqual(_statAverageHandleTimeSeconds, _target.StatAverageHandleTime.TotalSeconds);
            Assert.AreEqual(_statAverageTimeToAbandonSeconds, _target.StatAverageTimeToAbandon.TotalSeconds);
            Assert.AreEqual(_statAverageTimeLongestInQueueAnsweredSeconds, _target.StatAverageTimeLongestInQueueAnswered.TotalSeconds);
            Assert.AreEqual(_statAverageTimeLongestInQueueAbandonedSeconds, _target.StatAverageTimeLongestInQueueAbandoned.TotalSeconds);

            Assert.AreEqual(_statAverageAfterTaskTimeSeconds, _target.StatAverageAfterTaskTimeSeconds);
            Assert.AreEqual(_statAverageTaskTimeSeconds, _target.StatAverageTaskTimeSeconds);
        }
        [Test]
        public void VerifyEqualsWork()
        {
            _target.StatAbandonedTasks = _statAbandonedTasks;
            _target.StatAnsweredTasks = _statAnsweredTasks;
            _target.StatAverageAfterTaskTimeSeconds = _statAverageAfterTaskTimeSeconds;
            _target.StatAverageTaskTimeSeconds = _statAverageTaskTimeSeconds;
            _target.StatCalculatedTasks = _statCalculatedTasks;

            _target2.StatAbandonedTasks = _statAbandonedTasks;
            _target2.StatAnsweredTasks = _statAnsweredTasks;
            _target2.StatAverageAfterTaskTimeSeconds = _statAverageAfterTaskTimeSeconds;
            _target2.StatAverageTaskTimeSeconds = _statAverageTaskTimeSeconds;
            _target2.StatCalculatedTasks = _statCalculatedTasks;
            Assert.IsTrue(_target.Equals(_target2));
            Assert.IsTrue(_target.Equals((object)_target2));
        }
        [Test]
        public void VerifyEqualsReturnsFalseIfParameterIsNull()
        {
            object testObject = null;
            Assert.IsFalse(_target.Equals(testObject));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IDictionary<StatisticTask, int> dic = new Dictionary<StatisticTask, int>();
            dic[_target] = 5;

            Assert.AreEqual(5, dic[_target]);
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            Assert.IsTrue(_target == _target2);
            StatisticTask task = new StatisticTask();
            _target.StatAbandonedTasks = _statAbandonedTasks;
            _target.StatAnsweredTasks = _statAnsweredTasks;
            _target.StatAverageAfterTaskTimeSeconds = _statAverageAfterTaskTimeSeconds;
            _target.StatAverageTaskTimeSeconds = _statAverageTaskTimeSeconds;
            _target.StatCalculatedTasks = _statCalculatedTasks;
            Assert.IsTrue(_target != task);
        }
    }
}