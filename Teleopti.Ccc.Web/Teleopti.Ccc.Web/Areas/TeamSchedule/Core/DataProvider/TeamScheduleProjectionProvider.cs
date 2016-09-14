using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IToggleManager _toggleManager;

		public TeamScheduleProjectionProvider(IProjectionProvider projectionProvider, ILoggedOnUser loggedOnUser, IPersonNameProvider personNameProvider, IToggleManager toggleManager)
		{
			_projectionProvider = projectionProvider;
			_loggedOnUser = loggedOnUser;
			_personNameProvider = personNameProvider;
			_toggleManager = toggleManager;
		}

		public GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential, ICommonNameDescriptionSetting agentNameSetting)
		{
			var projections = new List<GroupScheduleProjectionViewModel>();
			var userTimeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var scheduleVm = new GroupScheduleShiftViewModel
			{
				PersonId = scheduleDay.Person.Id.GetValueOrDefault().ToString(),
				Name = agentNameSetting.BuildCommonNameDescription(scheduleDay.Person),
				Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToFixedDateFormat(),
				IsFullDayAbsence = IsFullDayAbsence(scheduleDay),
				ShiftCategory = getShiftCategoryDescription(scheduleDay)
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
					Start = TimeZoneInfo.ConvertTimeFromUtc(dayOffStart, userTimeZone).ToFixedDateTimeFormat(),
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
					if (_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_MakePersonalActivityUnmerged_40252) && getMatchedShiftLayers(scheduleDay, layer).Count > 1 && getMatchedPersonalShiftLayers(scheduleDay, layer).Count > 0)
					{
						projections.AddRange(splitMergedPersonalLayers(scheduleDay, layer, userTimeZone));
					}
					else
					{
						projections.Add(new GroupScheduleProjectionViewModel
						{
							ParentPersonAbsences = isPayloadAbsence ? getMatchedAbsenceLayers(scheduleDay, layer).ToArray() : null,
							ShiftLayerIds = isMainShiftLayer ? getMatchedShiftLayers(scheduleDay, layer).ToArray() : null,
							Description = description.Name,
							Color = isPayloadAbsence
							? (isAbsenceConfidential && !canViewConfidential
								? ConfidentialPayloadValues.DisplayColorHex
								: ((IAbsence)layer.Payload).DisplayColor.ToHtml())
							: layer.DisplayColor().ToHtml(),
							Start = startDateTimeInUserTimeZone.ToFixedDateTimeFormat(),
							Minutes = (int)layer.Period.ElapsedTime().TotalMinutes,
							IsOvertime = overtimeActivities != null
									 && overtimeActivities.Any(overtime => overtime.Period.Contains(layer.Period))
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
		private IList<GroupScheduleProjectionViewModel> splitMergedPersonalLayers(IScheduleDay scheduleDay, IVisualLayer layer, TimeZoneInfo userTimeZone)
		{
			var splittedVisualLayers = new List<GroupScheduleProjectionViewModel>();
			var unMergedCollection = _projectionProvider.UnmergedProjection(scheduleDay);
			var splittedLayers = unMergedCollection.Where(l => layer.Period.Contains(l.Period)).ToList();

			splittedLayers.ForEach(l =>
			{
				splittedVisualLayers.Add(new GroupScheduleProjectionViewModel
				{
					ShiftLayerIds = new[] { getMatchedShiftLayers(scheduleDay, l).Last() },
					Description = layer.DisplayDescription().Name,
					Color = layer.DisplayColor().ToHtml(),
					Start = TimeZoneInfo.ConvertTimeFromUtc(l.Period.StartDateTime, userTimeZone).ToFixedDateTimeFormat(),
					Minutes = (int)l.Period.ElapsedTime().TotalMinutes
				});
			});
			return splittedVisualLayers;
		}

		private IList<IPersonalShiftLayer> getMatchedPersonalShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayers = new List<IPersonalShiftLayer>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				var isPersonalLayer = shiftLayer is IPersonalShiftLayer;
				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && (layer.Period.Intersect(shiftLayer.Period)) && isPersonalLayer)
				{
					matchedLayers.Add(shiftLayer as IPersonalShiftLayer);
				}
			}
			return matchedLayers;
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
		private IList<Guid> getMatchedShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayerIds = new List<Guid>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && layer.Period.Intersect(shiftLayer.Period))
				{
					matchedLayerIds.Add(shiftLayer.Id.GetValueOrDefault());
				}
			}
			return matchedLayerIds;
		}
		private IList<Guid> getMatchedAbsenceLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayerIds = new List<Guid>();
			var personAbsences = scheduleDay.PersonAbsenceCollection().ToList();

			foreach (var personAbs in personAbsences)
			{
				if (layer.Payload.Id.GetValueOrDefault() == personAbs.Layer.Payload.Id.GetValueOrDefault() && (layer.Period.Contains(personAbs.Period) || personAbs.Period.Contains(layer.Period)))
				{
					matchedLayerIds.Add(personAbs.Id.GetValueOrDefault());
				}
			}
			return matchedLayerIds;
		}
	}

	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential, ICommonNameDescriptionSetting agentNameSetting);
		AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);

		bool IsFullDayAbsence(IScheduleDay scheduleDay);
		bool IsOvertimeOnDayOff(IScheduleDay scheduleDay);
	}
}