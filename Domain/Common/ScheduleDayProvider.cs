using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class ScheduleDayProvider : IScheduleDayProvider
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public ScheduleDayProvider(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public IScheduleDictionary GetScheduleDictionary(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null)
		{
			var period = date.ToDateOnlyPeriod().Inflate(1);
			var scheduleDicLoadOption = loadOptions ?? new ScheduleDictionaryLoadOptions(false, false);
			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				scheduleDicLoadOption,
				period,
				_currentScenario.Current());
			return dictionary;
		}

		public IScheduleDictionary GetScheduleDictionary(DateOnlyPeriod period, IEnumerable<IPerson> persons, ScheduleDictionaryLoadOptions loadOptions = null)
		{
			var scheduleDicLoadOption = loadOptions ?? new ScheduleDictionaryLoadOptions(false, false);
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons.ToArray(),
				scheduleDicLoadOption,
				period,
				_currentScenario.Current());
			return dictionary;
		}

		public IScheduleDictionary GetScheduleDictionary(DateOnly date, IEnumerable<IPerson> persons, ScheduleDictionaryLoadOptions loadOptions = null)
		{
			var period = date.ToDateOnlyPeriod().Inflate(1);
			var scheduleDicLoadOption = loadOptions ?? new ScheduleDictionaryLoadOptions(false, false);
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons.ToArray(),
				scheduleDicLoadOption,
				period,
				_currentScenario.Current());
			return dictionary;
		}

		public IScheduleDay GetScheduleDay(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null)
		{
			var scheduleDicLoadOption = loadOptions ?? new ScheduleDictionaryLoadOptions(false, false);
			var dictionary = GetScheduleDictionary(date, person, scheduleDicLoadOption);
			return dictionary[person].ScheduledDay(date);
		}

		public IEnumerable<IScheduleDay> GetScheduleDays(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScenario scenario)
		{
			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons.ToArray(),
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return dictionary.SchedulesForPeriod(period, persons.ToArray());
		}
	}
}