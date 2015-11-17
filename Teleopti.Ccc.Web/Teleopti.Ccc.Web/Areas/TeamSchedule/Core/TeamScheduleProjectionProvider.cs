using System;
using System.Collections.Generic;
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

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser)
		{
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
		}

		public GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential)
		{
			var scheduleVM = new GroupScheduleShiftViewModel();
			var layers = new List<GroupScheduleLayerViewModel>();
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			scheduleVM.PersonId = scheduleDay.Person.Id.GetValueOrDefault().ToString();
			scheduleVM.Name = scheduleDay.Person.Name.ToString();
			scheduleVM.Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat();
			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPartForDisplay();
			switch (significantPart)
			{
				case SchedulePartView.FullDayAbsence:
					scheduleVM.IsFullDayAbsence = true;
					break;
				case SchedulePartView.DayOff:
					var dayOff = scheduleDay.PersonAssignment().DayOff();
					var dayOffStart = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime;
					var dayOffEnd = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime;

					scheduleVM.DayOff = new GroupScheduleDayOffViewModel
					{
						DayOffName = dayOff.Description.Name,
						Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToFixedDateTimeFormat(),
						Minutes = (int)dayOffEnd.Subtract(dayOffStart).TotalMinutes

					};
					if (projection.HasLayers)
						scheduleVM.IsFullDayAbsence = true;
					break;

			}
			if (projection != null && projection.HasLayers)
			{
				scheduleVM.WorkTimeMinutes = projection.WorkTime().Minutes;
				scheduleVM.ContractTimeMinutes = projection.ContractTime().Minutes;


				foreach (var layer in projection)
				{
					var isPayloadAbsence = (layer.Payload is IAbsence);
					var isAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);
					var description = isPayloadAbsence
						? (isAbsenceConfidential && !canViewConfidential ? ConfidentialPayloadValues.Description : (layer.Payload as IAbsence).Description)
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
						Minutes = (int) endDateTimeInUserTimeZone.Subtract(startDateTimeInUserTimeZone).TotalMinutes

					});
				}
			}
			scheduleVM.Projection = layers;
			return scheduleVM;
		}
	}

	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
	}
}