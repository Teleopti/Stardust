﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddPersonScheduleViewModelMapper : IShiftTradeAddPersonScheduleViewModelMapper
	{
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;
		private readonly IPersonNameProvider _personNameProvider;

		public ShiftTradeAddPersonScheduleViewModelMapper(IShiftTradeAddScheduleLayerViewModelMapper layerMapper, IPersonNameProvider personNameProvider)
		{
			_layerMapper = layerMapper;
			_personNameProvider = personNameProvider;
		}

		public ShiftTradeAddPersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel, bool isMySchedule=false)
		{
			if (scheduleReadModel == null || scheduleReadModel.Start == null)
				return null;

			var shiftReadModel = JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model);
			ShiftTradeAddPersonScheduleViewModel ret = null;
			if ((shiftReadModel.Shift != null) && (shiftReadModel.Shift.Projection.Count > 0) && (isMySchedule || !shiftReadModel.Shift.IsFullDayAbsence))
			{
				ret = new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				StartTimeUtc = scheduleReadModel.Start.Value,
				Name = _personNameProvider.BuildNameFromSetting(shiftReadModel.FirstName, shiftReadModel.LastName),
				ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection, isMySchedule),
				MinStart = scheduleReadModel.MinStart,
				Total = scheduleReadModel.Total,
						IsDayOff = false
			};
		}

			if (shiftReadModel.DayOff != null)
			{
				var dayOffProjection = new List<SimpleLayer>();
				var sl = new SimpleLayer
					{
						Start = shiftReadModel.DayOff.Start,
						End = shiftReadModel.DayOff.End,
						Description = shiftReadModel.DayOff.Title,
						Minutes = (int)(shiftReadModel.DayOff.End - shiftReadModel.DayOff.Start).TotalMinutes
					};

				dayOffProjection.Add(sl);
				ret = new ShiftTradeAddPersonScheduleViewModel
				{
					PersonId = scheduleReadModel.PersonId,
					StartTimeUtc = scheduleReadModel.Start.Value,
					Name = _personNameProvider.BuildNameFromSetting(shiftReadModel.FirstName, shiftReadModel.LastName),
					ScheduleLayers = _layerMapper.Map(dayOffProjection),
					MinStart = scheduleReadModel.MinStart,
					Total = scheduleReadModel.Total,
					IsDayOff = true
				};
			}
			return ret;
		}

		public IList<ShiftTradeAddPersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(scheduleReadModel => Map(scheduleReadModel, false)).Where(s => s != null).ToList();
		}
	}
}