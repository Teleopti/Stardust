using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddPersonScheduleViewModelMapper : IShiftTradeAddPersonScheduleViewModelMapper
	{
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IPersonRepository _personRepository;

		public ShiftTradeAddPersonScheduleViewModelMapper(IShiftTradeAddScheduleLayerViewModelMapper layerMapper, IPersonNameProvider personNameProvider, IPersonRepository personRepository)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
			_personRepository = personRepository;
		}

		public ShiftTradeAddPersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel, bool isMySchedule=false)
		{
			if (scheduleReadModel == null)
				return null;

			var shiftReadModel = scheduleReadModel.Model != null
				? JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model)
				: null;
			//Full day absence for other agent should be excluded from possible tradable schedules.
			if (!isMySchedule && shiftReadModel != null && shiftReadModel.Shift != null && shiftReadModel.Shift.IsFullDayAbsence)
			{
				return null;
			}
			var ret = new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				Name =  _personNameProvider.BuildNameFromSetting(scheduleReadModel.FirstName, scheduleReadModel.LastName),
				Total = scheduleReadModel.Total,
				IsDayOff = false,
				ShiftExchangeOfferId = scheduleReadModel.ShiftExchangeOffer
			};
												
			if (shiftReadModel != null)
			{
				//while having shift and layers, if it is my schedule it'll get shown, or if others agent's schedule is not full day absence, 
				//that should be available for trade. (full day absence is not allowed to  be used for trade)

				if ((shiftReadModel.Shift != null) && (shiftReadModel.Shift.Projection.Count > 0) &&
				    (isMySchedule || !shiftReadModel.Shift.IsFullDayAbsence))
				{
					ret.IsFullDayAbsence = shiftReadModel.Shift.IsFullDayAbsence;
					if (scheduleReadModel.Start != null)
						ret.StartTimeUtc = scheduleReadModel.Start.Value;
					ret.MinStart = scheduleReadModel.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection, isMySchedule);
				}

				if (shiftReadModel.DayOff != null && isMySchedule)
				{
					ret.IsDayOff = true;
					ret.DayOffName = shiftReadModel.DayOff.Title;
					if (scheduleReadModel.Start != null)
						ret.StartTimeUtc = scheduleReadModel.Start.Value;
				}
				else if (((shiftReadModel.DayOff != null) && shiftReadModel.Shift != null && !shiftReadModel.Shift.IsFullDayAbsence))
				{
					var dayOffProjection = new List<SimpleLayer>();
					var sl = new SimpleLayer
					{
						Start = shiftReadModel.DayOff.Start,
						End = shiftReadModel.DayOff.End,
						Description = shiftReadModel.DayOff.Title,
						Minutes = (int) (shiftReadModel.DayOff.End - shiftReadModel.DayOff.Start).TotalMinutes
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

		public IList<ShiftTradeAddPersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(scheduleReadModel => Map(scheduleReadModel, false)).Where(s => s != null).ToList();
		}
	}
}