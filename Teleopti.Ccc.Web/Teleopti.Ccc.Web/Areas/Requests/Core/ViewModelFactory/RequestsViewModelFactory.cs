using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IRequestsProvider _requestsProvider;
		private readonly IPersonNameProvider _personNameProvider;

		public RequestsViewModelFactory(IRequestsProvider requestsProvider, IPersonNameProvider personNameProvider)
		{
			_requestsProvider = requestsProvider;
			_personNameProvider = personNameProvider;
		}

		public IEnumerable<RequestViewModel> Create(AllRequestsFormData input)
		{
			var requests = _requestsProvider.RetrieveRequests(new DateOnlyPeriod(input.StartDate, input.EndDate));

			var requestViewModels = requests.Select(toViewModel).ToArray();

			var primarySortingOrder = input.SortingOrders.Any()
				? input.SortingOrders.First()
				: RequestsSortingOrder.UpdatedOnDesc;

			switch (primarySortingOrder)
			{				
				case RequestsSortingOrder.AgentNameDesc:
					return requestViewModels.OrderByDescending(x => x.AgentName);
				case RequestsSortingOrder.AgentNameAsc:
					return requestViewModels.OrderBy(x => x.AgentName);
				default:				
					return requestViewModels.OrderByDescending(x => x.UpdatedTime);
			}
		}

		private RequestViewModel toViewModel(IPersonRequest request)
		{
			return new RequestViewModel()
			{
				Id = request.Id.GetValueOrDefault(),
				Subject = request.GetSubject(new NoFormatting()),
				Message = request.GetMessage(new NoFormatting()),
				CreatedTime = request.CreatedOn,
				UpdatedTime = request.UpdatedOn,
				AgentName = _personNameProvider.BuildNameFromSetting(request.Person.Name),
				Type = request.Request.RequestType,
				TypeText = request.Request.RequestTypeDescription,
				StatusText = request.StatusText,
				Status = request.IsApproved
					? RequestStatus.Approved
					: request.IsPending
						? RequestStatus.Pending
						: request.IsDenied
							? RequestStatus.Denied
							: RequestStatus.New
			};
		}

	}
}