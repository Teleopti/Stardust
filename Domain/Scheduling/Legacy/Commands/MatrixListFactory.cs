using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class MatrixListFactory
	{
		private readonly MatrixUserLockLocker _matrixUserLockLocker;
		private readonly MatrixNotPermittedLocker _matrixNotPermittedLocker;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;

		public MatrixListFactory(MatrixUserLockLocker matrixUserLockLocker,
			MatrixNotPermittedLocker matrixNotPermittedLocker, IPersonListExtractorFromScheduleParts personExtractor,
			PeriodExtractorFromScheduleParts periodExtractor)
		{
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
			_personExtractor = personExtractor;
			_periodExtractor = periodExtractor;
		}

		public IEnumerable<IScheduleMatrixPro> CreateMatrixListAllForLoadedPeriod(IScheduleDictionary schedules, IEnumerable<IPerson> personsInOrganization, DateOnlyPeriod selectedPeriod)
		{
			var period = schedules.Period.LoadedPeriod().ToDateOnlyPeriod(TimeZoneInfo.Utc).Inflate(10);
			var matrixes = createMatrixes(schedules, personsInOrganization, period);
			_matrixUserLockLocker.Execute(matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);
			return matrixes;
		}

		public IEnumerable<IScheduleMatrixPro> CreateMatrixListForSelection(IScheduleDictionary schedules, IEnumerable<IScheduleDay> scheduleDays)
		{
			var selectedPeriod = _periodExtractor.ExtractPeriod(scheduleDays);
			if (!selectedPeriod.HasValue)
				return Enumerable.Empty<IScheduleMatrixPro>();

			var selectedPersons = _personExtractor.ExtractPersons(scheduleDays);

			var matrixes = createMatrixes(schedules, selectedPersons, selectedPeriod.Value);
			_matrixUserLockLocker.Execute(matrixes, selectedPeriod.Value);
			_matrixNotPermittedLocker.Execute(matrixes);
			return matrixes;
		}

		public IEnumerable<IScheduleMatrixPro> CreateMatrixListForSelection(IScheduleDictionary schedules, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod)
		{
			var matrixes = createMatrixes(schedules, selectedAgents, selectedPeriod);
			_matrixUserLockLocker.Execute(matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);
			return matrixes;
		}

		private static IList<IScheduleMatrixPro> createMatrixes(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var matrixes = new List<IScheduleMatrixPro>();
			var startDate = period.StartDate;
			foreach (var person in agents)
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