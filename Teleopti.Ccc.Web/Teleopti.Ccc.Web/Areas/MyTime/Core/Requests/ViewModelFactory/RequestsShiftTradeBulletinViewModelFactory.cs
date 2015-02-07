using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradebulletinViewModelFactory : IRequestsShiftTradebulletinViewModelFactory
	{

		private readonly IShiftTradeScheduleViewModelMapper _shiftTradeScheduleViewModelMapper;

		public RequestsShiftTradebulletinViewModelFactory(IShiftTradeScheduleViewModelMapper shiftTradeScheduleViewModelMapper)
		{
			_shiftTradeScheduleViewModelMapper = shiftTradeScheduleViewModelMapper;
		}


		public ShiftTradeScheduleViewModel CreateShiftTradeBulletinViewModel(ShiftTradeScheduleViewModelData data)
		{
			return _shiftTradeScheduleViewModelMapper.MapForBulletin(data);
		}

	}
}