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
		private readonly IAbsenceAccountProvider _personAccountProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IShiftTradeRequestProvider _shiftTradeRequestprovider;
		private readonly IShiftTradePeriodViewModelMapper _shiftTradeRequestsPeriodViewModelMapper;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradeScheduleViewModelMapper _shiftTradeScheduleViewModelMapper;

		public RequestsViewModelFactory(
			IPersonRequestProvider personRequestProvider,
			IMappingEngine mapper,
			IAbsenceTypesProvider absenceTypesProvider, 
			IPermissionProvider permissionProvider,
			IShiftTradeRequestProvider shiftTradeRequestprovider, 
			IShiftTradePeriodViewModelMapper shiftTradeRequestsPeriodViewModelMapper, 
			IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker,
			INow now,
			ILoggedOnUser loggedOnUser, 
			IShiftTradeScheduleViewModelMapper shiftTradeScheduleViewModelMapper,
			IAbsenceAccountProvider personAccountProvider)
		{
			_personRequestProvider = personRequestProvider;
			_mapper = mapper;
			_absenceTypesProvider = absenceTypesProvider;
			_permissionProvider = permissionProvider;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
			_shiftTradeRequestsPeriodViewModelMapper = shiftTradeRequestsPeriodViewModelMapper;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_now = now;
			_loggedOnUser = loggedOnUser;
			_shiftTradeScheduleViewModelMapper = shiftTradeScheduleViewModelMapper;
			_personAccountProvider = personAccountProvider;
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
			var dateFormat = _loggedOnUser.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
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
				RequestPermission = permission,
				DatePickerFormat = dateFormat
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
		

		public AbsenceAccountViewModel GetAbsenceAccountViewModel(Guid absenceId, DateOnly date)
		{
			var absence = _absenceTypesProvider.GetRequestableAbsences().First(x => x.Id == absenceId);
			var absenceAccount = _personAccountProvider.GetPersonAccount(absence, date);
			return _mapper.Map<IAccount, AbsenceAccountViewModel>(absenceAccount);
		}

		public ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel()
		{
			return _shiftTradeRequestsPeriodViewModelMapper.Map(_shiftTradeRequestprovider.RetrieveUserWorkflowControlSet(), _now);
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(ShiftTradeScheduleViewModelData data)
		{
			return _shiftTradeScheduleViewModelMapper.Map(data);
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModelForAllTeams(ShiftTradeScheduleViewModelDataForAllTeams data)
		{
			return _shiftTradeScheduleViewModelMapper.Map(data);
		}

		public IList<ShiftTradeSwapDetailsViewModel> CreateShiftTradeRequestSwapDetails(Guid id)
		{
			var shiftTradeSwapDetailsList = new List<ShiftTradeSwapDetailsViewModel>();
			var personRequest = _personRequestProvider.RetrieveRequest(id);

			_shiftTradeRequestStatusChecker.Check(personRequest.Request as IShiftTradeRequest);
			var req = personRequest.Request as IShiftTradeRequest;

			foreach (var detail in req.ShiftTradeSwapDetails)
			{
				var shiftTradeSwapDetails = _mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(detail);

				var startTimeForTimeline = shiftTradeSwapDetails.TimeLineStartDateTime;
				
				DateTime startTimeForSchedOne;
				DateTime startTimeForSchedTwo;
				if (detail.SchedulePartFrom != null && detail.SchedulePartTo != null)
				{
					var isFromDayoff = detail.SchedulePartFrom.SignificantPart() == SchedulePartView.DayOff;
					var isToDayoff = detail.SchedulePartTo.SignificantPart() == SchedulePartView.DayOff;
					if (isFromDayoff && !isToDayoff)
					{
						startTimeForSchedTwo = shiftTradeSwapDetails.To.StartTimeUtc;
						startTimeForSchedOne = shiftTradeSwapDetails.To.StartTimeUtc;
					}
					else if (!isFromDayoff && isToDayoff)
					{
						startTimeForSchedOne = shiftTradeSwapDetails.From.StartTimeUtc;
						startTimeForSchedTwo = shiftTradeSwapDetails.From.StartTimeUtc;
					}
					else
					{
						startTimeForSchedOne = shiftTradeSwapDetails.From.StartTimeUtc;
						startTimeForSchedTwo = shiftTradeSwapDetails.To.StartTimeUtc;
					}
				}
				else
				{
					startTimeForSchedOne = shiftTradeSwapDetails.From.StartTimeUtc;
					startTimeForSchedTwo = shiftTradeSwapDetails.To.StartTimeUtc;
				}

				shiftTradeSwapDetails.From.MinutesSinceTimeLineStart = (int) startTimeForSchedOne.Subtract(startTimeForTimeline).TotalMinutes;
				shiftTradeSwapDetails.To.MinutesSinceTimeLineStart = (int) startTimeForSchedTwo.Subtract(startTimeForTimeline).TotalMinutes;

				shiftTradeSwapDetailsList.Add(shiftTradeSwapDetails);
			}
			shiftTradeSwapDetailsList.Sort((s1, s2) => s1.From.StartTimeUtc.CompareTo(s2.From.StartTimeUtc));
			return shiftTradeSwapDetailsList;
		}

		public string CreateShiftTradeMyTeamSimpleViewModel(DateOnly selectedDate)
		{
			Guid? myTeam = _shiftTradeRequestprovider.RetrieveMyTeamId(selectedDate);

			return myTeam.HasValue ? myTeam.ToString() : "";
		}
	}
}