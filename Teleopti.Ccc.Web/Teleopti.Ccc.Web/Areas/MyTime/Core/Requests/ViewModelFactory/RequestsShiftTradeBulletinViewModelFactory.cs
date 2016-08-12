using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradeBulletinViewModelFactory : IRequestsShiftTradeBulletinViewModelFactory
	{
		private readonly IShiftTradeScheduleViewModelMapper _shiftTradeScheduleViewModelMapper;
		private readonly IShiftTradePersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;

		public RequestsShiftTradeBulletinViewModelFactory(
			IShiftTradeScheduleViewModelMapper shiftTradeScheduleViewModelMapper,
			IShiftTradePersonScheduleViewModelMapper personScheduleViewModelMapper,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper)
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
			int pageCount;
			var mySchedule = _personScheduleViewModelMapper.MakeMyScheduleViewModel(data);
			var ret = new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules =
					_personScheduleViewModelMapper.MakePossibleShiftTradeAddPersonScheduleViewModels(data, mySchedule, out pageCount),
				PageCount = pageCount
			};
			ret.TimeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(ret.MySchedule, ret.PossibleTradeSchedules,
				data.ShiftTradeDate);
			return ret;
		}
	}
}