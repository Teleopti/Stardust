using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ScheduleDayWorkShiftTimeExtractorTest
    {
        private MockRepository _mock;
        private IScheduleDayWorkShiftTimeExtractor _target;
        private IScheduleDay _scheduleDay;
        private IPersonAssignment _personAssignment;
        private IProjectionService _projectionService;
        private IVisualLayerCollection _visualLayerCollection;
        private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _workTimeStartEndExtractor = _mock.StrictMock<IWorkTimeStartEndExtractor>();
            _target = new ScheduleDayWorkShiftTimeExtractor(_workTimeStartEndExtractor );
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
            _projectionService = _mock.StrictMock<IProjectionService>();
            _visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
        }

        [Test]
        public void ReturnNullIfNoMainShiftFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
            }
            using (_mock.Playback())
            {
                Assert.IsNull(_target.ShiftStartEndTime(_scheduleDay));
            }
        }
        [Test]
        public void ReturnShiftStartEndTimeOnMainShift()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(_visualLayerCollection))
                    .Return(new DateTime(2014, 03, 19, 7, 0, 0, DateTimeKind.Utc));
                Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(_visualLayerCollection))
                    .Return(new DateTime(2014, 03, 19, 17, 0, 0, DateTimeKind.Utc));
            }
            using (_mock.Playback())
            {
                var shiftStartEndTime = _target.ShiftStartEndTime(_scheduleDay);
                Assert.AreEqual(new DateTime(2014, 03, 19, 7, 0, 0, DateTimeKind.Utc),shiftStartEndTime.Value.StartDateTime );
                Assert.AreEqual(new DateTime(2014, 03, 19, 17, 0, 0, DateTimeKind.Utc),shiftStartEndTime.Value.EndDateTime  );
            }
        }
    }

   
}
