using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
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