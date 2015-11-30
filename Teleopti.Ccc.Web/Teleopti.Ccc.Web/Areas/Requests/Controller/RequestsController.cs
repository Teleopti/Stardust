using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
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

		[HttpPost, Route("api/Requests/loadTextAndAbsenceRequests"), UnitOfWork]
		public virtual IEnumerable<RequestViewModel> AllRequests(AllRequestsFormData input)
		{
			return _requestsViewModelFactory.Create(new DateOnlyPeriod(input.StartDate, input.EndDate));
		}

		public class AllRequestsFormData
		{
			public DateOnly StartDate;
			public DateOnly EndDate;
		}

	}
}