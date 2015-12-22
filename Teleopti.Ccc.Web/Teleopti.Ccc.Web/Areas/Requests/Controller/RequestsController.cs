using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

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

		[HttpPost, Route("api/Requests/loadTextAndAbsenceRequests"), UnitOfWork]
		public virtual IEnumerable<RequestViewModel> AllRequests(AllRequestsFormData input)
		{
			return _requestsViewModelFactory.Create(input);
		}


		[HttpGet, Route("api/Requests/requests"), UnitOfWork]
		public virtual RequestListViewModel GetRequests([ModelBinder(typeof(AllRequestsFormDataConverter))] AllRequestsFormData input)
		{			
			return _requestsViewModelFactory.CreateRequestListViewModel(input);
		}		

	}
}