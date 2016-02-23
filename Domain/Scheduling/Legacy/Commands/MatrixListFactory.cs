using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;

		public MatrixListFactory(Func<ISchedulerStateHolder> schedulerStateHolder,
			IMatrixUserLockLocker matrixUserLockLocker,
			IMatrixNotPermittedLocker matrixNotPermittedLocker, IPersonListExtractorFromScheduleParts personExtractor,
			PeriodExtractorFromScheduleParts periodExtractor)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_personExtractor = personExtractor;
			_periodExtractor = periodExtractor;
		}

		public IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(DateOnlyPeriod selectedPeriod)
		{
			var stateHolder = _schedulerStateHolder();
			var period = stateHolder.RequestedPeriod.DateOnlyPeriod.Inflate(10);
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
			var selectedPeriod = _periodExtractor.ExtractPeriod(scheduleDays);
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

		public IList<IScheduleMatrixPro> CreateMatrixListForSelectionPerPerson(IList<IScheduleDay> scheduleDays)
		{
			var matrixes = new List<IScheduleMatrixPro>();
			var selectedPersons = _personExtractor.ExtractPersons(scheduleDays);
			var scheduleDaysLookup = scheduleDays.ToLookup(s => s.Person);

			foreach (var person in selectedPersons)
			{
				var daysForPerson = scheduleDaysLookup[person].ToList();
				var selectedPeriod = _periodExtractor.ExtractPeriod(daysForPerson);
				if (!selectedPeriod.HasValue) continue;
				var startDate = selectedPeriod.Value.StartDate;
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
					_matrixUserLockLocker.Execute(new List<IScheduleMatrixPro> { matrix }, selectedPeriod.Value);
				}
			}

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