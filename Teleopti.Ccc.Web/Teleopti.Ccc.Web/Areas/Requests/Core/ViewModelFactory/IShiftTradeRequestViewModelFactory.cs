using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IShiftTradeRequestViewModelFactory
	{
		ShiftTradeRequestListViewModel CreateRequestListViewModel(AllRequestsFormData input);
	}
}