using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsViewModelFactory : IRequestsViewModelFactory
	{
		private readonly IPersonRequestProvider _personRequestProvider;
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
		private readonly RequestsViewModelMapper _mapper;
		private readonly ShiftTradeSwapDetailViewModelMapper _shiftTradeSwapDetailViewModelMapper;
		private readonly PersonAccountViewModelMapper _personAccountViewModelMapper;
		private readonly IMultiplicatorDefinitionSetProvider _multiplicatorDefinitionSetProvider;

		public RequestsViewModelFactory(
			IPersonRequestProvider personRequestProvider,
			IAbsenceTypesProvider absenceTypesProvider,
			IPermissionProvider permissionProvider,
			IShiftTradeRequestProvider shiftTradeRequestprovider,
			IShiftTradePeriodViewModelMapper shiftTradeRequestsPeriodViewModelMapper,
			IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker,
			INow now,
			ILoggedOnUser loggedOnUser,
			IShiftTradeScheduleViewModelMapper shiftTradeScheduleViewModelMapper,
			IAbsenceAccountProvider personAccountProvider,
			IPersonRequestRepository personRequestRepository,
			RequestsViewModelMapper mapper,
			ShiftTradeSwapDetailViewModelMapper shiftTradeSwapDetailViewModelMapper,
			PersonAccountViewModelMapper personAccountViewModelMapper,
			IMultiplicatorDefinitionSetProvider multiplicatorDefinitionSetProvider)
		{
			_personRequestProvider = personRequestProvider;
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
			_mapper = mapper;
			_shiftTradeSwapDetailViewModelMapper = shiftTradeSwapDetailViewModelMapper;
			_personAccountViewModelMapper = personAccountViewModelMapper;
			_multiplicatorDefinitionSetProvider = multiplicatorDefinitionSetProvider;
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
				OvertimeRequestPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb)
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
				DatePickerFormat = dateFormat,
				OvertimeTypes = _multiplicatorDefinitionSetProvider.GetDefinitionSetsForCurrentUser().Select(definitionSet => new OvertimeTypeViewModel
				{
					Id = definitionSet.Id,
					Name = definitionSet.Name
				}).ToList()
			};
		}

		public IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging, RequestListFilter filter)
		{
			var requests = _personRequestProvider.RetrieveRequestsForLoggedOnUser(paging, filter);
			return requests.Select(_mapper.Map).ToArray();
		}

		public RequestViewModel CreateRequestViewModel(Guid id)
		{
			var request = _personRequestProvider.RetrieveRequest(id);
			return _mapper.Map(request);
		}

		public AbsenceAccountViewModel GetAbsenceAccountViewModel(Guid absenceId, DateOnly date)
		{
			var absence = _absenceTypesProvider.GetRequestableAbsences().FirstOrDefault(x => x.Id == absenceId);
			if (absence == null) return null;

			var absenceAccount = _personAccountProvider.GetPersonAccount(absence, date);
			return _personAccountViewModelMapper.Map(absenceAccount);
		}

		public ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel(Guid? id = null)
		{
			var ret = _shiftTradeRequestsPeriodViewModelMapper.Map(_shiftTradeRequestprovider.RetrieveUserWorkflowControlSet(), _now);

			if (id == null) return ret;

			var personRequest = _personRequestRepository.Find(id.Value);
			var shiftTrade = personRequest.Request as IShiftTradeRequest;

			if (shiftTrade == null || shiftTrade.Offer != null) return ret;

			ret.MiscSetting = new ShiftTradeRequestMiscSetting
			{
				AnonymousTrading = false
			};
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
				var shiftTradeSwapDetails = _shiftTradeSwapDetailViewModelMapper.Map(detail);

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

				shiftTradeSwapDetails.From.MinutesSinceTimeLineStart = (int)startTimeForSchedOne.Subtract(startTimeForTimeline).TotalMinutes;
				shiftTradeSwapDetails.To.MinutesSinceTimeLineStart = (int)startTimeForSchedTwo.Subtract(startTimeForTimeline).TotalMinutes;
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

			var test = mySite.HasValue ? mySite.ToString() : "";
			return test;
		}
	}
}