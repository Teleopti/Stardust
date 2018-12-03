using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class DeleteScheduleDayFromUnsolvedPersonWeekTest
    {
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private DeleteScheduleDayFromUnsolvedPersonWeek _target;
        private MockRepository _mock;
	    private IScheduleMatrixPro _scheduleMatrixPro;
	    private IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _deleteSchedulePartService = _mock.StrictMock<IDeleteSchedulePartService>();
			_scheduleDayIsLockedSpecification = _mock.StrictMock<IScheduleDayIsLockedSpecification>();
	        _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_target = new DeleteScheduleDayFromUnsolvedPersonWeek(_deleteSchedulePartService, _scheduleDayIsLockedSpecification, new CorrectAlteredBetween(new UtcTimeZone()), new OptimizerActivitiesPreferencesFactory());
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
                
                Expect.Call(personRange.ScheduledDay(date.AddDays(-1)))
					.Return(scheduleDay);
	            Expect.Call(_scheduleDayIsLockedSpecification.IsSatisfy(scheduleDay, _scheduleMatrixPro))
		            .Return(false);
                Expect.Call(_deleteSchedulePartService.Delete(scheduleDayList, deleteOption, rollbackService,
                    new NoSchedulingProgress())).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.DeleteAppropiateScheduleDay(personRange,date,rollbackService, new DateOnlyPeriod(2014,03,25,2014,03,27), _scheduleMatrixPro, null);
            }
        }

		  [Test]
		  public void ShouldNotDeleteProvidedScheduleDayIfOutSideSelection()
		  {
			  var personRange = _mock.StrictMock<IScheduleRange>();
			  var date = new DateOnly(2014, 03, 26);
			  var rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			  _target.DeleteAppropiateScheduleDay(personRange, date, rollbackService, new DateOnlyPeriod(2014, 04, 25, 2014, 04, 27), _scheduleMatrixPro, null);
		  }
    }
}
