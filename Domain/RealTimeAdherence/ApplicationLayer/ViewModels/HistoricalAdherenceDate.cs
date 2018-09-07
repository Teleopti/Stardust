using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class HistoricalAdherenceDate
	{
		private readonly INow _now;
		private readonly ICurrentScenario _scenario;
		private readonly IPersonRepository _persons;
		private readonly IScheduleStorage _scheduleStorage;

		public HistoricalAdherenceDate(
			INow now,
			ICurrentScenario scenario,
			IPersonRepository persons,
			IScheduleStorage scheduleStorage)
		{
			_now = now;
			_scenario = scenario;
			_persons = persons;
			_scheduleStorage = scheduleStorage;
		}

		public DateOnly MostRecentShiftDate(Guid personId)
		{
			var person = _persons.Load(personId);
			var now = _now.UtcDateTime();
			var agentNow = TimeZoneInfo.ConvertTimeFromUtc(now, person.PermissionInformation.DefaultTimeZone());
			var agentDate = new DateOnly(agentNow);
			
			var period = new DateOnlyPeriod(agentDate.AddDays(-6), agentDate);
			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] {person},
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_scenario.Current());
			var scheduleDays = schedules[person].ScheduledDayCollection(period);

			var activities =
				from scheduleDay in scheduleDays
				where scheduleDay.SignificantPart() != SchedulePartView.FullDayAbsence
				let projection = scheduleDay.ProjectionService().CreateProjection()
				from projectedLayer in projection
				select new
				{
					Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
					Activity = projectedLayer
				};

			var lastActivity = activities.LastOrDefault(x => x.Activity.Period.StartDateTime < now);
			return lastActivity?.Date ?? agentDate;
		}
	}
}