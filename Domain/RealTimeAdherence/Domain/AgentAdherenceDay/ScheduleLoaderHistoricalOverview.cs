using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
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
			var schedule = Enumerable.Empty<IVisualLayer>();
			var person = _persons.Load(personId);
			var businessUnitId = person.Period(date)?.Team?.Site?.BusinessUnit?.Id;
			var businessUnit = _businessUnits.Load(businessUnitId.GetValueOrDefault());
			var scenario = _scenarios.LoadDefaultScenario(businessUnit);

			if (scenario != null && person != null)
			{
				var period = new DateOnlyPeriod(date, date);

				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
					new[] {person},
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					scenario);

				schedule = schedules[person]
					.ScheduledDay(date)
					.ProjectionService()
					.CreateProjection();
			}

			return schedule;
		}
	}
}