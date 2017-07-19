using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class TeamScheduleProjectionProvider : ITeamScheduleProjectionProvider
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IToggleManager _toggleManager;
		private readonly IScheduleProjectionHelper _projectionHelper;
		private readonly IProjectionSplitter _projectionSplitter;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IPersonNameProvider _personNameProvider;

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser,
			IToggleManager toggleManager, IScheduleProjectionHelper projectionHelper, IProjectionSplitter projectionSplitter,
			IIanaTimeZoneProvider ianaTimeZoneProvider, IPersonNameProvider personNameProvider)
		{
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
			_toggleManager = toggleManager;
			_projectionHelper = projectionHelper;
			_projectionSplitter = projectionSplitter;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_personNameProvider = personNameProvider;
		}

		public GroupScheduleShiftViewModel MakeViewModel(IPerson person, DateOnly date, IScheduleDay scheduleDay,
			bool canViewConfidential, bool canViewUnpublished, bool includeNote, ICommonNameDescriptionSetting agentNameSetting)
		{
			var personPeriod = person.Period(date);

			var vm = new GroupScheduleShiftViewModel
			{
				PersonId = person.Id.GetValueOrDefault().ToString(),
				Name = agentNameSetting.BuildCommonNameDescription(person),
				Date = date.Date.ToGregorianDateTimeString().Remove(10),
				Projection = new List<GroupScheduleProjectionViewModel>(),
				MultiplicatorDefinitionSetIds =
				(personPeriod != null && personPeriod.PersonContract != null &&
				 personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Count > 0)
					? personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Select(s => s.Id.GetValueOrDefault())
						.ToList()
					: null
			};

			if (scheduleDay != null)
			{
				var isPublished = isSchedulePublished(scheduleDay.DateOnlyAsPeriod.DateOnly,person);
				if (isPublished || canViewUnpublished)
				{
					vm = Projection(scheduleDay, canViewConfidential,agentNameSetting);
				}

				if (includeNote)
				{
					var note = scheduleDay.NoteCollection().FirstOrDefault();
					vm.InternalNotes = note != null
						? note.GetScheduleNote(new NormalizeText())
						: string.Empty;
					if (_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_DisplayAndEditPublicNote_44783))
					{
						var publicNotes = scheduleDay.PublicNoteCollection().FirstOrDefault();
						vm.PublicNotes = publicNotes != null ? publicNotes.GetScheduleNote(new NormalizeText()) : String.Empty;
					}
				}
			}

			vm.Timezone = new TimeZoneViewModel
			{
				IanaId = _ianaTimeZoneProvider.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id),
				DisplayName = person.PermissionInformation.DefaultTimeZone().DisplayName
			};

			return vm;
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
				Name = agentNameSetting.BuildCommonNameDescription(scheduleDay.Person),
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToGregorianDateTimeString().Remove(10),
				IsFullDayAbsence = IsFullDayAbsence(scheduleDay),
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
					End = TimeZoneInfo.ConvertTimeFromUtc(dayOffEnd,userTimeZone).ToGregorianDateTimeString().Replace("T", " ").Remove(16),
					Minutes = (int) dayOffEnd.Subtract(dayOffStart).TotalMinutes
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
							: ((IAbsence) layer.Payload).Description)
						: layer.DisplayDescription();
					var matchedPersonalLayers = _projectionHelper.GetMatchedPersonalShiftLayers(scheduleDay, layer);
					if (_projectionHelper.GetMatchedShiftLayerIds(scheduleDay, layer).Count > 1
						&& matchedPersonalLayers.Count > 0)
					{
						projections.AddRange(_projectionSplitter.SplitMergedPersonalLayers(scheduleDay, layer, matchedPersonalLayers.ToArray(), userTimeZone));
					}
					else
					{
						var isOvertime = overtimeActivities != null
										 &&
										 overtimeActivities.Any(overtime => (layer.DefinitionSet != null && layer.Period.Intersect(overtime.Period)));
						projections.Add(new GroupScheduleProjectionViewModel
						{
							ParentPersonAbsences = isPayloadAbsence ? _projectionHelper.GetMatchedAbsenceLayers(scheduleDay, layer).ToArray() : null,
							ShiftLayerIds = isMainShiftLayer ? _projectionHelper.GetMatchedShiftLayerIds(scheduleDay, layer, isOvertime).ToArray() : null,
							Description = description.Name,
							Color = isPayloadAbsence
							? (isAbsenceConfidential && !canViewConfidential
								? ConfidentialPayloadValues.DisplayColorHex
								: ((IAbsence)layer.Payload).DisplayColor.ToHtml())
							: layer.DisplayColor().ToHtml(),
							Start = startDateTimeInUserTimeZone.ToGregorianDateTimeString().Replace("T", " ").Remove(16),
							End = startDateTimeInUserTimeZone.Add(layer.Period.ElapsedTime()).ToGregorianDateTimeString().Replace("T", " ").Remove(16),
							Minutes = (int)layer.Period.ElapsedTime().TotalMinutes,
							IsOvertime = isOvertime
						});
					}
				}
			}
			scheduleVm.Projection = projections;
			return scheduleVm;
		}

		public AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential)
		{
			var ret = new AgentInTeamScheduleViewModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				Name = _personNameProvider.BuildNameFromSetting(person.Name),
				IsFullDayAbsence = IsFullDayAbsence(scheduleDay)
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
					var isAbsenceConfidential = isPayloadAbsence && ((IAbsence) layer.Payload).Confidential;
					var startDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, userTimeZone);
					var endDateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, userTimeZone);

					var description = isPayloadAbsence
						? (isAbsenceConfidential && !isPermittedToViewConfidential
							? ConfidentialPayloadValues.Description
							: ((IAbsence) layer.Payload).Description)
						: layer.DisplayDescription();
					var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
						startDateTimeInUserTimeZone.ToShortTimeString(), endDateTimeInUserTimeZone.ToShortTimeString());

					layers.Add(new TeamScheduleLayerViewModel
					{
						TitleHeader = description.Name,
						Color = isPayloadAbsence
							? (isAbsenceConfidential && !isPermittedToViewConfidential
								? ConfidentialPayloadValues.DisplayColorHex
								: (layer.Payload as IAbsence).DisplayColor.ToHtml())
							: layer.DisplayColor().ToHtml(),
						Start = startDateTimeInUserTimeZone,
						End = endDateTimeInUserTimeZone,
						LengthInMinutes = (int) endDateTimeInUserTimeZone.Subtract(startDateTimeInUserTimeZone).TotalMinutes,
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

		public bool IsFullDayAbsence(IScheduleDay scheduleDay)
		{
			if (scheduleDay == null)
			{
				return false;
			}

			var projection = _projectionProvider.Projection(scheduleDay);
			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
			{
				return projection.HasLayers && projection.All(l => l.Payload is IAbsence);
			}

			return significantPart == SchedulePartView.FullDayAbsence;
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

		private static bool isSchedulePublished(DateOnly date,IPerson person)
		{
			var workflowControlSet = person.WorkflowControlSet;
			if(workflowControlSet == null)
				return false;
			return workflowControlSet.SchedulePublishedToDate.HasValue &&
				   workflowControlSet.SchedulePublishedToDate.Value >= date.Date;
		}
	}
}