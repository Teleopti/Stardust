using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class AgentScheduleViewModelMapper : IAgentScheduleViewModelMapper
	{
		private readonly ILayerViewModelMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IProjectionProvider _projectionProvider;

		public AgentScheduleViewModelMapper(ILayerViewModelMapper layerMapper,
			IPersonNameProvider personNameProvider,
			IPermissionProvider permissionProvider,
			IScheduleProvider scheduleProvider,
			IProjectionProvider projectionProvider)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
		}

		public AgentInTeamScheduleViewModel Map(PersonSchedule personSchedule)
		{
			if (personSchedule == null)
				return null;

			var ret = new AgentInTeamScheduleViewModel
			{
				PersonId = personSchedule.Schedule.PersonId,
				Total = personSchedule.Schedule.Total,
				IsDayOff = false,
				Name = _personNameProvider.BuildNameFromSetting(personSchedule.Schedule.FirstName, personSchedule.Schedule.LastName),
			};

			if (personSchedule.Schedule == null)
			{
				ret.IsNotScheduled = true;
				return ret;
			}

			var canSeeUnpublishedSchedule =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var isSchedulePublished = _permissionProvider.IsPersonSchedulePublished(personSchedule.Date,
				personSchedule.Person, ScheduleVisibleReasons.Any);

			var teamScheduleReadModel = personSchedule.Schedule.Model != null
				? JsonConvert.DeserializeObject<Model>(personSchedule.Schedule.Model)
				: null;

			if (teamScheduleReadModel != null && (canSeeUnpublishedSchedule || isSchedulePublished))
			{
				if (teamScheduleReadModel.Shift != null && teamScheduleReadModel.Shift.Projection.Count > 0)
				{
					ret.IsFullDayAbsence = teamScheduleReadModel.Shift.IsFullDayAbsence;
					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(teamScheduleReadModel.Shift.Projection);
				}

				mapDayOff(personSchedule, ret, teamScheduleReadModel);

				if (!teamScheduleReadModel.Shift.Projection.Any() && !ret.IsDayOff)
				{
					ret.IsNotScheduled = true;
				}
			}
			return ret;
		}

		private void mapDayOff(PersonSchedule personSchedule, AgentInTeamScheduleViewModel ret, Model teamScheduleReadModel)
		{
			ret.IsDayOff = teamScheduleReadModel.DayOff != null;
			ret.DayOffName = ret.IsDayOff ? teamScheduleReadModel.DayOff.Title : "";
			if (teamScheduleReadModel.DayOff != null && teamScheduleReadModel.Shift != null &&
				!teamScheduleReadModel.Shift.IsFullDayAbsence)
			{
				var dayOffProjection = new List<SimpleLayer>();

				var scheduleDay = _scheduleProvider.GetScheduleForPersons(personSchedule.Date, new[] {personSchedule.Person})
					.SingleOrDefault();
				if (scheduleDay != null)
				{
					var projection = _projectionProvider.Projection(scheduleDay);
					if (projection.HasLayers)
					{
						foreach (var layer in projection)
						{
							var isPayloadAbsence = layer.Payload is IAbsence;
							var isAbsenceConfidential = isPayloadAbsence && ((IAbsence) layer.Payload).Confidential;
							var description = isPayloadAbsence
								? ((IAbsence) layer.Payload).Description
								: layer.Payload.ConfidentialDescription_DONTUSE(personSchedule.Person);
							var sl = new SimpleLayer
							{
								Description = description.Name,
								Color = isPayloadAbsence
									? ((IAbsence) layer.Payload).DisplayColor.ToCSV()
									: layer.Payload.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person).ToCSV(),
								Start = layer.Period.StartDateTime,
								End = layer.Period.EndDateTime,
								IsAbsenceConfidential = isAbsenceConfidential
							};

							dayOffProjection.Add(sl);
						}
					}
					else
					{
						var sl = new SimpleLayer
						{
							Start = teamScheduleReadModel.DayOff.Start,
							End = teamScheduleReadModel.DayOff.End,
							Description = teamScheduleReadModel.DayOff.Title,
							Minutes = (int) (teamScheduleReadModel.DayOff.End - teamScheduleReadModel.DayOff.Start).TotalMinutes
						};

						dayOffProjection.Add(sl);
					}

					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(dayOffProjection);
					ret.ShiftCategory = getDayOffShiftCategoryDescription(scheduleDay);
				}
			}
		}

		private ShiftCategoryViewModel getDayOffShiftCategoryDescription(IScheduleDay scheduleDay)
		{
			var dayOffInfo = scheduleDay.PersonAssignment().DayOff();
			return new ShiftCategoryViewModel
			{
				Name = dayOffInfo.Description.Name,
				ShortName = dayOffInfo.Description.ShortName,
				DisplayColor = dayOffInfo.DisplayColor.ToHtml()
			};
		}

		public IEnumerable<AgentInTeamScheduleViewModel> Map(IEnumerable<PersonSchedule> personSchedules)
		{
			return personSchedules.Select(Map).Where(s => s != null);
		}
	}
}