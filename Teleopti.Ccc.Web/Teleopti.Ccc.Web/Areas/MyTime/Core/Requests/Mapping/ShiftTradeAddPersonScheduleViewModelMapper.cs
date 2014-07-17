using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddPersonScheduleViewModelMapper : IShiftTradeAddPersonScheduleViewModelMapper
	{
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;

		public ShiftTradeAddPersonScheduleViewModelMapper(IShiftTradeAddScheduleLayerViewModelMapper layerMapper)
		{
			_layerMapper = layerMapper;
		}

		public ShiftTradeAddPersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel)
		{
			if (scheduleReadModel == null || scheduleReadModel.Start == null)
				return null;

			var shiftReadModel = JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model);
			ShiftTradeAddPersonScheduleViewModel ret = null;
			if ((shiftReadModel.Shift != null) && (shiftReadModel.Shift.Projection.Count > 0))
			{
				ret = new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				StartTimeUtc = scheduleReadModel.Start.Value,
				Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}", shiftReadModel.FirstName, shiftReadModel.LastName),
				ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection),
				MinStart = scheduleReadModel.MinStart,
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
					Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}", shiftReadModel.FirstName, shiftReadModel.LastName),
					ScheduleLayers = _layerMapper.Map(dayOffProjection),
					MinStart = scheduleReadModel.MinStart,
					IsDayOff = true
				};
			}
			return ret;
		}

		public IList<ShiftTradeAddPersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(Map).Where(s => s != null).ToList();
		}
	}
}