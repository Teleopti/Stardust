using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestViewModelMapper
	{
		RequestViewModel Map (RequestViewModel requestViewModel, IPersonRequest request);
	}
}