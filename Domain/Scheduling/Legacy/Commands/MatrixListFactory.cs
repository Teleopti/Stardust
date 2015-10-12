using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class MatrixListFactory : IMatrixListFactory
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;

		public MatrixListFactory(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleMatrixListCreator scheduleMatrixListCreator, IMatrixUserLockLocker matrixUserLockLocker, IMatrixNotPermittedLocker matrixNotPermittedLocker)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
		}

		public IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(DateOnlyPeriod selectedPeriod)
		{
			var stateHolder = _schedulerStateHolder();
			var period = stateHolder.RequestedPeriod.DateOnlyPeriod;
			period = new DateOnlyPeriod(period.StartDate.AddDays(-10), period.EndDate.AddDays(10));
			var persons = stateHolder.FilteredPersonDictionary;
			var startDate = period.StartDate;
			var matrixes = new List<IScheduleMatrixPro>();
			foreach (var person in persons)
			{
				var date = startDate;
				while (date <= period.EndDate)
				{
					var matrix = createMatrixForPersonAndDate(person.Value, date);
					if (matrix == null)
					{
						date = date.AddDays(1);
						continue;
					}
					matrixes.Add(matrix);
					date = matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1);
				}
			}
			_matrixUserLockLocker.Execute(matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);


			return matrixes;
		}

		private IScheduleMatrixPro createMatrixForPersonAndDate(IPerson person, DateOnly date)
		{
			var virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
			if (!virtualSchedulePeriod.IsValid)
				return null;
			
			IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
				new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, person);

			return new ScheduleMatrixPro(_schedulerStateHolder().SchedulingResultState,
				fullWeekOuterWeekPeriodCreator,
				virtualSchedulePeriod);
		}

		public IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			if (scheduleDays == null)
				throw new ArgumentNullException("scheduleDays");

			IList<IScheduleMatrixPro> matrixes = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(scheduleDays);

			_matrixUserLockLocker.Execute(matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);

			return matrixes;
		}
	}
}