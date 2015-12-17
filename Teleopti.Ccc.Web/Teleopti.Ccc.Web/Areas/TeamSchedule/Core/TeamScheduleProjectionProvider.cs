﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
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
			var overtimeActivities = scheduleDay.PersonAssignment().OvertimeActivities().ToArray();

			switch (significantPart)
			{
				case SchedulePartView.FullDayAbsence:
					scheduleVm.IsFullDayAbsence = true;
					break;
				case SchedulePartView.DayOff:
				case SchedulePartView.ContractDayOff:
					var personAssignment = scheduleDay.PersonAssignment();
					var dayOff = personAssignment != null ? personAssignment.DayOff() : null;
					var dayOffStart = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime;
					var dayOffEnd = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime;

					scheduleVm.DayOff = new GroupScheduleDayOffViewModel
					{
						DayOffName = dayOff != null ? dayOff.Description.Name : "",
						Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToFixedDateTimeFormat(),
						Minutes = (int) dayOffEnd.Subtract(dayOffStart).TotalMinutes
					};
					if (projection.HasLayers)
						scheduleVm.IsFullDayAbsence = true;
					break;
			}

			if (projection != null && projection.HasLayers)
			{
				scheduleVm.WorkTimeMinutes = projection.WorkTime().TotalMinutes;
				scheduleVm.ContractTimeMinutes = projection.ContractTime().TotalMinutes;

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
						IsOvertime = overtimeActivities.Any(overtime => overtime.Period.Contains(layer.Period))
					});
				}
			}
			scheduleVm.Projection = layers;
			return scheduleVm;
		}

		public AgentScheduleViewModelReworked MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential)
		{
			var ret = new AgentScheduleViewModelReworked();
			var layers = new List<LayerViewModelReworked>();
			ret.PersonId = person.Id.GetValueOrDefault();
			ret.Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person);
			if (scheduleDay == null)
			{
				return ret;
			}

			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPart();
			ret.IsDayOff = significantPart == SchedulePartView.DayOff;

			var projectionPeriod = projection.Period();
			if (projectionPeriod != null)
			{
				ret.StartTimeUtc = projectionPeriod.Value.StartDateTime;
			}

			if (projection.HasLayers)
			{

				foreach (var layer in projection)
				{
					var isPayloadAbsence = layer.Payload is IAbsence;
					var isOvertime = layer.DefinitionSet != null && layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime;
					var isAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);

					var description = isPayloadAbsence
						? (isAbsenceConfidential && !isPermittedToViewConfidential
							? ConfidentialPayloadValues.Description
							: (layer.Payload as IAbsence).Description)
						: layer.DisplayDescription();
					var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												startDateTimeInUserTimeZone.ToShortTimeString(),
												endDateTimeInUserTimeZone.ToShortTimeString());

					layers.Add(new LayerViewModelReworked
					{
						TitleHeader = description.Name,
						Color =
							isPayloadAbsence
								? (isAbsenceConfidential && !isPermittedToViewConfidential
									? ConfidentialPayloadValues.DisplayColorHex
									: (layer.Payload as IAbsence).DisplayColor.ToHtml())
								: layer.DisplayColor().ToHtml(),
						Start = startDateTimeInUserTimeZone,
						End = endDateTimeInUserTimeZone,
						LengthInMinutes = (int)endDateTimeInUserTimeZone.Subtract(startDateTimeInUserTimeZone).TotalMinutes,
						IsAbsenceConfidential = isAbsenceConfidential,
						TitleTime = expectedTime,
						IsOvertime = isOvertime
					});
				}
			}
			ret.ScheduleLayers = layers.ToArray();
			ret.MinStart = layers.Min(l => l.Start);
			ret.Total = layers.Count;
			return ret;
		}
	}

	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
		AgentScheduleViewModelReworked MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);
	}
}