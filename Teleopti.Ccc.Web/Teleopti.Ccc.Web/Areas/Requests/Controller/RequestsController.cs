using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebRequests)]
	public class RequestsController : ApiController
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly IRequestCommandHandlingProvider _commandHandlingProvider;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory, IRequestCommandHandlingProvider commandHandlingProvider)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_commandHandlingProvider = commandHandlingProvider;
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

		[HttpPost, Route("api/Requests/approveRequests"), UnitOfWork]
		public virtual IEnumerable<Guid> ApproveRequests(IEnumerable<Guid> ids)
		{
			return _commandHandlingProvider.ApproveRequests(ids);
		}

		[HttpPost, Route("api/Requests/denyRequests"), UnitOfWork]
		public virtual IEnumerable<Guid> DenyRequests(IEnumerable<Guid> ids)
		{
			return _commandHandlingProvider.DenyRequests(ids);
		}

	}
}