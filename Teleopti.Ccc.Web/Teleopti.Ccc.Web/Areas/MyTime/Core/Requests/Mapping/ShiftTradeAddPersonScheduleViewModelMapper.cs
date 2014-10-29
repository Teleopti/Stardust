﻿using System;
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
			var ret = new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				Total = scheduleReadModel.Total,
				IsDayOff = false
			};

			if (shiftReadModel == null)
			{
				var person = _personRepository.FindPeople(new List<Guid> {scheduleReadModel.PersonId}).First();
				ret.Name = _personNameProvider.BuildNameFromSetting(person.Name.FirstName, person.Name.LastName);
			}
			else
			{
				ret.Name = _personNameProvider.BuildNameFromSetting(shiftReadModel.FirstName, shiftReadModel.LastName);
				if ((shiftReadModel.Shift != null) && (shiftReadModel.Shift.Projection.Count > 0) &&
				(isMySchedule || !shiftReadModel.Shift.IsFullDayAbsence))
				{
					ret.StartTimeUtc = scheduleReadModel.Start.Value;
					ret.MinStart = scheduleReadModel.MinStart;
					ret.ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection, isMySchedule);
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