using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebRequests)]
	public class RequestsController : ApiController
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
		}

		public IEnumerable<RequestViewModel> All(DateOnlyPeriod dateOnlyPeriod)
		{
			return _requestsViewModelFactory.Create(dateOnlyPeriod);
		}
	}
}