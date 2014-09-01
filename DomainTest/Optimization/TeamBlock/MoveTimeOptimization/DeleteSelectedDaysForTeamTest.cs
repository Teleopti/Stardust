using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class DeleteSelectedDaysForTeamTest
	{
		private IDeleteSelectedDaysForTeam _target;
		private MockRepository _mock;
		private IList<IScheduleMatrixPro> _matrixList;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private ISchedulePartModifyAndRollbackService _rollback;
		private IVirtualSchedulePeriod _virtualSchedulePeriod1;
		private DateOnlyPeriod _dateOnlyPeriod1;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDay _scheduleDay1;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private IScheduleDay _scheduleDay2;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_rollback = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_virtualSchedulePeriod1 = _mock.StrictMock<IVirtualSchedulePeriod>();
			_deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_target = new DeleteSelectedDaysForTeam(_deleteAndResourceCalculateService);
			_dateOnlyPeriod1 = new DateOnlyPeriod(2014,09,01,2014,09,05);
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldExecuteIfOneDateIsInMatrixList()
		{
			_matrixList = new List<IScheduleMatrixPro>(){_scheduleMatrixPro};
			var firstDay = new DateOnly(2014, 09,02);
			var secondDate = new DateOnly(2014, 09, 08);
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod1);
				Expect.Call(_virtualSchedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1);
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(firstDay)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);

				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod1);
				Expect.Call(_virtualSchedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1);

				_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> {_scheduleDay1}, _rollback,
					true);
			}
			using (_mock.Playback())
			{
				_target.PerformDelete(_matrixList,firstDay,secondDate,_rollback,true);
			}
		}

		[Test]
		public void ShouldExecuteIfBothDateIsInMatrixList()
		{
			_matrixList = new List<IScheduleMatrixPro>() { _scheduleMatrixPro };
			var firstDay = new DateOnly(2014, 09, 02);
			var secondDate = new DateOnly(2014, 09, 03);
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod1);
				Expect.Call(_virtualSchedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1);
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(firstDay)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);

				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod1);
				Expect.Call(_virtualSchedulePeriod1.DateOnlyPeriod).Return(_dateOnlyPeriod1);
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(secondDate)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay2);

				_deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay> { _scheduleDay1,_scheduleDay2  }, _rollback,
					true);
			}
			using (_mock.Playback())
			{
				_target.PerformDelete(_matrixList, firstDay, secondDate, _rollback, true);
			}
		}
	}

	
}
