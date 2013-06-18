using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class EndTimeLimiterTest
    {
        private EndTimeLimiter _endTimeLimiter;
        private TimePeriod _endTimeLimitation;

        IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _endTimeLimitation = new TimePeriod(17, 0, 18, 0);
            _endTimeLimiter = new EndTimeLimiter(_endTimeLimitation);
					_endTimeLimiter.SetId(Guid.NewGuid());

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

            var activity1 = new Activity("act1");
            var activity2 = new Activity("act2");
            var activity3 = new Activity("act3");

						_visualLayerCollection = new[]
		        {
			        new MainShiftLayer(activity1, period1),
			        new MainShiftLayer(activity2, period2),
			        new MainShiftLayer(activity3, period3),
			        new MainShiftLayer(activity3, period4),
			        new MainShiftLayer(activity3, period5)
		        }.CreateProjection();
        }
        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_endTimeLimiter);
        }

        [Test]
        public void VerifyClone()
        {
            EndTimeLimiter clonedLimiter = (EndTimeLimiter)_endTimeLimiter.Clone();
            clonedLimiter.EndTimeLimitation = new TimePeriod(10, 0, 11, 0);

            Assert.AreNotEqual(clonedLimiter.EndTimeLimitation, _endTimeLimiter.EndTimeLimitation);
            Assert.AreEqual(_endTimeLimiter.Id, clonedLimiter.Id);
            EndTimeLimiter nonEntityClone = (EndTimeLimiter) _endTimeLimiter.NoneEntityClone();

            Assert.AreNotEqual(_endTimeLimiter.Id, nonEntityClone.Id);
        }
        [Test]
        public void VerifyIsValidAtStart()
        {
            IWorkShift workValid = WorkShiftFactory.Create(new TimeSpan(0, 7, 0, 0), new TimeSpan(0, 17, 0, 0));
            IWorkShift workShiftNotValid = WorkShiftFactory.Create(new TimeSpan(0, 10, 0, 0), new TimeSpan(0, 20, 0, 0));
            Assert.IsTrue(_endTimeLimiter.IsValidAtStart(workValid, null));
            Assert.IsFalse(_endTimeLimiter.IsValidAtStart(workShiftNotValid, null));

            //Check what kind of time shoud be used UTC or local
            //workValid = WorkShiftFactory.Create(new TimeSpan(0, 18, 0, 0), new TimeSpan(0, 19, 0, 0));
            //Assert.IsTrue(_endTimeLimiter.IsValidAtStart(workValid, null));
        }
        [Test]
        public void VerifyIsValidAtEnd()
        {
            //If there are remaining shift they should be valid
            Assert.IsTrue(_endTimeLimiter.IsValidAtEnd(_visualLayerCollection));
            //If no shifts, return false
            Assert.IsFalse(_endTimeLimiter.IsValidAtEnd(new IMainShiftLayer[0].CreateProjection()));
        }

    }
}
