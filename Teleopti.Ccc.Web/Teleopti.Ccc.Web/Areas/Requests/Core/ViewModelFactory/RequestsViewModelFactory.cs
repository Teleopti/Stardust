using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly IPersonNameProvider _personNameProvider;

		public RequestsViewModelFactory(IPersonRequestProvider personRequestProvider, IPersonNameProvider personNameProvider)
		{
			_personRequestProvider = personRequestProvider;
			_personNameProvider = personNameProvider;
		}

		public IEnumerable<RequestViewModel> Create(AllRequestsFormData input)
		{
			var requests = _personRequestProvider.RetrieveRequests(new DateOnlyPeriod(input.StartDate, input.EndDate));
			
			var requestViewModels = requests.Select(x => new RequestViewModel()
			{
				Id = x.Id.GetValueOrDefault(),
				Subject = x.GetSubject(new NoFormatting()),
				Message = x.GetMessage(new NoFormatting()),
				CreatedTime = x.CreatedOn,
				UpdatedTime = x.UpdatedOn,
				AgentName =  _personNameProvider.BuildNameFromSetting(x.Person.Name),

				TypeText = x.Request.RequestTypeDescription,
				StatusText = x.StatusText,
				Status = x.IsApproved? RequestStatus.Approved : 
					x.IsPending? RequestStatus.Pending:
						x.IsDenied? RequestStatus.Denied
							: RequestStatus.New
			}).ToArray();


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
	}
}