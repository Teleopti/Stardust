using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class ActivityTimeLimiterTest
    {
        private ActivityTimeLimiter _target;
        Activity _activity1;
        Activity _activity2;
        Activity _activity3;
        IVisualLayerCollection _visualLayerCollection;


        [SetUp]
        public void Setup()
        {
            _target = new ActivityTimeLimiter(new Activity("act"), TimeSpan.FromHours(2), OperatorLimiter.Equals);
            var period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc));

            var period2 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));

            var period3 = new DateTimePeriod(new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            var period4 = new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));

            var period5 = new DateTimePeriod(new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                             new DateTime(2000, 1, 1, 14, 0, 0, DateTimeKind.Utc));

            _activity1 = new Activity("act1");
            _activity2 = new Activity("act2");
            _activity3 = new Activity("act3");

	        _visualLayerCollection = new[]
		        {
			        new MainShiftLayer(_activity1, period1),
			        new MainShiftLayer(_activity2, period2),
			        new MainShiftLayer(_activity3, period3),
			        new MainShiftLayer(_activity3, period4),
			        new MainShiftLayer(_activity3, period5)
		        }.CreateProjection();
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void CanSetProperties()
        {
            //TimePeriod newTime = new TimePeriod(11, 12, 13, 15);
            TimeSpan newTime = new TimeSpan(2, 3, 0);
            _target.TimeLimit = newTime;
            _target.TimeLimitOperator = OperatorLimiter.LessThenEquals;
            _target.Activity = _activity1;
            Assert.AreEqual(newTime, _target.TimeLimit);
            Assert.AreEqual(OperatorLimiter.LessThenEquals, _target.TimeLimitOperator);
            Assert.AreEqual(_activity1, _target.Activity);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            ActivityTimeLimiter targetCloned = (ActivityTimeLimiter)_target.Clone();
            Assert.IsNotNull(targetCloned);
            Assert.AreEqual(_target.Id, targetCloned.Id);

            _target.TimeLimit = new TimeSpan(2, 3, 0);
            Assert.AreNotEqual(_target.TimeLimit, targetCloned.TimeLimit);

            targetCloned = (ActivityTimeLimiter)_target.NoneEntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.IsNull(targetCloned.Id);
        }

        [Test]
        public void VerifyIsValidEndEquals()
        {
            TimeSpan timeLimitOk1 = TimeSpan.FromHours(2);
            TimeSpan timeLimitOk2 = TimeSpan.FromHours(1);
            TimeSpan timeLimitNotOk1 = TimeSpan.FromHours(1);
            TimeSpan timeLimitNotOk2 = TimeSpan.FromHours(2);

            _target = new ActivityTimeLimiter(_activity1, timeLimitOk1, OperatorLimiter.Equals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitOk2, OperatorLimiter.Equals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity1, timeLimitNotOk1, OperatorLimiter.Equals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitNotOk2, OperatorLimiter.Equals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidEndlessThen()
        {
            TimeSpan timeLimitOk1 = new TimeSpan(2, 1, 0);
            TimeSpan timeLimitOk2 = new TimeSpan(4, 30, 0);
            TimeSpan timeLimitNotOk1 = TimeSpan.FromHours(1);
            TimeSpan timeLimitNotOk2 = TimeSpan.FromMinutes(10);

            _target = new ActivityTimeLimiter(_activity1, timeLimitOk1, OperatorLimiter.LessThen);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitOk2, OperatorLimiter.LessThen);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity1, timeLimitNotOk1, OperatorLimiter.LessThen);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitNotOk2, OperatorLimiter.LessThen);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidEndGreaterThen()
        {
            TimeSpan timeLimitOk1 = new TimeSpan(1, 59, 0);
            TimeSpan timeLimitOk2 = TimeSpan.FromMinutes(30);
            TimeSpan timeLimitNotOk1 = TimeSpan.FromHours(2);
            TimeSpan timeLimitNotOk2 = new TimeSpan(1, 1, 0);

            _target = new ActivityTimeLimiter(_activity1, timeLimitOk1, OperatorLimiter.GreaterThen);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitOk2, OperatorLimiter.GreaterThen);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity1, timeLimitNotOk1, OperatorLimiter.GreaterThen);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitNotOk2, OperatorLimiter.GreaterThen);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidEndlessThenEquals()
        {
            TimeSpan timeLimitOk1 = TimeSpan.FromHours(2);
            TimeSpan timeLimitOk2 = new TimeSpan(1, 30, 0);
            TimeSpan timeLimitNotOk1 = new TimeSpan(1, 59, 0);
            TimeSpan timeLimitNotOk2 = TimeSpan.FromMinutes(1);

            _target = new ActivityTimeLimiter(_activity1, timeLimitOk1, OperatorLimiter.LessThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitOk2, OperatorLimiter.LessThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity1, timeLimitNotOk1, OperatorLimiter.LessThenEquals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitNotOk2, OperatorLimiter.LessThenEquals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidEndGreaterThenEquals()
        {
            TimeSpan timeLimitOk1 = TimeSpan.FromHours(2);
            TimeSpan timeLimitOk2 = TimeSpan.FromMinutes(30);
            TimeSpan timeLimitNotOk1 = new TimeSpan(2, 59, 0);
            TimeSpan timeLimitNotOk2 = new TimeSpan(3, 1, 0);

            _target = new ActivityTimeLimiter(_activity1, timeLimitOk1, OperatorLimiter.GreaterThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitOk2, OperatorLimiter.GreaterThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity1, timeLimitNotOk1, OperatorLimiter.GreaterThenEquals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity2, timeLimitNotOk2, OperatorLimiter.GreaterThenEquals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void IsValidEndAtEndWhenActivityNotInlayers()
        {
            _target = new ActivityTimeLimiter(new Activity("actNotInVisualLayers"), TimeSpan.FromHours(2), OperatorLimiter.LessThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(new Activity("actNotInVisualLayers"), TimeSpan.FromHours(2), OperatorLimiter.GreaterThenEquals);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidEndWhenAdjacentActivities()
        {
            _target = new ActivityTimeLimiter(_activity3, TimeSpan.FromHours(3), OperatorLimiter.GreaterThenEquals);
            Assert.IsTrue(_target.IsValidAtEnd(_visualLayerCollection));

            _target = new ActivityTimeLimiter(_activity3, TimeSpan.FromHours(3), OperatorLimiter.LessThen);
            Assert.IsFalse(_target.IsValidAtEnd(_visualLayerCollection));
        }

        [Test]
        public void VerifyIsValidStart()
        {
            //AutoPosExtender
            //between 15 and 15 min with 15 min steps between min and max, here it is just an 15 min long activity
            TimePeriodWithSegment activityLength1 = new TimePeriodWithSegment(0, 15, 0, 15, 15);
            //activity can start 15, 30, 45 etc from shift start
            TimeSpan startInterval1 = new TimeSpan(0, 15, 0);
            //number of layers
            byte numLayers = 1;
            

            //AbsoluteStartExtender
            //between 1 and 2 hours, with 15 min steps between min and max, 01:00, 01:15, 01:30, 01:45, 02:00
            TimePeriodWithSegment activityLength2 = new TimePeriodWithSegment(1, 0, 2, 0, 15);
            TimePeriodWithSegment startIntervals = new TimePeriodWithSegment(10, 0, 17, 0, 15);


            //15 min
            IWorkShiftExtender autoPosExtender = new AutoPositionedActivityExtender(_activity1, activityLength1, startInterval1, numLayers);
            //2 hour
            IWorkShiftExtender absoluteStartExtender1 = new ActivityAbsoluteStartExtender(_activity1, activityLength2, startIntervals);
            //0 min, not the activity we have the limit on
            IWorkShiftExtender absoluteStartExtender2 = new ActivityAbsoluteStartExtender(_activity2, activityLength2, startIntervals);
            //2 hour 15 min
            IList<IWorkShiftExtender> extenders = new List<IWorkShiftExtender>() { autoPosExtender, absoluteStartExtender1, absoluteStartExtender2 };

            //TimePeriod timeLimit0 = new TimePeriod(2, 0, 4, 14);
            //TimePeriod timeLimit1 = new TimePeriod(2, 0, 4, 15);
            //TimePeriod timeLimit2 = new TimePeriod(2, 0, 4, 16);

            TimeSpan timeLimit0 = new TimeSpan(2, 14, 0);
            TimeSpan timeLimit1 = new TimeSpan(2, 15, 0);
            TimeSpan timeLimit2 = new TimeSpan(2, 16, 0);

            IWorkShift shift = WorkShiftFactory.Create(new TimeSpan(2, 0, 0), new TimeSpan(11, 0, 0));

            //we have limit > 2 hour 15 min and we have a sum of 2 hour and 15 min, we should have a false return
            _target = new ActivityTimeLimiter(_activity1, timeLimit1, OperatorLimiter.GreaterThen);
            Assert.IsFalse(_target.IsValidAtStart(shift, extenders));

            //we have a limit > 2 hour 14 min and we have a sum of 2 hour and 15 min, we should have a true return
            _target = new ActivityTimeLimiter(_activity1, timeLimit0, OperatorLimiter.GreaterThen);
            Assert.IsTrue(_target.IsValidAtStart(shift, extenders));

            //we have a limit >= 2 hour 16 min and we have a sum of 2 hour and 15 min, we should have false return
            _target = new ActivityTimeLimiter(_activity1, timeLimit2, OperatorLimiter.GreaterThenEquals);
            Assert.IsFalse(_target.IsValidAtStart(shift, extenders));

            //we have a limit >= 2 hour 15 min and we have a sum of 2 hour and 15 min, we should have a true return
            _target = new ActivityTimeLimiter(_activity1, timeLimit1, OperatorLimiter.GreaterThenEquals);
            Assert.IsTrue(_target.IsValidAtStart(shift, extenders));
        }
    }
}
