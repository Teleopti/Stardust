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
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly INow _now;

		public RequestsViewModelFactory(IPersonRequestProvider personRequestProvider, IMappingEngine mapper, IAbsenceTypesProvider absenceTypesProvider, 
										IPermissionProvider permissionProvider, IShiftTradeRequestProvider shiftTradeRequestprovider, 
										IShiftTradePeriodViewModelMapper shiftTradeRequestsPeriodViewModelMapper, 
										IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker, INow now)
		{
			_personRequestProvider = personRequestProvider;
			_mapper = mapper;
			_absenceTypesProvider = absenceTypesProvider;
			_permissionProvider = permissionProvider;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
			_shiftTradeRequestsPeriodViewModelMapper = shiftTradeRequestsPeriodViewModelMapper;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_now = now;
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
											DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb),
									ShiftTradeRequestPermission =
										_permissionProvider.HasApplicationFunctionPermission(
											DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)
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
			return _shiftTradeRequestsPeriodViewModelMapper.Map(_shiftTradeRequestprovider.RetrieveUserWorkflowControlSet(), _now);
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(DateTime selectedDate)
		{
			return _mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(selectedDate));
		}

		public ShiftTradeSwapDetailsViewModel CreateShiftTradeRequestSwapDetails(Guid id)
		{
			var personRequest =  _personRequestProvider.RetrieveRequest(id);
			
			_shiftTradeRequestStatusChecker.Check(personRequest.Request as IShiftTradeRequest);

			var shiftTradeSwapDetails = _mapper.Map<IShiftTradeRequest,ShiftTradeSwapDetailsViewModel>(personRequest.Request as IShiftTradeRequest);
			
			var startTimeForTimeline = shiftTradeSwapDetails.TimeLineStartDateTime;
			var startTimeForSchedOne = shiftTradeSwapDetails.From.StartTimeUtc;
			var startTimeForSchedTwo = shiftTradeSwapDetails.To.StartTimeUtc;

			shiftTradeSwapDetails.From.MinutesSinceTimeLineStart = (int)startTimeForSchedOne.Subtract(startTimeForTimeline).TotalMinutes;
			shiftTradeSwapDetails.To.MinutesSinceTimeLineStart = (int) startTimeForSchedTwo.Subtract(startTimeForTimeline).TotalMinutes;
			
			return shiftTradeSwapDetails;
		}
	}
}