using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class AgentScheduleViewModelReworkedMapper : IAgentScheduleViewModelReworkedMapper
	{
		private readonly ILayerViewModelReworkedMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IPermissionProvider _permissionProvider;

		public AgentScheduleViewModelReworkedMapper(ILayerViewModelReworkedMapper layerMapper, IPersonNameProvider personNameProvider, IPermissionProvider permissionProvider)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
			_permissionProvider = permissionProvider;
		}

		public AgentInTeamScheduleViewModel Map(PersonSchedule personSchedule)
		{
			if (personSchedule == null )
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
				return ret;

			var teamScheduleReadModel = personSchedule.Schedule.Model != null
				? JsonConvert.DeserializeObject<Model>(personSchedule.Schedule.Model)
				: null;


			if(teamScheduleReadModel != null &&(canSeeUnpublishedSchedule||isSchedulePublished))
			{
				if ((teamScheduleReadModel.Shift != null) && (teamScheduleReadModel.Shift.Projection.Count > 0))
				{
					ret.IsFullDayAbsence = teamScheduleReadModel.Shift.IsFullDayAbsence;
					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(teamScheduleReadModel.Shift.Projection);
				}
				ret.IsDayOff = teamScheduleReadModel.DayOff != null;
				ret.DayOffName = ret.IsDayOff ? teamScheduleReadModel.DayOff.Title : "";
				if (((teamScheduleReadModel.DayOff != null) && teamScheduleReadModel.Shift != null && !teamScheduleReadModel.Shift.IsFullDayAbsence))
				{
					var dayOffProjection = new List<SimpleLayer>();
					var sl = new SimpleLayer
					{
						Start = teamScheduleReadModel.DayOff.Start,
						End = teamScheduleReadModel.DayOff.End,
						Description = teamScheduleReadModel.DayOff.Title,
						Minutes = (int) (teamScheduleReadModel.DayOff.End - teamScheduleReadModel.DayOff.Start).TotalMinutes
					};

					dayOffProjection.Add(sl);

					if (personSchedule.Schedule.Start != null)
						ret.StartTimeUtc = personSchedule.Schedule.Start.Value;
					ret.MinStart = personSchedule.Schedule.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(dayOffProjection);
				}
			}
			return ret;
		}
		public IEnumerable<AgentInTeamScheduleViewModel> Map(IEnumerable<PersonSchedule> personSchedules)
		{
			return personSchedules.Select(Map).Where(s => s != null);
		}
	}
}