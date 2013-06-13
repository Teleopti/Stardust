

using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface IMatrixListFactory
	{
		IList<IScheduleMatrixPro> CreateMatrixListAll(DateOnlyPeriod selectedPeriod);
		IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod);
	}

	public class MatrixListFactory : IMatrixListFactory
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;

		public MatrixListFactory(ISchedulerStateHolder schedulerStateHolder, IMatrixUserLockLocker matrixUserLockLocker)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_matrixUserLockLocker = matrixUserLockLocker;
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

			return matrixes;
		} 
	}
}