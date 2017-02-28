using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class MatrixListFactory : IMatrixListFactory
	{
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;

		public MatrixListFactory(IMatrixUserLockLocker matrixUserLockLocker,
			IMatrixNotPermittedLocker matrixNotPermittedLocker, IPersonListExtractorFromScheduleParts personExtractor,
			PeriodExtractorFromScheduleParts periodExtractor)
		{
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_personExtractor = personExtractor;
			_periodExtractor = periodExtractor;
		}

		public IList<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(IScheduleDictionary schedules, IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod selectedPeriod)
		{
			var period = schedules.Period.LoadedPeriod().ToDateOnlyPeriod(TimeZoneInfo.Utc).Inflate(10); 
			var startDate = period.StartDate;
			var matrixes = new List<IScheduleMatrixPro>();
			foreach (var person in personsInOrganization)
			{
				var date = startDate;
				while (date <= period.EndDate)
				{
					var matrix = createMatrixForPersonAndDate(schedules, person, date);
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

		public IList<IScheduleMatrixPro> CreateMatrixListForSelection(IScheduleDictionary schedules, IEnumerable<IScheduleDay> scheduleDays)
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
					var matrix = createMatrixForPersonAndDate(schedules, person, date);
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

		private static IScheduleMatrixPro createMatrixForPersonAndDate(IScheduleDictionary schedules, IPerson person, DateOnly date)
		{
			var virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
			if (!virtualSchedulePeriod.IsValid)
				return null;
			
			IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
				new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, person);

			return new ScheduleMatrixPro(schedules[person],
				fullWeekOuterWeekPeriodCreator,
				virtualSchedulePeriod);
		}
	}
}