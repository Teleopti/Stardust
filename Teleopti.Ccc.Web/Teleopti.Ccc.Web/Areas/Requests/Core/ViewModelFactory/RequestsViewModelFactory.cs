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

			var primarySortingOrder = input.SortingOrders.Any()
				? input.SortingOrders.First()
				: RequestsSortingOrder.UpdatedOnDesc;

			switch (primarySortingOrder)
			{				
				case RequestsSortingOrder.AgentNameDesc:
					return requestViewModels.OrderByDescending(x => x.AgentName);
				case RequestsSortingOrder.AgentNameAsc:
					return requestViewModels.OrderBy(x => x.AgentName);
				case RequestsSortingOrder.CreatedOnDesc:
					return requestViewModels.OrderByDescending(x => x.CreatedTime);
				case RequestsSortingOrder.CreatedOnAsc:
					return requestViewModels.OrderBy(x => x.CreatedTime);
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