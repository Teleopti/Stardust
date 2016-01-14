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
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly PeriodExctractorFromScheduleParts _periodExctractor;

		public MatrixListFactory(Func<ISchedulerStateHolder> schedulerStateHolder,
			IMatrixUserLockLocker matrixUserLockLocker,
			IMatrixNotPermittedLocker matrixNotPermittedLocker, IPersonListExtractorFromScheduleParts personExtractor,
			PeriodExctractorFromScheduleParts periodExctractor)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_personExtractor = personExtractor;
			_periodExctractor = periodExctractor;
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

		public IList<IScheduleMatrixPro> CreateMatrixListForSelection(IList<IScheduleDay> scheduleDays)
		{
			var matrixes = new List<IScheduleMatrixPro>();
			var selectedPeriod = _periodExctractor.ExtractPeriod(scheduleDays);
			if (!selectedPeriod.HasValue)
				return matrixes;

			var startDate = selectedPeriod.Value.StartDate;
			var selectedPersons = _personExtractor.ExtractPersons(scheduleDays);			
			foreach (var person in selectedPersons)
			{
				var date = startDate;
				while (date <= selectedPeriod.Value.EndDate)
				{
					var matrix = createMatrixForPersonAndDate(person, date);
					if (matrix == null)
					{
						date = date.AddDays(1);
						continue;
					}
					matrixes.Add(matrix);
					date = matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1);
				}
			}

			_matrixUserLockLocker.Execute(matrixes, selectedPeriod.Value);
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
	}
}