using System;
using System.Linq;
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
    public class StartTimeLimiterTest
    {
        private StartTimeLimiter _startTimeLimiter;
        private TimePeriod _startTimeLimitation;
        IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _startTimeLimitation = new TimePeriod(7, 0, 8, 0);
            _startTimeLimiter = new StartTimeLimiter(_startTimeLimitation);
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
        public void VerifyClone()
        {
					_startTimeLimiter.SetId(Guid.NewGuid());
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
            //If no shifts, return false
            Assert.IsFalse(_startTimeLimiter.IsValidAtEnd(Enumerable.Empty<IMainShiftLayer>().CreateProjection()));
        }

    }
}
