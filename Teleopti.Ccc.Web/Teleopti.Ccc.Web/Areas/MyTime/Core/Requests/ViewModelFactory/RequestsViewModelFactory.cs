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
		private readonly IPersonRequestRepository _personRequestRepository;

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
			IAbsenceAccountProvider personAccountProvider,
			IPersonRequestRepository personRequestRepository)
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
			_personRequestRepository = personRequestRepository;
		}

		public RequestsViewModel CreatePageViewModel()
		{
			var permission = new RequestPermission
			{
				TextRequestPermission = _permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.TextRequests),
				AbsenceRequestPermission = _permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb),
				ShiftTradeRequestPermission = _permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				AbsenceReportPermission = _permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.AbsenceReport),
				ShiftTradeBulletinBoardPermission = _permissionProvider.HasApplicationFunctionPermission(
						DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard),
			};
			var dateFormat = _loggedOnUser.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			return new RequestsViewModel
			{
				AbsenceTypes =
					_absenceTypesProvider.GetRequestableAbsences().Select(requestableAbsence => new AbsenceTypeViewModel
					{
						Id = requestableAbsence.Id,
						Name = requestableAbsence.Description.Name
					}).ToList(),
				AbsenceTypesForReport =
					_absenceTypesProvider.GetReportableAbsences().Select(x => new AbsenceTypeViewModel
					{
						Id = x.Id,
						Name = x.Description.Name
					}).ToList(),
				RequestPermission = permission,
				DatePickerFormat = dateFormat
			};
		}

		public IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging, bool hideOldRequest = false)
		{
			var requests = _personRequestProvider.RetrieveRequestsForLoggedOnUser(paging, hideOldRequest);
			return _mapper.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(requests);
		}

		public RequestViewModel CreateRequestViewModel(Guid id)
		{
			var request = _personRequestProvider.RetrieveRequest(id);
			return _mapper.Map<IPersonRequest, RequestViewModel>(request);
		}

		public AbsenceAccountViewModel GetAbsenceAccountViewModel(Guid absenceId, DateOnly date)
		{
			var absence = _absenceTypesProvider.GetRequestableAbsences().FirstOrDefault(x => x.Id == absenceId);
			if (absence==null) return null;

			var absenceAccount = _personAccountProvider.GetPersonAccount(absence, date);
			return _mapper.Map<IAccount, AbsenceAccountViewModel>(absenceAccount);
		}

		public ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel(Guid? id = null)
		{
			var ret = _shiftTradeRequestsPeriodViewModelMapper.Map(_shiftTradeRequestprovider.RetrieveUserWorkflowControlSet(), _now);

			if (id != null)
			{
				var personRequest = _personRequestRepository.Find(id.Value);
				var shiftTrade = personRequest.Request as IShiftTradeRequest;
				if (shiftTrade != null)
				{
					if (shiftTrade.Offer == null)
					{
						ret.MiscSetting = new ShiftTradeRequestMiscSetting()
						{
							AnonymousTrading = false
						};
					}
				}
			}
			return ret;
		}

		public ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(ShiftTradeScheduleViewModelData data)
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
				shiftTradeSwapDetails.To.IsMySchedule = _loggedOnUser.CurrentUser().Equals(req.PersonTo);

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

		public string CreateShiftTradeMySiteIdViewModel(DateOnly selectedDate)
		{
			Guid? mySite = _shiftTradeRequestprovider.RetrieveMySiteId(selectedDate);

			return mySite.HasValue ? mySite.ToString() : "";
		}
	}
}