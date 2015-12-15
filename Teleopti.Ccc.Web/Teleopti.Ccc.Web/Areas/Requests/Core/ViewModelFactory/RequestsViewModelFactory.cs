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

		// Deprecate after Wfm_Requests_Performance_36295. Use CreateRequestListViewModel instead.
		public IEnumerable<RequestViewModel> Create(AllRequestsFormData input)
		{
			
			var requests = _requestsProvider.RetrieveRequests(input);
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

		public RequestListViewModel CreateRequestListViewModel(AllRequestsFormData input)
		{
			throw new NotImplementedException();
		}

		private RequestViewModel toViewModel(IPersonRequest request)
		{
			var team = request.Person.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
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
							: RequestStatus.New,
				Payload = request.Request.RequestPayloadDescription,
				Team = team == null? null:team.Description.Name,
				IsFullDay = isFullDay(request)
			};
		}

		private bool isFullDay(IPersonRequest request)
		{
			// ref: Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.TextRequestFormMappingProfile
			var startTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			if (startTime.Hour != 0 || startTime.Minute != 0 || startTime.Second != 0) return false;
			var endTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			if (endTime.Hour != 23 || endTime.Minute != 59 || endTime.Second != 0) return false;
			return true;
		}

	}
}