using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestsViewModelFactory
	{
		IEnumerable<RequestViewModel> Create(DateOnlyPeriod dateOnlyPeriod);
	}
}