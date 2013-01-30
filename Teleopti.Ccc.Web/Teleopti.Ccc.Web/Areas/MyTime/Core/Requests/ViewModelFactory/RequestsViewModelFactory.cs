using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly IMappingEngine _mapper;
		private readonly IAbsenceTypesProvider _absenceTypesProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IShiftTradeRequestProvider _shiftTradeRequestprovider;
		private readonly IShiftTradePeriodViewModelMapper _shiftTradeRequestsPeriodViewModelMapper;

		public RequestsViewModelFactory(IPersonRequestProvider personRequestProvider, IMappingEngine mapper, IAbsenceTypesProvider absenceTypesProvider, 
										IPermissionProvider permissionProvider, IShiftTradeRequestProvider shiftTradeRequestprovider, 
										IShiftTradePeriodViewModelMapper shiftTradeRequestsPeriodViewModelMapper)
		{
			_personRequestProvider = personRequestProvider;
			_mapper = mapper;
			_absenceTypesProvider = absenceTypesProvider;
			_permissionProvider = permissionProvider;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
			_shiftTradeRequestsPeriodViewModelMapper = shiftTradeRequestsPeriodViewModelMapper;
		}

		public RequestsViewModel CreatePageViewModel()
		{
			var permission = new RequestPermission
			                 	{
			                 		TextRequestPermission =
			                 			_permissionProvider.HasApplicationFunctionPermission(
			                 				DefinedRaptorApplicationFunctionPaths.TextRequests),
			                 		AbsenceRequestPermission =
			                 			_permissionProvider.HasApplicationFunctionPermission(
			                 				DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)
			                 	};
			return new RequestsViewModel
			{
				AbsenceTypes =
					_absenceTypesProvider.GetRequestableAbsences().Select(requestableAbsence => new AbsenceTypeViewModel
					{
						Id = requestableAbsence.Id,
						Name =
							requestableAbsence.
							Description.Name
					}).ToList(),
				RequestPermission = permission
			};
		}

		public IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging)
		{
			var requests = _personRequestProvider.RetrieveRequests(paging);
			return _mapper.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(requests);
		}

		public RequestViewModel CreateRequestViewModel(Guid id)
		{
			var request = _personRequestProvider.RetrieveRequest(id);
			return _mapper.Map<IPersonRequest, RequestViewModel>(request);
		}
		
		public ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel()
		{
			return _shiftTradeRequestsPeriodViewModelMapper.Map(_shiftTradeRequestprovider.RetrieveUserWorkflowControlSet());
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(DateTime selectedDate)
		{
			return _mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(selectedDate));
		}
	}
}