using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class HistoricalAdherenceViewModelBuilder
	{
		private readonly IHistoricalAdherenceReadModelReader _reader;
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _persons;
		private readonly INow _now;

		public HistoricalAdherenceViewModelBuilder(
			IHistoricalAdherenceReadModelReader reader,
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IPersonRepository persons,
			INow now)
		{
			_reader = reader;
			_scenario = scenario;
			_scheduleStorage = scheduleStorage;
			_persons = persons;
			_now = now;
		}

		private IEnumerable<HistoricalAdherenceActivityViewModel> getCurrentSchedules(IPerson person)
		{
			var scenario = _scenario.Current();
			if (scenario == null || person == null)
				return Enumerable.Empty<HistoricalAdherenceActivityViewModel>();

			var from = new DateOnly(_now.UtcDateTime().AddDays(-1));
			var to = new DateOnly(_now.UtcDateTime().AddDays(1));

			var period = new DateOnlyPeriod(from.AddDays(-1), to.AddDays(1));

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return (
				from scheduleDay in schedules[person].ScheduledDayCollection(period)
				from layer in scheduleDay.ProjectionService().CreateProjection()
				select new HistoricalAdherenceActivityViewModel
				{
					Color = ColorTranslator.ToHtml(layer.DisplayColor()),
					StartTime = layer.Period.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
					EndTime = layer.Period.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss")
				})
				.ToArray();
		}

		public HistoricalAdherenceViewModel Build(Guid personId)
		{
			var result = _reader.Read(personId, _now.UtcDateTime().Date, _now.UtcDateTime().AddDays(1).Date);
			var person = _persons.Load(personId);
			var schedule = getCurrentSchedules(person);
			
			return new HistoricalAdherenceViewModel
			{
				PersonId = personId,
				AgentName = person?.Name.ToString(),
				Schedules = schedule,
				Now = _now.UtcDateTime().ToString("yyyy-MM-ddTHH:mm:ss"),
				OutOfAdherences = (result?.OutOfAdherences).EmptyIfNull().Select(y =>
				{
					string endTime = null;
					if (y.EndTime.HasValue)
						endTime = y.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss");
					return new AgentOutOfAdherenceViewModel
					{
						StartTime = y.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
						EndTime = endTime
					};
				})
			};
		}
	}
}