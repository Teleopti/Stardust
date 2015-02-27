using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class AgentScheduleViewModelReworkedMapper : IAgentScheduleViewModelReworkedMapper
	{
		private readonly ILayerViewModelReworkedMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;

		public AgentScheduleViewModelReworkedMapper(ILayerViewModelReworkedMapper layerMapper, IPersonNameProvider personNameProvider)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
		}

		public AgentScheduleViewModelReworked Map(IPersonScheduleDayReadModel scheduleReadModel)
		{
			if (scheduleReadModel == null)
				return null;

			var teamScheduleReadModel = scheduleReadModel.Model != null
				? JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model)
				: null;
			
			var ret = new AgentScheduleViewModelReworked
			{
				PersonId = scheduleReadModel.PersonId,
				Total = scheduleReadModel.Total,
				IsDayOff = false,
				Name = _personNameProvider.BuildNameFromSetting(scheduleReadModel.FirstName, scheduleReadModel.LastName),
			};
			if(teamScheduleReadModel != null)
			{
				//while having shift and layers, if it is my schedule it'll get shown, or if others agent's schedule is not full day absence, 
				//that should be available for trade. (full day absence is not allowed to  be used for trade)
				if ((teamScheduleReadModel.Shift != null) && (teamScheduleReadModel.Shift.Projection.Count > 0))
				{
					if (scheduleReadModel.Start != null)
						ret.StartTimeUtc = scheduleReadModel.Start.Value;
					ret.MinStart = scheduleReadModel.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(teamScheduleReadModel.Shift.Projection);
				}

				//for DayOff schedule, logic is same except using different mapping method.
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

					if (scheduleReadModel.Start != null)
						ret.StartTimeUtc = scheduleReadModel.Start.Value;
					ret.MinStart = scheduleReadModel.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(dayOffProjection);
					ret.IsDayOff = true;
				}
			}
			return ret;
		}
		public IEnumerable<AgentScheduleViewModelReworked> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(Map).Where(s => s != null);
		}
	}
}