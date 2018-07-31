using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleProjectionProvider : ITeamScheduleProjectionProvider
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProjectionHelper _projectionHelper;
		private readonly IProjectionSplitter _projectionSplitter;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IPersonNameProvider _personNameProvider;

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser,
			IScheduleProjectionHelper projectionHelper, IProjectionSplitter projectionSplitter,
			IIanaTimeZoneProvider ianaTimeZoneProvider, IPersonNameProvider personNameProvider)
		{
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
			_projectionHelper = projectionHelper;
			_projectionSplitter = projectionSplitter;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_personNameProvider = personNameProvider;
		}

		public GroupScheduleShiftViewModel MakeViewModel(IPerson person, DateOnly date, IScheduleDay scheduleDay, IScheduleDay previousScheduleDay,
			bool canViewConfidential, bool canViewUnpublished, bool isScheduleDate, ICommonNameDescriptionSetting agentNameSetting)
		{
			var personPeriod = person.Period(date);
			var vm = new GroupScheduleShiftViewModel
			{
				PersonId = person.Id.GetValueOrDefault().ToString(),
				Name = agentNameSetting.BuildFor(person),
				Date = date.Date.ToGregorianDateTimeString().Remove(10),
				Projection = new List<GroupScheduleProjectionViewModel>(),
				MultiplicatorDefinitionSetIds =
				(personPeriod != null && personPeriod.PersonContract != null &&
				 personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Count > 0)
					? personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Select(s => s.Id.GetValueOrDefault())
						.ToList()
					: null,
				Timezone = new TimeZoneViewModel
				{
					IanaId = _ianaTimeZoneProvider.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id),
					DisplayName = person.PermissionInformation.DefaultTimeZone().DisplayName
				}
			};

			var isPublished = isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly, person);
			if (isPublished || canViewUnpublished)
			{
				vm = Projection(scheduleDay, canViewConfidential, agentNameSetting);
				if (isScheduleDate) {
					vm.UnderlyingScheduleSummary = getUnderlyingScheduleSummary(scheduleDay, previousScheduleDay, canViewConfidential);
				}
			}

			if (isScheduleDate)
			{
				var note = scheduleDay.NoteCollection().FirstOrDefault();
				vm.InternalNotes = note?.GetScheduleNote(new NormalizeText()) ?? string.Empty;

				var publicNote = scheduleDay.PublicNoteCollection().FirstOrDefault();
				vm.PublicNotes = publicNote?.GetScheduleNote(new NormalizeText()) ?? String.Empty;
			}
			
			var pa = scheduleDay.PersonAssignment();
			vm.IsProtected = pa?.Person.PersonWriteProtection.IsWriteProtected(date) ?? false;
			return vm;
		}

		private UnderlyingScheduleSummary getUnderlyingScheduleSummary(IScheduleDay scheduleDay, IScheduleDay previousScheduleDay, bool canViewConfidential)
		{
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var pa = scheduleDay.PersonAssignment();
			var hasPartTimeAbsence = !scheduleDay.IsFullDayAbsence() && (scheduleDay.PersonAbsenceCollection()?.Any() ?? false);
			var hasPersonalActivities = pa?.PersonalActivities()?.Any() ?? false;
			var hasPersonMeetings = scheduleDay.PersonMeetingCollection()?.Any() ?? false;

			var underlyingSummary = new UnderlyingScheduleSummary();

			if (hasPartTimeAbsence)
			{
				var parttimeAbsences = scheduleDay.PersonAbsenceCollection()
					.Where(personAbsence =>
					{
						var intersectPeriodForYesterday = previousScheduleDay?.PersonAssignment()?.Period.Intersection(personAbsence.Period);
						var intersectPeriod = pa?.Period.Intersection(personAbsence.Period);
						var isBelongsToYesterday = intersectPeriodForYesterday != null && scheduleDay.Period.Intersect(personAbsence.Period) && intersectPeriod == null;
						return !isBelongsToYesterday;
					});
				hasPartTimeAbsence = parttimeAbsences.Any();

				underlyingSummary.PersonPartTimeAbsences = hasPartTimeAbsence ? parttimeAbsences
					.Select(personAbsence => new Summary
					{
						Description = !canViewConfidential && (personAbsence.Layer.Payload as IAbsence).Confidential ? ConfidentialPayloadValues.Description.Name : personAbsence.Layer.Payload.Description.Name,
						Start = TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Period.StartDateTime, timezone).ToFixedDateTimeFormat(),
						End = TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Period.EndDateTime, timezone).ToFixedDateTimeFormat()
					})
					.ToArray() : null;
			}

			underlyingSummary.PersonalActivities = hasPersonalActivities ? pa.PersonalActivities()
				.Select(personalActivity => new Summary
				{
					Description = personalActivity.Payload.Description.Name,
					Start = TimeZoneInfo.ConvertTimeFromUtc(personalActivity.Period.StartDateTime, timezone).ToFixedDateTimeFormat(),
					End = TimeZoneInfo.ConvertTimeFromUtc(personalActivity.Period.EndDateTime, timezone).ToFixedDateTimeFormat()
				})
				.ToArray() : null;

			underlyingSummary.PersonMeetings = hasPersonMeetings ? scheduleDay.PersonMeetingCollection()
				.Select(personMeeting => new Summary
				{
					Description = personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()),
					Start = TimeZoneInfo.ConvertTimeFromUtc(personMeeting.Period.StartDateTime, timezone).ToFixedDateTimeFormat(),
					End = TimeZoneInfo.ConvertTimeFromUtc(personMeeting.Period.EndDateTime, timezone).ToFixedDateTimeFormat()
				})
				.ToArray() : null;

			var hasUnderlyingSchedules = hasPartTimeAbsence
										|| hasPersonMeetings
										|| hasPersonalActivities;

			if (hasUnderlyingSchedules)
			{
				return underlyingSummary;
			}
			return null;
		}

		public GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential, ICommonNameDescriptionSetting agentNameSetting)
		{
			var projections = new List<GroupScheduleProjectionViewModel>();
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var person = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var scheduleVm = new GroupScheduleShiftViewModel
			{
				PersonId = scheduleDay.Person.Id.GetValueOrDefault().ToString(),
				Name = agentNameSetting.BuildFor(scheduleDay.Person),
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToGregorianDateTimeString().Remove(10),
				IsFullDayAbsence = scheduleDay.IsFullDayAbsence(),
				ShiftCategory = getShiftCategoryDescription(scheduleDay),
				Timezone = new TimeZoneViewModel
				{
					IanaId = _ianaTimeZoneProvider.WindowsToIana(scheduleDay.Person.PermissionInformation.DefaultTimeZone().Id),
					DisplayName = scheduleDay.Person.PermissionInformation.DefaultTimeZone().DisplayName
				},
				MultiplicatorDefinitionSetIds =
				(person.Period(date) != null &&
				 person.Period(date).PersonContract.Contract.MultiplicatorDefinitionSetCollection.Count > 0)
					? person.Period(date)
						.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Select(s => s.Id.GetValueOrDefault())
						.ToList()
					: null
			};
			var personAssignment = scheduleDay.PersonAssignment();

			var overtimeActivities = personAssignment != null
				? personAssignment.OvertimeActivities().ToArray()
				: null;

			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.ContractDayOff)
			{
				var dayOff = personAssignment != null ? personAssignment.DayOff() : null;
				var dayOffStart = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime;
				var dayOffEnd = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime;

				scheduleVm.DayOff = new GroupScheduleDayOffViewModel
				{
					DayOffName = dayOff != null ? dayOff.Description.Name : "",
					Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToGregorianDateTimeString().Replace("T", " ").Remove(16),
					End = TimeZoneInfo.ConvertTimeFromUtc(dayOffEnd, userTimeZone).ToGregorianDateTimeString().Replace("T", " ").Remove(16),
					Minutes = (int)dayOffEnd.Subtract(dayOffStart).TotalMinutes
				};
			}

			var visualLayerCollection = _projectionProvider.Projection(scheduleDay);

			if (visualLayerCollection != null && visualLayerCollection.HasLayers)
			{
				scheduleVm.WorkTimeMinutes = visualLayerCollection.WorkTime().TotalMinutes;
				scheduleVm.ContractTimeMinutes = visualLayerCollection.ContractTime().TotalMinutes;

				foreach (var layer in visualLayerCollection)
				{
					var isPayloadAbsence = layer.Payload is IAbsence;
					var isMainShiftLayer = layer.Payload is IActivity;
					var isAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var description = isPayloadAbsence
						? (isAbsenceConfidential && !canViewConfidential
							? ConfidentialPayloadValues.Description
							: ((IAbsence)layer.Payload).Description)
						: layer.Payload.ConfidentialDescription(person);
					var matchedPersonalLayers = _projectionHelper.GetMatchedPersonalShiftLayers(scheduleDay, layer);
					if (_projectionHelper.GetMatchedShiftLayerIds(scheduleDay, layer).Count > 1
						&& matchedPersonalLayers.Count > 0)
					{
						projections.AddRange(_projectionSplitter.SplitMergedPersonalLayers(scheduleDay, layer, matchedPersonalLayers.ToArray(), userTimeZone));
					}
					else
					{
						var isOvertime = overtimeActivities != null
										 && overtimeActivities.Any(overtime => (layer.DefinitionSet != null && layer.Period.Intersect(overtime.Period)));

						var matchedShiftLayers = isMainShiftLayer ? _projectionHelper.GetMatchedShiftLayers(scheduleDay, layer, isOvertime) : null;

						var projection = new GroupScheduleProjectionViewModel
						{
							ParentPersonAbsences = isPayloadAbsence ? _projectionHelper.GetMatchedAbsenceLayers(scheduleDay, layer).ToArray() : null,
							ShiftLayerIds = matchedShiftLayers?.Select(sl => sl.Id.GetValueOrDefault()).ToArray(),
							TopShiftLayerId = _projectionHelper.GetTopShiftLayerId(matchedShiftLayers, layer),
							ActivityId = layer.Payload.Id.GetValueOrDefault(),
							Description = description.Name,
							Color = isPayloadAbsence
													? (isAbsenceConfidential && !canViewConfidential
														? ConfidentialPayloadValues.DisplayColorHex
														: ((IAbsence)layer.Payload).DisplayColor.ToHtml())
													: layer.Payload.ConfidentialDisplayColor(scheduleDay.Person).ToHtml(),
							Start = startDateTimeInUserTimeZone.ToGregorianDateTimeString().Replace("T", " ").Remove(16),
							End = startDateTimeInUserTimeZone.Add(layer.Period.ElapsedTime()).ToGregorianDateTimeString().Replace("T", " ").Remove(16),
							Minutes = (int)layer.Period.ElapsedTime().TotalMinutes,
							IsOvertime = isOvertime
						};

						projections.Add(projection);
					}
				}
			}

			resetTopLayerIdIfItWasSplited(projections);

			scheduleVm.Projection = projections;
			return scheduleVm;
		}


		public AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential)
		{
			var ret = new AgentInTeamScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				Name = _personNameProvider.BuildNameFromSetting(person.Name),
				IsFullDayAbsence = scheduleDay.IsFullDayAbsence()
			};

			if (scheduleDay == null)
			{
				ret.IsNotScheduled = true;
				return ret;
			}

			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPart();

			if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
			{
				ret.IsDayOff = true;
				var dayOff = scheduleDay.PersonAssignment() != null ? scheduleDay.PersonAssignment().DayOff() : null;
				ret.DayOffName = dayOff != null ? dayOff.Description.Name : "";
			}
			if (!projection.Any() && !ret.IsDayOff)
			{
				ret.IsNotScheduled = true;
			}

			ret.ShiftCategory = getShiftCategoryDescriptionIncludingDayOff(scheduleDay);

			var projectionPeriod = projection.Period();
			if (projectionPeriod != null)
			{
				ret.StartTimeUtc = projectionPeriod.Value.StartDateTime;
			}

			var layers = new List<TeamScheduleLayerViewModel>();
			if (projection.HasLayers)
			{
				foreach (var layer in projection)
				{
					var isPayloadAbsence = layer.Payload is IAbsence;
					var isOvertime = person.Id == _loggedOnUser.CurrentUser().Id &&
									 (layer.DefinitionSet != null && layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime);
					var isAbsenceConfidential = isPayloadAbsence && ((IAbsence)layer.Payload).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);

					var description = isPayloadAbsence
						? (isAbsenceConfidential && !isPermittedToViewConfidential
							? ConfidentialPayloadValues.Description
							: ((IAbsence)layer.Payload).Description)
						: layer.Payload.ConfidentialDescription(person);
					var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
						startDateTimeInUserTimeZone.ToShortTimeString(), endDateTimeInUserTimeZone.ToShortTimeString());

					layers.Add(new TeamScheduleLayerViewModel
					{
						Meeting = mapMeeting(layer.Payload as IMeetingPayload),
						TitleHeader = description.Name,
						Color = isPayloadAbsence
							? (isAbsenceConfidential && !isPermittedToViewConfidential
								? ConfidentialPayloadValues.DisplayColorHex
								: (layer.Payload as IAbsence).DisplayColor.ToCSV())
							: layer.Payload.ConfidentialDisplayColor(scheduleDay.Person).ToCSV(),
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
			ret.Total = layers.Count;

			var visualLayers = _projectionProvider.Projection(scheduleDay);
			ret.ContractTimeInMinute = visualLayers.ContractTime().TotalMinutes;
			return ret;
		}

		public bool IsOvertimeOnDayOff(IScheduleDay scheduleDay)
		{
			if (scheduleDay == null)
			{
				return false;
			}

			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.DayOff)
			{
				return projection.HasLayers && projection.All(layer => layer.DefinitionSet != null
					&& layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime);
			}
			return false;
		}

		private ShiftCategoryViewModel getShiftCategoryDescription(IScheduleDay scheduleDay)
		{
			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.MainShift)
			{
				var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
				return new ShiftCategoryViewModel
				{
					Name = shiftCategory.Description.Name,
					ShortName = shiftCategory.Description.ShortName,
					DisplayColor = shiftCategory.DisplayColor.ToHtml()
				};
			}
			return null;
		}

		private ShiftCategoryViewModel getShiftCategoryDescriptionIncludingDayOff(IScheduleDay scheduleDay)
		{
			var significantPart = scheduleDay.SignificantPart();
			switch (significantPart)
			{
				case SchedulePartView.MainShift:
					var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
					return new ShiftCategoryViewModel
					{
						Name = shiftCategory.Description.Name,
						ShortName = shiftCategory.Description.ShortName,
						DisplayColor = shiftCategory.DisplayColor.ToHtml()
					};
				case SchedulePartView.DayOff:
					var dayOffInfo = scheduleDay.PersonAssignment().DayOff();
					return new ShiftCategoryViewModel
					{
						Name = dayOffInfo.Description.Name,
						ShortName = dayOffInfo.Description.ShortName,
						DisplayColor = dayOffInfo.DisplayColor.ToHtml()
					};
			}

			return null;
		}

		private static bool isSchedulePublished(DateOnly date, IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
				   workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}

		private static MeetingViewModel mapMeeting(IMeetingPayload meetingPayload)
		{
			if (meetingPayload == null)
				return null;
			var formatter = new NoFormatting();
			return new MeetingViewModel
			{
				Location = meetingPayload.Meeting.GetLocation(formatter),
				Title = meetingPayload.Meeting.GetSubject(formatter),
				Description = meetingPayload.Meeting.GetDescription(formatter)
			};
		}

		private void resetTopLayerIdIfItWasSplited(IList<GroupScheduleProjectionViewModel> projections)
		{

			foreach (var projection in projections)
			{
				if (projection.TopShiftLayerId.HasValue
					&& projections.Where(p => !p.ShiftLayerIds.IsNullOrEmpty() && p.ShiftLayerIds.Contains(projection.TopShiftLayerId.Value)).Count() > 1)
				{
					projection.TopShiftLayerId = null;
				}
			}
		}
	}
}