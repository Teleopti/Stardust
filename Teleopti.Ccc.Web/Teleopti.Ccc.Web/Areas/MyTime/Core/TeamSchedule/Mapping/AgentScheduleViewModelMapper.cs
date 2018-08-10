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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class AgentScheduleViewModelMapper : IAgentScheduleViewModelMapper
	{
		private readonly ILayerViewModelMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleProvider _scheduleProvider;

		public AgentScheduleViewModelMapper(ILayerViewModelMapper layerMapper,
			IPersonNameProvider personNameProvider,
			IPermissionProvider permissionProvider,
			IScheduleProvider scheduleProvider)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
			_permissionProvider = permissionProvider;
			_scheduleProvider = scheduleProvider;
		}

		public AgentInTeamScheduleViewModel Map(PersonSchedule personSchedule)
		{
			if (personSchedule == null)
				return null;
			var canSeeUnpublishedSchedule =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			var isSchedulePublished = _permissionProvider.IsPersonSchedulePublished(personSchedule.Date,
				personSchedule.Person, ScheduleVisibleReasons.Any);

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

			var teamScheduleReadModel = personSchedule.Schedule.Model != null
				? JsonConvert.DeserializeObject<Model>(personSchedule.Schedule.Model)
				: null;


			if (teamScheduleReadModel != null && (canSeeUnpublishedSchedule || isSchedulePublished))
			{
				if (teamScheduleReadModel.Shift != null && (teamScheduleReadModel.Shift.Projection.Count > 0))
				{
					ret.IsFullDayAbsence = teamScheduleReadModel.Shift.IsFullDayAbsence;
					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(teamScheduleReadModel.Shift.Projection);
				}
				ret.IsDayOff = teamScheduleReadModel.DayOff != null;
				ret.DayOffName = ret.IsDayOff ? teamScheduleReadModel.DayOff.Title : "";
				if (teamScheduleReadModel.DayOff != null && teamScheduleReadModel.Shift != null && !teamScheduleReadModel.Shift.IsFullDayAbsence)
				{
					var dayOffProjection = new List<SimpleLayer>();
					var sl = new SimpleLayer
					{
						Start = teamScheduleReadModel.DayOff.Start,
						End = teamScheduleReadModel.DayOff.End,
						Description = teamScheduleReadModel.DayOff.Title,
						Minutes = (int)(teamScheduleReadModel.DayOff.End - teamScheduleReadModel.DayOff.Start).TotalMinutes
					};

					dayOffProjection.Add(sl);

					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(dayOffProjection);
					ret.ShiftCategory = getDayOffShiftCategoryDescription(personSchedule);
				}
				if (!teamScheduleReadModel.Shift.Projection.Any() && !ret.IsDayOff)
				{
					ret.IsNotScheduled = true;
				}
			}
			return ret;
		}

		private ShiftCategoryViewModel getDayOffShiftCategoryDescription(PersonSchedule personSchedule)
		{
			var scheduleDay = _scheduleProvider.GetScheduleForPersons(personSchedule.Date, new[] { personSchedule.Person }).SingleOrDefault();
			if (scheduleDay == null) return null;

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