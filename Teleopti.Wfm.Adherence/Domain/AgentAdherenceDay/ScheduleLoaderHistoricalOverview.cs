using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public class ScheduleLoaderHistoricalOverview : IScheduleLoader
	{
		private readonly IPersonRepository _persons;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IScenarioRepository _scenarios;
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleLoaderHistoricalOverview(
			IPersonRepository persons,
			IBusinessUnitRepository businessUnits,
			IScenarioRepository scenarios,
			IScheduleStorage scheduleStorage)
		{
			_persons = persons;
			_businessUnits = businessUnits;
			_scenarios = scenarios;
			_scheduleStorage = scheduleStorage;
		}

		public IEnumerable<IVisualLayer> Load(Guid personId, DateOnly date)
		{
			var person = _persons.Load(personId);
			if (person == null)
				return Enumerable.Empty<IVisualLayer>();

			var businessUnitId = person.Period(new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(date.Date))?.Team?.Site?.BusinessUnit?.Id;
			if (businessUnitId == null)
				return Enumerable.Empty<IVisualLayer>();
			var businessUnit = _businessUnits.Load(businessUnitId.GetValueOrDefault());
			var scenario = _scenarios.LoadDefaultScenario(businessUnit);
			if (scenario == null)
				return Enumerable.Empty<IVisualLayer>();

			var period = new DateOnlyPeriod(date.Date, date.Date);

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return schedules[person]
				.ScheduledDay(period.StartDate)
				.ProjectionService()
				.CreateProjection();
		}
	}
}