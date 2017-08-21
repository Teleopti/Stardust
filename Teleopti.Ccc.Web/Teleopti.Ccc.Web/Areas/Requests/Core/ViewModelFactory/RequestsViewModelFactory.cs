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
		private readonly IRequestViewModelMapper<AbsenceAndTextRequestViewModel> _requestViewModelMapper;
		private readonly IRequestFilterCreator _requestFilterCreator;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public RequestsViewModelFactory(IRequestsProvider requestsProvider,
			IRequestViewModelMapper<AbsenceAndTextRequestViewModel> requestViewModelMapper,
			IRequestFilterCreator requestFilterCreator, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_requestsProvider = requestsProvider;
			_requestViewModelMapper = requestViewModelMapper;
			_requestFilterCreator = requestFilterCreator;
			_nameFormatSettings = nameFormatSettings;
		}

		public RequestListViewModel<AbsenceAndTextRequestViewModel> CreateAbsenceAndTextRequestListViewModel(AllRequestsFormData input)
		{
			if (input == null || input.SelectedGroupIds.Length == 0)
			{
				return new RequestListViewModel<AbsenceAndTextRequestViewModel>
				{
					Requests = new AbsenceAndTextRequestViewModel[] { }
				};
			}

			int totalCount;
			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.AbsenceRequest, RequestType.TextRequest });
			var requests = _requestsProvider.RetrieveRequests(requestFilter, out totalCount);
			var nameFormatSettings = _nameFormatSettings.Get();

			return new RequestListViewModel<AbsenceAndTextRequestViewModel>
			{
				Requests = requests.Select(s => toAbsenceAndTextRequestViewModel(s, nameFormatSettings)).ToList(),
				TotalCount = totalCount,
				Skip = input.Paging.Skip,
				Take = input.Paging.Take
			};
		}

		private AbsenceAndTextRequestViewModel toAbsenceAndTextRequestViewModel(IPersonRequest request, NameFormatSettings nameFormatSetting)
		{
			return _requestViewModelMapper.Map(new AbsenceAndTextRequestViewModel(), request, nameFormatSetting);
		}
	}
}