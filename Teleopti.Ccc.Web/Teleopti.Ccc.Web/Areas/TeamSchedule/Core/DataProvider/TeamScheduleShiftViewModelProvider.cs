using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleShiftViewModelProvider
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IScheduleProjectionHelper _projectionHelper;
		private readonly IProjectionSplitter _projectionSplitter;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IUserTimeZone _userTimeZone;

		public TeamScheduleShiftViewModelProvider(IProjectionProvider projectionProvider,
			IScheduleProjectionHelper projectionHelper,
			IProjectionSplitter projectionSplitter,
			IIanaTimeZoneProvider ianaTimeZoneProvider,
			ICommonAgentNameProvider commonAgentNameProvider,
			IUserTimeZone userTimeZone)
		{
			_projectionProvider = projectionProvider;
			_projectionHelper = projectionHelper;
			_projectionSplitter = projectionSplitter;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
			_userTimeZone = userTimeZone;
		}

		public GroupScheduleShiftViewModel MakeViewModel(
			IPerson person,
			DateOnly date,
			IScheduleDay scheduleDay,
			IScheduleDay previousScheduleDay,
			ICommonNameDescriptionSetting commonNameDescriptionSetting,
			bool canViewConfidential,
			bool canViewUnpublished,
			bool needToLoadNoteAndUnderlyingSummary = false)
		{
			var timezone = person.PermissionInformation.DefaultTimeZone();
			GroupScheduleShiftViewModel vm = null;

			if (scheduleDay.IsFullyPublished || canViewUnpublished)
			{
				vm = Projection(scheduleDay, commonNameDescriptionSetting, canViewConfidential);
				if (needToLoadNoteAndUnderlyingSummary)
				{
					vm.UnderlyingScheduleSummary = getUnderlyingScheduleSummary(timezone, scheduleDay, previousScheduleDay, canViewConfidential);
				}
			}
			else
			{
				vm = new GroupScheduleShiftViewModel
				{
					PersonId = person.Id.GetValueOrDefault().ToString(),
					Name = commonNameDescriptionSetting.BuildFor(person),
					Date = date.Date.ToServiceDateFormat(),
					Projection = new List<GroupScheduleProjectionViewModel>(),
					MultiplicatorDefinitionSetIds = person.Period(date)?
												.PersonContract?
												.Contract?
												.MultiplicatorDefinitionSetCollection?.Select(s => s.Id.GetValueOrDefault()).ToList(),
					Timezone = new TimeZoneViewModel
					{
						IanaId = _ianaTimeZoneProvider.WindowsToIana(timezone.Id),
						DisplayName = timezone.DisplayName
					}
				};
			}

			if (needToLoadNoteAndUnderlyingSummary)
			{
				var note = scheduleDay.NoteCollection().FirstOrDefault();
				vm.InternalNotes = note?.GetScheduleNote(new NormalizeText()) ?? string.Empty;

				var publicNote = scheduleDay.PublicNoteCollection().FirstOrDefault();
				vm.PublicNotes = publicNote?.GetScheduleNote(new NormalizeText()) ?? string.Empty;
			}

			var pa = scheduleDay.PersonAssignment();
			vm.IsProtected = pa?.Person.PersonWriteProtection.IsWriteProtected(date) ?? false;
			return vm;
		}

		public PersonWeekScheduleViewModel MakeWeekViewModel(
				IPerson person,
				IList<DateOnly> weekDays,
				IScheduleRange scheduleRange,
				IDictionary<DateOnly, HashSet<Guid>> peopleCanSeeSchedulesFor,
				IDictionary<DateOnly, HashSet<Guid>> peopleCanSeeUnpublishedSchedulesFor,
				IDictionary<DateOnly, HashSet<Guid>> viewableConfidentialAbsenceAgents,
				ICommonNameDescriptionSetting nameDescriptionSetting)
		{
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var timezoneVm = new TimeZoneViewModel
			{
				IanaId = _ianaTimeZoneProvider.WindowsToIana(timezone.Id),
				DisplayName = timezone.DisplayName
			};
			var daySchedules = weekDays
				.Select(date =>
				{
					var personId = person.Id.GetValueOrDefault();
					var isTerminated = person.IsTerminated(date);

					var dayScheduleViewModel = new PersonDayScheduleSummayViewModel
					{
						IsTerminated = isTerminated,
						Date = date,
						DayOfWeek = (int)date.DayOfWeek
					};

					if (isTerminated || !peopleCanSeeSchedulesFor[date].Contains(personId)) return dayScheduleViewModel;

					var scheduleDay = scheduleRange.ScheduledDay(date);

					if (scheduleDay == null)
					{
						return dayScheduleViewModel;
					}

					var canSeeUnpublishedSchedules = peopleCanSeeUnpublishedSchedulesFor[date].Contains(personId);

					if (!scheduleDay.IsFullyPublished && !canSeeUnpublishedSchedules) return dayScheduleViewModel;
					
					var visualLayerCollection = _projectionProvider.Projection(scheduleDay);
					if (visualLayerCollection != null && visualLayerCollection.HasLayers)
					{
						dayScheduleViewModel.ContractTimeMinutes = visualLayerCollection.ContractTime().TotalMinutes;
					}

					var significantPart = scheduleDay.SignificantPartForDisplay();
					var personAssignment = scheduleDay.PersonAssignment();

					if (significantPart == SchedulePartView.DayOff)
					{
						dayScheduleViewModel.Title = personAssignment.DayOff().Description.Name;
						dayScheduleViewModel.IsDayOff = true;
					}
					else if (significantPart == SchedulePartView.MainShift)
					{
						dayScheduleViewModel.Timezone = timezoneVm;
						dayScheduleViewModel.DateTimeSpan = visualLayerCollection.Period();

						if (personAssignment.ShiftCategory != null)
						{
							dayScheduleViewModel.Title = personAssignment.ShiftCategory.Description.Name;
							dayScheduleViewModel.Color =
								$"rgb({personAssignment.ShiftCategory.DisplayColor.R},{personAssignment.ShiftCategory.DisplayColor.G},{personAssignment.ShiftCategory.DisplayColor.B})";
						}
					}
					else if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
					{
						var canViewConfidentialAbsence = viewableConfidentialAbsenceAgents[date].Contains(personId);
						var absenceCollection = scheduleDay.PersonAbsenceCollection();
						var absence = absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
							.ThenByDescending(a => absenceCollection.IndexOf(a)).First().Layer.Payload;

						dayScheduleViewModel.IsDayOff = significantPart == SchedulePartView.ContractDayOff;

						if (absence.Confidential && !canViewConfidentialAbsence)
						{
							dayScheduleViewModel.Title = ConfidentialPayloadValues.Description.Name;
							dayScheduleViewModel.Color = ConfidentialPayloadValues.DisplayColorHex;
						}
						else
						{
							dayScheduleViewModel.Title = absence.Description.Name;
							dayScheduleViewModel.Color = $"rgb({absence.DisplayColor.R},{absence.DisplayColor.G},{absence.DisplayColor.B})";
						}
					}
					return dayScheduleViewModel;
				}).ToList();

			return new PersonWeekScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				Name = nameDescriptionSetting.BuildFor(person),
				DaySchedules = daySchedules,
				ContractTimeMinutes = daySchedules.Sum(s => s.ContractTimeMinutes)
			};
		}

		private UnderlyingScheduleSummary getUnderlyingScheduleSummary(TimeZoneInfo timeZone, IScheduleDay scheduleDay, IScheduleDay previousScheduleDay, bool canViewConfidential)
		{
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
						Start = personAbsence.Period.StartDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
						End = personAbsence.Period.EndDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
						StartInUtc = personAbsence.Period.StartDateTime.ToServiceDateTimeFormat(),
						EndInUtc = personAbsence.Period.EndDateTime.ToServiceDateTimeFormat()
					})
					.ToArray() : null;
			}

			underlyingSummary.PersonalActivities = hasPersonalActivities ? pa.PersonalActivities()
				.Select(personalActivity => new Summary
				{
					Description = personalActivity.Payload.Description.Name,
					Start = personalActivity.Period.StartDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
					End = personalActivity.Period.EndDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
					StartInUtc = personalActivity.Period.StartDateTime.ToServiceDateTimeFormat(),
					EndInUtc = personalActivity.Period.EndDateTime.ToServiceDateTimeFormat()
				})
				.ToArray() : null;

			underlyingSummary.PersonMeetings = hasPersonMeetings ? scheduleDay.PersonMeetingCollection()
				.Select(personMeeting => new Summary
				{
					Description = personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()),
					Start = personMeeting.Period.StartDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
					End = personMeeting.Period.EndDateTimeLocal(timeZone).ToServiceDateTimeFormat(),
					StartInUtc = personMeeting.Period.StartDateTime.ToServiceDateTimeFormat(),
					EndInUtc = personMeeting.Period.EndDateTime.ToServiceDateTimeFormat()
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

		public GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, ICommonNameDescriptionSetting commonNameDescriptionSetting, bool canViewConfidential)
		{
			var projections = new List<GroupScheduleProjectionViewModel>();
			var userTimeZone = _userTimeZone.TimeZone();
			var person = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = person.Period(date);
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var scheduleVm = new GroupScheduleShiftViewModel
			{
				PersonId = person.Id.GetValueOrDefault().ToString(),
				Name = commonNameDescriptionSetting.BuildFor(person),
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToServiceDateFormat(),
				IsFullDayAbsence = scheduleDay.IsFullDayAbsence(),
				ShiftCategory = getShiftCategoryDescription(scheduleDay),
				Timezone = new TimeZoneViewModel
				{
					IanaId = _ianaTimeZoneProvider.WindowsToIana(timeZone.Id),
					DisplayName = timeZone.DisplayName
				},
				MultiplicatorDefinitionSetIds =
				(personPeriod != null &&
				 personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Count > 0)
					? personPeriod
						.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Select(s => s.Id.GetValueOrDefault())
						.ToList()
					: null
			};
			var personAssignment = scheduleDay.PersonAssignment();

			var overtimeActivities = personAssignment?.OvertimeActivities().ToArray();

			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.ContractDayOff)
			{
				var dayOff = personAssignment?.DayOff();
				var dayOffStart = scheduleDay.DateOnlyAsPeriod.Period().StartDateTime;
				var dayOffEnd = scheduleDay.DateOnlyAsPeriod.Period().EndDateTime;

				scheduleVm.DayOff = new GroupScheduleDayOffViewModel
				{
					DayOffName = dayOff != null ? dayOff.Description.Name : "",
					Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToServiceDateTimeFormat(),
					End = TimeZoneInfo.ConvertTimeFromUtc(dayOffEnd, userTimeZone).ToServiceDateTimeFormat(),
					StartInUtc = dayOffStart.ToServiceDateTimeFormat(),
					EndInUtc = dayOffEnd.ToServiceDateTimeFormat(),
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
					var isMeeting = layer.Payload is MeetingPayload;
					var isAbsenceConfidential = isPayloadAbsence && (layer.Payload as IAbsence).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);
					var description = isPayloadAbsence
						? (isAbsenceConfidential && !canViewConfidential
							? ConfidentialPayloadValues.Description
							: ((IAbsence)layer.Payload).Description)
						: layer.Payload.ConfidentialDescription_DONTUSE(person);

					var matchedPersonalLayers = _projectionHelper.GetMatchedPersonalShiftLayers(scheduleDay, layer);
					var needSplitPersonalLayers = _projectionHelper.GetMatchedShiftLayerIds(scheduleDay, layer).Count > 1
						&& matchedPersonalLayers.Any();

					if (needSplitPersonalLayers)
					{
						projections.AddRange(_projectionSplitter.SplitMergedPersonalLayers(scheduleDay, layer, matchedPersonalLayers.ToArray(), userTimeZone));

					}
					else
					{
						var isOvertime = overtimeActivities != null
										 && overtimeActivities.Any(overtime => (layer.DefinitionSet != null && layer.Period.Intersect(overtime.Period)));

						var matchedShiftLayers = isMainShiftLayer ? _projectionHelper.GetMatchedShiftLayers(scheduleDay, layer, isOvertime) : null;
						var reportLevelDetail = (layer.Payload as IActivity)?.ReportLevelDetail;

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
													: layer.Payload.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person).ToHtml(),
							Start = startDateTimeInUserTimeZone.ToServiceDateTimeFormat(),
							StartInUtc = layer.Period.StartDateTime.ToServiceDateTimeFormat(),
							End = endDateTimeInUserTimeZone.ToServiceDateTimeFormat(),
							EndInUtc = layer.Period.EndDateTime.ToServiceDateTimeFormat(),
							Minutes = (int)layer.Period.ElapsedTime().TotalMinutes,
							IsOvertime = isOvertime,
							FloatOnTop = reportLevelDetail == ReportLevelDetail.Lunch
										|| reportLevelDetail == ReportLevelDetail.ShortBreak
										|| isOvertime
										|| matchedPersonalLayers.Any()
										|| isPayloadAbsence
										|| isMeeting,
							IsPersonalActivity = matchedPersonalLayers.Any(),
							IsMeeting = isMeeting
						};

						projections.Add(projection);
					}
				}
			}

			resetTopLayerIdIfItWasSplited(projections);

			scheduleVm.Projection = projections;
			return scheduleVm;
		}

		public AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson currentUser, IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential)
		{
			var ret = new AgentInTeamScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				IsFullDayAbsence = scheduleDay.IsFullDayAbsence()
			};

			if (scheduleDay == null)
			{
				ret.IsNotScheduled = true;
				return ret;
			}

			var userTimeZone = _userTimeZone.TimeZone();
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
					var isOvertime = layer.DefinitionSet != null && layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime;
					var isAbsenceConfidential = isPayloadAbsence && ((IAbsence)layer.Payload).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);

					var description = isPayloadAbsence
						? (isAbsenceConfidential && !isPermittedToViewConfidential
							? ConfidentialPayloadValues.Description
							: ((IAbsence)layer.Payload).Description)
						: layer.Payload.ConfidentialDescription_DONTUSE(person);
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
							: layer.Payload.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person).ToCSV(),
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
					&& projections.Count(p => !p.ShiftLayerIds.IsNullOrEmpty() && p.ShiftLayerIds.Contains(projection.TopShiftLayerId.Value)) > 1)
				{
					projection.TopShiftLayerId = null;
				}
			}
		}

	}
}