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
		private readonly IRequestViewModelMapper<AbsenceAndTextRequestViewModel> _absenceAndTextRequestViewModelMapper;
		private readonly IRequestViewModelMapper<OvertimeRequestViewModel> _overtimeRequestViewModelMapper;
		private readonly IRequestFilterCreator _requestFilterCreator;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public RequestsViewModelFactory(IRequestsProvider requestsProvider,
			IRequestViewModelMapper<AbsenceAndTextRequestViewModel> absenceAndTextRequestViewModelMapper,
			IRequestFilterCreator requestFilterCreator, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings, IRequestViewModelMapper<OvertimeRequestViewModel> overtimeRequestViewModelMapper)
		{
			_requestsProvider = requestsProvider;
			_absenceAndTextRequestViewModelMapper = absenceAndTextRequestViewModelMapper;
			_requestFilterCreator = requestFilterCreator;
			_nameFormatSettings = nameFormatSettings;
			_overtimeRequestViewModelMapper = overtimeRequestViewModelMapper;
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

			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.AbsenceRequest, RequestType.TextRequest });
			var requests = _requestsProvider.RetrieveRequests(requestFilter, out int totalCount);
			var nameFormatSettings = _nameFormatSettings.Get();

			return new RequestListViewModel<AbsenceAndTextRequestViewModel>
			{
				Requests = requests.Select(s => toAbsenceAndTextRequestViewModel(s, nameFormatSettings)).ToList(),
				TotalCount = totalCount,
				Skip = input.Paging.Skip,
				Take = input.Paging.Take
			};
		}

		public RequestListViewModel<OvertimeRequestViewModel> CreateOvertimeRequestListViewModel(AllRequestsFormData input)
		{
			if (input == null || input.SelectedGroupIds.Length == 0)
			{
				return new RequestListViewModel<OvertimeRequestViewModel>
				{
					Requests = new OvertimeRequestViewModel[] { }
				};
			}

			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.OvertimeRequest });
			var requests = _requestsProvider.RetrieveRequests(requestFilter, out int totalCount);
			var nameFormatSettings = _nameFormatSettings.Get();

			return new RequestListViewModel<OvertimeRequestViewModel>
			{
				Requests = requests.Select(s => toOvertimeRequestViewModel(s, nameFormatSettings)).ToList(),
				TotalCount = totalCount,
				Skip = input.Paging.Skip,
				Take = input.Paging.Take
			};
		}

		private AbsenceAndTextRequestViewModel toAbsenceAndTextRequestViewModel(IPersonRequest request,
			NameFormatSettings nameFormatSetting)
		{
			return _absenceAndTextRequestViewModelMapper.Map(new AbsenceAndTextRequestViewModel(), request, nameFormatSetting);
		}

		private OvertimeRequestViewModel toOvertimeRequestViewModel(IPersonRequest request, NameFormatSettings nameFormatSetting)
		{
			return _overtimeRequestViewModelMapper.Map(new OvertimeRequestViewModel(), request, nameFormatSetting);
		}
	}
}