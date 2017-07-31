using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IRequestsProvider _requestsProvider;
		private readonly IRequestViewModelMapper _requestViewModelMapper;
		private readonly IRequestFilterCreator _requestFilterCreator;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public RequestsViewModelFactory(IRequestsProvider requestsProvider,
			IRequestViewModelMapper requestViewModelMapper,
			IRequestFilterCreator requestFilterCreator, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_requestsProvider = requestsProvider;
			_requestViewModelMapper = requestViewModelMapper;
			_requestFilterCreator = requestFilterCreator;
			_nameFormatSettings = nameFormatSettings;
		}

		// Deprecate after Wfm_Requests_Performance_36295. Use CreateRequestListViewModel instead.
		public IEnumerable<RequestViewModel> Create(AllRequestsFormData input)
		{
			int totalCount;

			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.AbsenceRequest, RequestType.TextRequest });
			var requests = _requestsProvider.RetrieveRequests(requestFilter, out totalCount);
			var nameFormatSettings = _nameFormatSettings.Get();
			var requestViewModels = requests.Select(s => toViewModel(s, nameFormatSettings)).ToArray();

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
			if (input == null || input.SelectedTeamIds.Length == 0)
			{
				return new RequestListViewModel
				{
					Requests = new RequestViewModel[] { }
				};
			}

			int totalCount;
			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.AbsenceRequest, RequestType.TextRequest });
			var requests = _requestsProvider.RetrieveRequests(requestFilter, out totalCount);
			var nameFormatSettings = _nameFormatSettings.Get();

			return new RequestListViewModel
			{
				Requests = requests.Select(s => toViewModel(s, nameFormatSettings)).ToList(),
				TotalCount = totalCount,
				Skip = input.Paging.Skip,
				Take = input.Paging.Take
			};
		}

		private RequestViewModel toViewModel(IPersonRequest request, NameFormatSettings nameFormatSetting)
		{
			return _requestViewModelMapper.Map(new RequestViewModel(), request, nameFormatSetting);
		}
	}
}