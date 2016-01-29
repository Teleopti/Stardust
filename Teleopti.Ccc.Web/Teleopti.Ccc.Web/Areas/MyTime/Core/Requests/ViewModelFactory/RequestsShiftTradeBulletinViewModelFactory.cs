using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradeBulletinViewModelFactory : IRequestsShiftTradeBulletinViewModelFactory
	{

		private readonly IShiftTradeScheduleViewModelMapper _shiftTradeScheduleViewModelMapper;
		private readonly IShiftTradePersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;

		public RequestsShiftTradeBulletinViewModelFactory(IShiftTradeScheduleViewModelMapper shiftTradeScheduleViewModelMapper, IShiftTradePersonScheduleViewModelMapper personScheduleViewModelMapper, IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper)
		{
			_shiftTradeScheduleViewModelMapper = shiftTradeScheduleViewModelMapper;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
		}


		public ShiftTradeScheduleViewModel CreateShiftTradeBulletinViewModel(ShiftTradeScheduleViewModelData data)
		{
			return _shiftTradeScheduleViewModelMapper.MapForBulletin(data);
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeBulletinViewModelFromRawData(ShiftTradeScheduleViewModelData data)
		{
			var ret = new ShiftTradeScheduleViewModel();
			ret.MySchedule = _personScheduleViewModelMapper.MakeMyScheduleViewModel(data);
			int pageCount;
			ret.PossibleTradeSchedules = _personScheduleViewModelMapper.MakePossibleShiftTradeAddPersonScheduleViewModels(data, out pageCount);
			ret.PageCount = pageCount;
			ret.TimeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(ret.MySchedule, ret.PossibleTradeSchedules,
				data.ShiftTradeDate);
			return ret;
		}
	}
}