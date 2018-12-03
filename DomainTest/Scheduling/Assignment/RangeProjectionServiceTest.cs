using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class RangeProjectionServiceTest
    {
        private IRangeProjectionService _target;
        private MockRepository _mocks;
        private IScheduleRange _scheduleRange;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
            _target = new RangeProjectionService();
        }

        [Test]
        public void VerifyCreateProjection()
        {
            var period = new DateTimePeriod(new DateTime(2010, 5, 24, 22, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2010, 5, 26, 10, 0, 0, DateTimeKind.Utc));
            var activity = ActivityFactory.CreateActivity("Phone");
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            var projectionService1 = _mocks.StrictMock<IProjectionService>();
            var projectionService2 = _mocks.StrictMock<IProjectionService>();
            var visualLayerFactory = new VisualLayerFactory();
            var visualLayer1 = visualLayerFactory.CreateShiftSetupLayer(activity,
                                                                        new DateTimePeriod(period.StartDateTime.AddHours(2),
                                                                                           period.StartDateTime.AddHours(4)));
            var visualLayerCollection1 = new VisualLayerCollection(new List<IVisualLayer>{visualLayer1}, new ProjectionPayloadMerger());
            var visualLayer2 = visualLayerFactory.CreateShiftSetupLayer(activity,
                                                                        new DateTimePeriod(period.EndDateTime.AddHours(-4),
                                                                                           period.EndDateTime.AddHours(-2)));
            var visualLayer3 = visualLayerFactory.CreateShiftSetupLayer(activity,
                                                                        new DateTimePeriod(period.EndDateTime.AddHours(2),
                                                                                           period.EndDateTime.AddHours(4)));
            var visualLayerCollection2 = new VisualLayerCollection(new List<IVisualLayer> { visualLayer2,visualLayer3 }, new ProjectionPayloadMerger());
            using (_mocks.Record())
            {
                Expect.Call(_scheduleRange.Person).Return(person);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2010, 5, 25))).Return(scheduleDay1);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2010, 5, 26))).Return(scheduleDay2);
                Expect.Call(scheduleDay1.HasProjection()).Return(true);
                Expect.Call(scheduleDay2.HasProjection()).Return(true);
                Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
                Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
                Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
                Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
            }
            using (_mocks.Playback())
            {
                var result = _target.CreateProjection(_scheduleRange, period);
                Assert.AreEqual(2, result.Count());
            }
        }
    }
}