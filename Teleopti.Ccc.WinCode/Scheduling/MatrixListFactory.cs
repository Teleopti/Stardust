using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class MatrixListFactory : IMatrixListFactory
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;

		public MatrixListFactory(ISchedulerStateHolder schedulerStateHolder, IMatrixUserLockLocker matrixUserLockLocker, IMatrixNotPermittedLocker matrixNotPermittedLocker)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
		}

		public IList<IScheduleMatrixPro> CreateMatrixListAll(DateOnlyPeriod selectedPeriod)
		{
			var allSchedules = new List<IScheduleDay>();
			var period = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			period = new DateOnlyPeriod(period.StartDate.AddDays(-10), period.EndDate.AddDays(10));
			var persons = _schedulerStateHolder.FilteredPersonDictionary;

			foreach (var day in period.DayCollection())
			{
				foreach (var person in persons)
				{
					var theDay = _schedulerStateHolder.Schedules[person.Value].ScheduledDay(day);
					allSchedules.Add(theDay);
				}
			}

			return CreateMatrixList(allSchedules, selectedPeriod);
		}

		public IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			if (scheduleDays == null)
				throw new ArgumentNullException("scheduleDays");

			IList<IScheduleMatrixPro> matrixes =
				new ScheduleMatrixListCreator(_schedulerStateHolder.SchedulingResultState).CreateMatrixListFromScheduleParts(scheduleDays);

			_matrixUserLockLocker.Execute(scheduleDays, matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);

			return matrixes;
		}
	}
}