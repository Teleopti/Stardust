﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleProjectionProvider : ITeamScheduleProjectionProvider
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser,
			ICommonAgentNameProvider commonAgentNameProvider)
		{
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		public GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential)
		{
			var scheduleVm = new GroupScheduleShiftViewModel();
			var layers = new List<GroupScheduleLayerViewModel>();
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			scheduleVm.PersonId = scheduleDay.Person.Id.GetValueOrDefault().ToString();
			scheduleVm.Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(scheduleDay.Person);
			scheduleVm.Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat();
			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPart();

			var overtimeStart = DateTime.MaxValue;
			if (scheduleDay.PersonAssignment() != null && scheduleDay.PersonAssignment().OvertimeActivities().Any())
			{
				var overtimeActivities = scheduleDay.PersonAssignment().OvertimeActivities().ToList();
				overtimeStart = overtimeActivities.Select(x => x.Period.StartDateTime).Min();
			}

			switch (significantPart)
			{
				case SchedulePartView.FullDayAbsence:
					scheduleVm.IsFullDayAbsence = true;
					break;
				case SchedulePartView.DayOff:
				case SchedulePartView.ContractDayOff:
					var dayOff = scheduleDay.PersonAssignment().DayOff();
					var dayOffStart = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime;
					var dayOffEnd = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime;

					scheduleVm.DayOff = new GroupScheduleDayOffViewModel
					{
						DayOffName = dayOff != null ?dayOff.Description.Name:"",
						Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToFixedDateTimeFormat(),
						Minutes = (int)dayOffEnd.Subtract(dayOffStart).TotalMinutes

					};
					if (projection.HasLayers)
						scheduleVm.IsFullDayAbsence = true;
					break;
			}

			if (projection != null && projection.HasLayers)
			{
				scheduleVm.WorkTimeMinutes = projection.WorkTime().Minutes;
				scheduleVm.ContractTimeMinutes = projection.ContractTime().Minutes;

				foreach (var layer in projection)
				{
					var isPayloadAbsence = layer.Payload is IAbsence;
					var isAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);

					var description = isPayloadAbsence
						? (isAbsenceConfidential && !canViewConfidential
							? ConfidentialPayloadValues.Description
							: (layer.Payload as IAbsence).Description)
						: layer.DisplayDescription();

					layers.Add(new GroupScheduleLayerViewModel
					{
						Description = description.Name,
						Color =
							isPayloadAbsence
								? (isAbsenceConfidential && !canViewConfidential
									? ConfidentialPayloadValues.DisplayColorHex
									: (layer.Payload as IAbsence).DisplayColor.ToHtml())
								: layer.DisplayColor().ToHtml(),
						Start = startDateTimeInUserTimeZone.ToFixedDateTimeFormat(),
						Minutes = (int) endDateTimeInUserTimeZone.Subtract(startDateTimeInUserTimeZone).TotalMinutes,
						IsOvertime = layer.Period.StartDateTime >= overtimeStart
					});
				}
			}
			scheduleVm.Projection = layers;
			return scheduleVm;
		}
	}

	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
	}
}