﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDayCountMapper
	{
		IAnalyticsFactScheduleDayCount Map(ProjectionChangedEventScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, int scenarioId, int shiftCategoryId);
	}

	public class AnalyticsFactScheduleDayCountMapper : IAnalyticsFactScheduleDayCountMapper
	{
		private readonly IAnalyticsFactScheduleDateMapper _dateMapper;
		private readonly IAnalyticsFactScheduleTimeMapper _timeMapper;

		public AnalyticsFactScheduleDayCountMapper(
			IAnalyticsFactScheduleDateMapper dateMapper,
			IAnalyticsFactScheduleTimeMapper timeMapper)

		{
			_dateMapper = dateMapper;
			_timeMapper = timeMapper;
		}

		public IAnalyticsFactScheduleDayCount Map(
			ProjectionChangedEventScheduleDay scheduleDay,
			IAnalyticsFactSchedulePerson personPart,
			int scenarioId,
			int shiftCategoryId)

		{
			int dateId;
			if (!_dateMapper.MapDateId(new DateOnly(scheduleDay.Date), out dateId)) return null;

			DateTime starTime;
			var absenceId = -1;

			if (scheduleDay.IsFullDayAbsence)
			{
				var firstLayer = scheduleDay.Shift.Layers.First();
				starTime = firstLayer.StartDateTime;
				var absence = _timeMapper.MapAbsenceId(firstLayer.PayloadId);
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