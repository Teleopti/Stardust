using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestViewModelMapper
	{
		RequestViewModel Map (RequestViewModel requestViewModel, IPersonRequest request);
	}
}