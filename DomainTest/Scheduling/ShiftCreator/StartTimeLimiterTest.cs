using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class StartTimeLimiterTest
    {
        private StartTimeLimiter _startTimeLimiter;
        private TimePeriod _startTimeLimitation;
        private Guid _guid = Guid.NewGuid();
        private FakeShift _shift;
        DateTimePeriod _period1;
        DateTimePeriod _period2;
        DateTimePeriod _period3;
        DateTimePeriod _period4;
        DateTimePeriod _period5;
        Activity _activity1;
        Activity _activity2;
        Activity _activity3;
        IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _startTimeLimitation = new TimePeriod(7, 0, 8, 0);
            _startTimeLimiter = new StartTimeLimiter(_startTimeLimitation);
            ((IEntity)_startTimeLimiter).SetId(_guid);

            _shift = new FakeShift();
            typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_shift, Guid.NewGuid());
            _period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc));

            _period2 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));

            _period3 = new DateTimePeriod(new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            _period4 = new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));

            _period5 = new DateTimePeriod(new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                             new DateTime(2000, 1, 1, 14, 0, 0, DateTimeKind.Utc));

            _activity1 = new Activity("act1");
            _activity2 = new Activity("act2");
            _activity3 = new Activity("act3");

            _shift.LayerCollection.Add(new ActivityLayer(_activity1, _period1));
            _shift.LayerCollection.Add(new ActivityLayer(_activity2, _period2));
            _shift.LayerCollection.Add(new ActivityLayer(_activity3, _period3));
            _shift.LayerCollection.Add(new ActivityLayer(_activity3, _period4));
            _shift.LayerCollection.Add(new ActivityLayer(_activity3, _period5));

            IProjectionService svc = _shift.ProjectionService();
            _visualLayerCollection = svc.CreateProjection();
        }
        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_startTimeLimiter);
        }

        [Test]
        public void VerifyClone()
        {
            StartTimeLimiter clonedLimiter = (StartTimeLimiter)_startTimeLimiter.Clone();
            clonedLimiter.StartTimeLimitation = new TimePeriod(10, 0, 11, 0);

            Assert.AreNotEqual(clonedLimiter.StartTimeLimitation, _startTimeLimiter.StartTimeLimitation);
            Assert.AreEqual(_startTimeLimiter.Id, clonedLimiter.Id);

            StartTimeLimiter nonEntityClone = (StartTimeLimiter) _startTimeLimiter.NoneEntityClone();

            Assert.AreNotEqual(_startTimeLimiter.Id, nonEntityClone.Id);
        }
        [Test]
        public void VerifyIsValidAtStart()
        {
            IWorkShift workValid = WorkShiftFactory.Create(new TimeSpan(0, 7, 0, 0), new TimeSpan(0, 17, 0, 0));
            IWorkShift workShiftNotValid = WorkShiftFactory.Create(new TimeSpan(0, 10, 0, 0), new TimeSpan(0, 17, 0, 0));
            Assert.IsTrue(_startTimeLimiter.IsValidAtStart(workValid, null));
            Assert.IsFalse(_startTimeLimiter.IsValidAtStart(workShiftNotValid, null));

            //Check what kind of time shoud be used UTC or local
            //workValid = WorkShiftFactory.Create(new TimeSpan(0, 8, 0, 0), new TimeSpan(0, 17, 0, 0));
            //Assert.IsTrue(_startTimeLimiter.IsValidAtStart(workValid, null));
        }
        [Test]
        public void VerifyIsValidAtEnd()
        {
            //If there are remaining shift they should be valid
            Assert.IsTrue(_startTimeLimiter.IsValidAtEnd(_visualLayerCollection));
            _shift = new FakeShift();
            IProjectionService svc = _shift.ProjectionService();
            _visualLayerCollection = svc.CreateProjection();
            //If no shifts, return false
            Assert.IsFalse(_startTimeLimiter.IsValidAtEnd(_visualLayerCollection));
        }

    }
}
