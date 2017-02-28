using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IExportToScenarioAbsenceFinder
	{
		Dictionary<IPerson, HashSet<IAbsence>> Find(IScenario exportScenario,
			IScheduleDictionary scheduleDictionaryExportTo,
			IEnumerable<IPerson> persons,
			IEnumerable<IScheduleDay> scheduleDaysToExport,
			ICollection<DateOnly> datesToExport);
	}

	public class ExportToScenarioAbsenceFinder : IExportToScenarioAbsenceFinder
	{
		public Dictionary<IPerson, HashSet<IAbsence>> Find(IScenario exportScenario, 
			IScheduleDictionary scheduleDictionaryExportTo, 
			IEnumerable<IPerson> persons, 
			IEnumerable<IScheduleDay> scheduleDaysToExport, 
			ICollection<DateOnly> datesToExport)
		{
			var involvedAbsences = new Dictionary<IPerson, HashSet<IAbsence>>();

			if (!exportScenario.DefaultScenario) return involvedAbsences;

			var minDay = DateOnly.MaxValue;
			var maxDay = DateOnly.MinValue;

			foreach (var dateOnly in datesToExport)
			{
				if (dateOnly < minDay)
					minDay = dateOnly;

				if (dateOnly > maxDay)
					maxDay = dateOnly;
			}

			foreach (var person in persons)
			{
				var schedules = scheduleDictionaryExportTo[person];
				var scheduleData = schedules.ScheduledDayCollection(new DateOnlyPeriod(minDay, maxDay));
				var absences = new HashSet<IAbsence>();

				foreach (var personAbsence in scheduleData.SelectMany(scheduleDay => scheduleDay.PersonAbsenceCollection()))
				{
					absences.Add(personAbsence.Layer.Payload);
				}

				involvedAbsences.Add(person, absences);
			}

			foreach (var scheduleDay in scheduleDaysToExport)
			{
				foreach (var personAbsence in scheduleDay.PersonAbsenceCollection())
				{
					involvedAbsences[scheduleDay.Person].Add(personAbsence.Layer.Payload);
				}
			}

			return involvedAbsences;
		}
	}
}