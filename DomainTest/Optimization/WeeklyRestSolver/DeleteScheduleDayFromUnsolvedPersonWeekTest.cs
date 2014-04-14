using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class DeleteScheduleDayFromUnsolvedPersonWeekTest
    {
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private DeleteScheduleDayFromUnsolvedPersonWeek _target;
        private MockRepository _mock;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _deleteSchedulePartService = _mock.StrictMock<IDeleteSchedulePartService>();
            _target = new DeleteScheduleDayFromUnsolvedPersonWeek(_deleteSchedulePartService);
        }

        [Test]
        public void ShouldDeleteProvidedScheduleDay()
        {
            var personRange = _mock.StrictMock<IScheduleRange>();
            var date = new DateOnly(2014,03,26);
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            IList<IScheduleDay> scheduleDayList = new List<IScheduleDay>( ) { scheduleDay };
            var deleteOption = new DeleteOption { Default = true };
            var  rollbackService =_mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            using(_mock.Record())
            {
                
                Expect.Call(personRange.ScheduledDay(date.AddDays(-1))).Return(scheduleDay);
                Expect.Call(_deleteSchedulePartService.Delete(scheduleDayList, deleteOption, rollbackService,
                    new BackgroundWorker())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.DeleteAppropiateScheduleDay(personRange,date,rollbackService);
            }
        }
    }
}
