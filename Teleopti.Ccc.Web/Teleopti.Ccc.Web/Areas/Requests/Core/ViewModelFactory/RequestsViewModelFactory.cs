using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
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
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;

		public RequestsViewModelFactory(IRequestsProvider requestsProvider, IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider)
		{
			_requestsProvider = requestsProvider;
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
		}

		public IEnumerable<RequestViewModel> Create(AllRequestsFormData input)
		{
			var requests = _requestsProvider.RetrieveRequests(new DateOnlyPeriod(input.StartDate, input.EndDate));

			var requestViewModels = requests.Select(toViewModel).ToArray();

			var mapping = new Dictionary<RequestsSortingOrder, Func<RequestViewModel, object>>
			{
				{ RequestsSortingOrder.AgentNameAsc, model => model.AgentName },
				{ RequestsSortingOrder.AgentNameDesc, model => model.AgentName },
				{ RequestsSortingOrder.UpdatedOnAsc, model => model.UpdatedTime },
				{ RequestsSortingOrder.UpdatedOnDesc, model => model.UpdatedTime },
				{ RequestsSortingOrder.CreatedOnAsc, model => model.CreatedTime },
				{ RequestsSortingOrder.CreatedOnDesc, model => model.CreatedTime }
			};

			var primarySortingOrder = input.SortingOrders.Any()
				? input.SortingOrders.First()
				: RequestsSortingOrder.UpdatedOnDesc;

			var result = primarySortingOrder.ToString().EndsWith("Asc") ?
				requestViewModels.OrderBy(mapping[primarySortingOrder]) :
				requestViewModels.OrderByDescending(mapping[primarySortingOrder]);

			return input.SortingOrders.Where(o => mapping.ContainsKey(o))
				.Aggregate(result, (current, order) => order.ToString().EndsWith("Asc")
				? current.ThenBy(mapping[order])
				: current.ThenByDescending(mapping[order]));
		}

		private RequestViewModel toViewModel(IPersonRequest request)
		{
			return new RequestViewModel()
			{
				Id = request.Id.GetValueOrDefault(),
				Subject = request.GetSubject(new NoFormatting()),
				Message = request.GetMessage(new NoFormatting()),
				TimeZone =  _ianaTimeZoneProvider.WindowsToIana(request.Person.PermissionInformation.DefaultTimeZone().Id),
				PeriodStartTime = request.Request.Period.StartDateTime,
				PeriodEndTime = request.Request.Period.EndDateTime,
				CreatedTime = request.CreatedOn,
				UpdatedTime = request.UpdatedOn,
				AgentName = _personNameProvider.BuildNameFromSetting(request.Person.Name),
				Seniority=request.Person.Seniority,
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