using System;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDayCountHandler : IAnalyticsFactScheduleDayCountHandler
	{
		private readonly IAnalyticsFactScheduleDateHandler _dateHandler;
		private readonly IAnalyticsFactScheduleTimeHandler _timeHandler;

		public AnalyticsFactScheduleDayCountHandler(
			IAnalyticsFactScheduleDateHandler dateHandler, 
			IAnalyticsFactScheduleTimeHandler timeHandler)

		{
			_dateHandler = dateHandler;
			_timeHandler = timeHandler;
		}

		public IAnalyticsFactScheduleDayCount Handle(
			ProjectionChangedEventScheduleDay scheduleDay,
			IAnalyticsFactSchedulePerson personPart, 
			int scenarioId, 
			int shiftCategoryId)

		{
			int dateId;
			if (!_dateHandler.MapDateId(new DateOnly(scheduleDay.Date), out dateId)) return null;

			DateTime starTime;
			var absenceId = -1;

			if (scheduleDay.IsFullDayAbsence)
			{
				var firstLayer = scheduleDay.Shift.Layers.First();
				starTime = firstLayer.StartDateTime;
				var absence = _timeHandler.MapAbsenceId(firstLayer.PayloadId);
				if (absence == null) return null;
				absenceId = absence.AbsenceId;
			}
			else if (scheduleDay.DayOff != null)
				starTime = scheduleDay.DayOff.Anchor;
			else
			{
				if (shiftCategoryId == -1) return null;
				starTime = scheduleDay.Shift.StartDateTime;
			}

			return new AnalyticsFactScheduleDayCount
			{
				ShiftStartDateLocalId = dateId,
				PersonId = personPart.PersonId,
				BusinessUnitId = personPart.BusinessUnitId,
				ScenarioId = scenarioId,
				ShiftCategoryId = shiftCategoryId,
				StartTime = starTime,
				DayOffName = scheduleDay.DayOff != null ? scheduleDay.Name : null,
				DayOffShortName = scheduleDay.DayOff != null ? scheduleDay.ShortName : null,
				AbsenceId = absenceId
			};
		}
	}
}