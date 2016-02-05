using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class NightRestWhiteSpotSolverServiceTest
    {
        private INightRestWhiteSpotSolverService _target;
        private INightRestWhiteSpotSolver _solver;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IScheduleService _scheduleService;
        private IPerson _person;
        private ISchedulingOptions _schedulingOptions;
    	private IResourceCalculateDelayer _resourceCalculateDelayer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _solver = _mocks.StrictMock<INightRestWhiteSpotSolver>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_deleteAndResourceCalculateService = _mocks.StrictMock<IDeleteAndResourceCalculateService>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _scheduleService = _mocks.StrictMock<IScheduleService>();
            _person = PersonFactory.CreatePerson();
            _schedulingOptions = new SchedulingOptions();
        	_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_target = new NightRestWhiteSpotSolverService(_solver, _deleteAndResourceCalculateService, _scheduleService, ()=>new WorkShiftFinderResultHolder(), _resourceCalculateDelayer);
        }

        [Test]
        public void ShouldReturnFalseIfReschedulingFails()
        {
            NightRestWhiteSpotSolverResult nightRestWhiteSpotSolverResult = new NightRestWhiteSpotSolverResult();
            nightRestWhiteSpotSolverResult.DaysToDelete.Add(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 2));

            IScheduleDay scheduleDayToDelete = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay scheduleDayToFill = _mocks.StrictMock<IScheduleDay>();
            IScheduleDayPro day1 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day2 = _mocks.StrictMock<IScheduleDayPro>();

            using(_mocks.Record())
            {
                Expect.Call(_solver.Resolve(_matrix)).Return(nightRestWhiteSpotSolverResult);
                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToDelete[0])).Return(day1).Repeat.Twice();
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete).Repeat.Twice();
				Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { scheduleDayToDelete },
															  _schedulePartModifyAndRollbackService, true, false));

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[0])).Return(day2);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(day2.DaySchedulePart()).Return(scheduleDayToFill);
				Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToFill, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToDelete, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(false);
                
            }

            bool result;

            using(_mocks.Playback())
            {
                result = _target.Resolve(_matrix, _schedulingOptions, _schedulePartModifyAndRollbackService);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldRemoveIndicatedDaysAndRescheduleAllDays()
        {
            NightRestWhiteSpotSolverResult nightRestWhiteSpotSolverResult = new NightRestWhiteSpotSolverResult();
            nightRestWhiteSpotSolverResult.DaysToDelete.Add(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 2));

            IScheduleDay scheduleDayToDelete = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay scheduleDayToFill = _mocks.StrictMock<IScheduleDay>();
            IScheduleDayPro day1 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day2 = _mocks.StrictMock<IScheduleDayPro>();

            using(_mocks.Record())
            {
                Expect.Call(_solver.Resolve(_matrix)).Return(nightRestWhiteSpotSolverResult);
                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToDelete[0])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
				Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { scheduleDayToDelete },
															  _schedulePartModifyAndRollbackService, true, false));

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[0])).Return(day2);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(day2.DaySchedulePart()).Return(scheduleDayToFill);
				Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToFill, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(true);

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[1])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
				Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToDelete, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(true);
            }

            bool result;

            using(_mocks.Playback())
            {
                result = _target.Resolve(_matrix, _schedulingOptions, _schedulePartModifyAndRollbackService);
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldReturnFalseIfNoWhiteSpotFound()
        {
            NightRestWhiteSpotSolverResult nightRestWhiteSpotSolverResult = new NightRestWhiteSpotSolverResult();

            using (_mocks.Record())
            {
                Expect.Call(_solver.Resolve(_matrix)).Return(nightRestWhiteSpotSolverResult);
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, _schedulingOptions, _schedulePartModifyAndRollbackService);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldRescheduleDeletedDays()
        {
            NightRestWhiteSpotSolverResult nightRestWhiteSpotSolverResult = new NightRestWhiteSpotSolverResult();
            nightRestWhiteSpotSolverResult.DaysToDelete.Add(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 2));

            IScheduleDay scheduleDayToDelete = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay scheduleDayToFill = _mocks.StrictMock<IScheduleDay>();
            IScheduleDayPro day1 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day2 = _mocks.StrictMock<IScheduleDayPro>();

            using (_mocks.Record())
            {
                Expect.Call(_solver.Resolve(_matrix)).Return(nightRestWhiteSpotSolverResult);
                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToDelete[0])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
                Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { scheduleDayToDelete },
                                                              _schedulePartModifyAndRollbackService, true, false));

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[0])).Return(day2);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(day2.DaySchedulePart()).Return(scheduleDayToFill);
                Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToFill, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(false);

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[1])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
                Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToDelete, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(true);
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, _schedulingOptions, _schedulePartModifyAndRollbackService);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnTrueIfTheBlankSpotIsResolved()
        {
            NightRestWhiteSpotSolverResult nightRestWhiteSpotSolverResult = new NightRestWhiteSpotSolverResult();
            nightRestWhiteSpotSolverResult.DaysToDelete.Add(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 1));
            nightRestWhiteSpotSolverResult.AddDayToReschedule(new DateOnly(2011, 1, 2));

            IScheduleDay scheduleDayToDelete = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay scheduleDayToFill = _mocks.StrictMock<IScheduleDay>();
            IScheduleDayPro day1 = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro day2 = _mocks.StrictMock<IScheduleDayPro>();

            using (_mocks.Record())
            {
                Expect.Call(_solver.Resolve(_matrix)).Return(nightRestWhiteSpotSolverResult);
                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToDelete[0])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
                Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { scheduleDayToDelete },
                                                              _schedulePartModifyAndRollbackService, true, false));

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[0])).Return(day2);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(day2.DaySchedulePart()).Return(scheduleDayToFill);
                Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToFill, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(true);

                Expect.Call(_matrix.GetScheduleDayByKey(nightRestWhiteSpotSolverResult.DaysToReschedule()[1])).Return(day1);
                Expect.Call(day1.DaySchedulePart()).Return(scheduleDayToDelete);
                Expect.Call(_scheduleService.SchedulePersonOnDay(scheduleDayToDelete, _schedulingOptions, _resourceCalculateDelayer, _schedulePartModifyAndRollbackService)).Return(false);
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, _schedulingOptions, _schedulePartModifyAndRollbackService);
            }

            Assert.IsTrue(result);
        }
    }
}