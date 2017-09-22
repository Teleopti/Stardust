using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestsViewModelFactory
	{
		RequestListViewModel<AbsenceAndTextRequestViewModel> CreateAbsenceAndTextRequestListViewModel(AllRequestsFormData input);
		RequestListViewModel<OvertimeRequestViewModel> CreateOvertimeRequestListViewModel(AllRequestsFormData input);
	}
}