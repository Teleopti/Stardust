using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class ShiftTradeRequestViewModelFactory : IShiftTradeRequestViewModelFactory
	{
		private readonly IRequestsProvider _requestsProvider;
		private readonly IRequestViewModelMapper _requestViewModelMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IUserCulture _userCulture;
		private readonly IScheduleProvider _scheduleProvider;

		public ShiftTradeRequestViewModelFactory(IRequestsProvider requestsProvider, IRequestViewModelMapper requestViewModelMapper, IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider, IUserCulture userCulture, IScheduleProvider scheduleProvider)
		{
			_requestsProvider = requestsProvider;
			_requestViewModelMapper = requestViewModelMapper;
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_userCulture = userCulture;
			_scheduleProvider = scheduleProvider;
		}

		public ShiftTradeRequestListViewModel CreateRequestListViewModel(AllRequestsFormData input)
		{
			var requestListModel = new ShiftTradeRequestListViewModel()
			{
				Requests = new RequestViewModel[] { }
			};

			if (input == null)
			{
				return requestListModel;
			}

			int totalCount;
			var requests = _requestsProvider.RetrieveRequests(input, new[] { RequestType.ShiftTradeRequest }, out totalCount).ToArray();

			requestListModel.TotalCount = totalCount;
			requestListModel.Skip = input.Paging.Skip;
			requestListModel.Take = input.Paging.Take;

			var existsRequests = requests.Any();

			if (existsRequests)
			{
				var requestMinDate = requests.Min(r => r.Request.Period.LocalStartDateTime);
				var requestMaxDate = requests.Max(r => r.Request.Period.LocalEndDateTime);
				requestListModel.Requests = requests.Select(request => _requestViewModelMapper.Map(createShiftTradeRequestViewModel(request), request)).ToList();
				requestListModel.MinimumDateTime = requestMinDate;
				requestListModel.MaximumDateTime = requestMaxDate;
				requestListModel.FirstDateForVisualisation = new DateOnly(DateHelper.GetFirstDateInWeek(requestMinDate, _userCulture.GetCulture()));
				requestListModel.LastDateForVisualisation = new DateOnly(DateHelper.GetLastDateInWeek(requestMaxDate, _userCulture.GetCulture()));
			}

			return requestListModel;
		}

		private ShiftTradeRequestViewModel createShiftTradeRequestViewModel(IPersonRequest request)
		{
			var shiftTradeRequest = (IShiftTradeRequest)request.Request;
			var personTo = shiftTradeRequest.PersonTo;
			var personToTeam = personTo.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
			var shiftTradeDays = shiftTradeRequest.ShiftTradeSwapDetails.Select(createShiftTradeDayViewModel);

			return new ShiftTradeRequestViewModel
			{
				PersonTo = _personNameProvider.BuildNameFromSetting(personTo.Name),
				PersonToTeam = personToTeam?.SiteAndTeam,
				PersonToTimeZone = _ianaTimeZoneProvider.WindowsToIana(personTo.PermissionInformation.DefaultTimeZone().Id),
				ShiftTradeDays = shiftTradeDays.ToList()
			};
		}

		private ShiftTradeDayViewModel createShiftTradeDayViewModel(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{

			var schedulePartTo = shiftTradeSwapDetail.SchedulePartTo ??
								 _scheduleProvider.GetScheduleForPersons (shiftTradeSwapDetail.DateTo, new[] {shiftTradeSwapDetail.PersonTo})
									 .SingleOrDefault();

			var schedulePartFrom = shiftTradeSwapDetail.SchedulePartFrom ??
								 _scheduleProvider.GetScheduleForPersons(shiftTradeSwapDetail.DateFrom, new[] { shiftTradeSwapDetail.PersonFrom })
									 .SingleOrDefault();


			var shiftTradeDayViewModel = new ShiftTradeDayViewModel()
			{
				Date = shiftTradeSwapDetail.DateTo,
				ToScheduleDayDetail = createScheduleDayDetail(schedulePartTo),
				FromScheduleDayDetail = createScheduleDayDetail(schedulePartFrom)
			};

			return shiftTradeDayViewModel;
		}

		private static ShiftTradeScheduleDayDetailViewModel createScheduleDayDetail(IScheduleDay scheduleDay)
		{
			var shiftTradeScheduleDayDetailViewModel = new ShiftTradeScheduleDayDetailViewModel();

			if (scheduleDay == null) return shiftTradeScheduleDayDetailViewModel;

			var significantPartForDisplay = scheduleDay.SignificantPartForDisplay();


			if (scheduleDay.HasDayOff())
			{
				mapDayOffFields (scheduleDay, shiftTradeScheduleDayDetailViewModel);
			}
			else
			{

				if (significantPartForDisplay == SchedulePartView.FullDayAbsence || significantPartForDisplay == SchedulePartView.ContractDayOff)
				{
					mapFullDayAbsenceFields (scheduleDay, shiftTradeScheduleDayDetailViewModel);
				}
				else
				{
					mapMainShiftFields(scheduleDay, shiftTradeScheduleDayDetailViewModel);
				}
			}

			return shiftTradeScheduleDayDetailViewModel;
		}

		private static void mapFullDayAbsenceFields (IScheduleDay scheduleDay, ShiftTradeScheduleDayDetailViewModel shiftTradeScheduleDayDetailViewModel)
		{
			var absenceCollection = scheduleDay.PersonAbsenceCollection();
			if (absenceCollection.Count > 0)
			{
				var absence = absenceCollection[0].Layer.Payload;

				shiftTradeScheduleDayDetailViewModel.Name = absence.Description.Name;
				shiftTradeScheduleDayDetailViewModel.ShortName = absence.Description.ShortName;
				shiftTradeScheduleDayDetailViewModel.Color = Color.White.ToHtml(); // absence.DisplayColor.ToHtml(); as requested, colour it white.
				shiftTradeScheduleDayDetailViewModel.Type = ShiftObjectType.FullDayAbsence;
			}
		}

		private static void mapMainShiftFields(IScheduleDay scheduleDay, ShiftTradeScheduleDayDetailViewModel shiftTradeScheduleDayDetailViewModel)
		{

			if (scheduleDay.SignificantPartForDisplay() != SchedulePartView.MainShift  )
			{
				return;
			}
			var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;

			shiftTradeScheduleDayDetailViewModel.Name = shiftCategory.Description.Name;
			shiftTradeScheduleDayDetailViewModel.ShortName = shiftCategory.Description.ShortName;
			shiftTradeScheduleDayDetailViewModel.Color = shiftCategory.DisplayColor.ToHtml();
			shiftTradeScheduleDayDetailViewModel.Type = ShiftObjectType.PersonAssignment;
		}

		private static void mapDayOffFields(IScheduleDay scheduleDay, ShiftTradeScheduleDayDetailViewModel shiftTradeScheduleDayDetailViewModel)
		{
			var dayOff = scheduleDay.PersonAssignment(false).DayOff();

			shiftTradeScheduleDayDetailViewModel.Name = dayOff.Description.Name;
			shiftTradeScheduleDayDetailViewModel.ShortName = dayOff.Description.ShortName;
			shiftTradeScheduleDayDetailViewModel.Color = dayOff.DisplayColor.ToHtml();
			shiftTradeScheduleDayDetailViewModel.Type = ShiftObjectType.DayOff;

		}
	}
}