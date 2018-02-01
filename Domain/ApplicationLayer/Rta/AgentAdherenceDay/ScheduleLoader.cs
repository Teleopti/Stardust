﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class ScheduleLoader
	{
		private readonly IPersonRepository _persons;
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;

		public ScheduleLoader(
			IPersonRepository persons,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage)
		{
			_persons = persons;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
		}

		public IEnumerable<IVisualLayer> Load(Guid personId, DateOnly date)
		{
			var schedule = Enumerable.Empty<IVisualLayer>();
			var person = _persons.Load(personId);

			var scenario = _scenario.Current();
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