using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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

		public ShiftTradeRequestViewModelFactory(IRequestsProvider requestsProvider, IRequestViewModelMapper requestViewModelMapper, IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider)
		{
			_requestsProvider = requestsProvider;
			_requestViewModelMapper = requestViewModelMapper;
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
		}

		public RequestListViewModel CreateRequestListViewModel(AllRequestsFormData input)
		{
			int totalCount;
			var requests = _requestsProvider.RetrieveRequests(input, new[] { RequestType.ShiftTradeRequest }, out totalCount);

			return new RequestListViewModel()
			{
				Requests =
					requests.Select(request => _requestViewModelMapper.Map(createShiftTradeRequestViewModel(request), request))
			};

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
				ShiftTradeDays = shiftTradeDays
			};
		}

		private static ShiftTradeDayViewModel createShiftTradeDayViewModel(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			var shiftTradeDayViewModel = new ShiftTradeDayViewModel()
			{
				Date = shiftTradeSwapDetail.DateTo,
				ToScheduleDayDetail = createScheduleDayDetail(shiftTradeSwapDetail.SchedulePartTo),
				FromScheduleDayDetail = createScheduleDayDetail(shiftTradeSwapDetail.SchedulePartFrom)
			};

			return shiftTradeDayViewModel;
		}

		private static ShiftTradeScheduleDayDetailViewModel createScheduleDayDetail(IScheduleDay scheduleDay)
		{
			var shiftTradeScheduleDayDetailViewModel = new ShiftTradeScheduleDayDetailViewModel();

			if (scheduleDay.HasDayOff())
			{
				mapDayOffFields(scheduleDay, shiftTradeScheduleDayDetailViewModel);
			}
			else
			{
				mapMainShiftFields(scheduleDay, shiftTradeScheduleDayDetailViewModel);
			}

			return shiftTradeScheduleDayDetailViewModel;
		}

		private static void mapMainShiftFields(IScheduleDay scheduleDay, ShiftTradeScheduleDayDetailViewModel shiftTradeScheduleDayDetailViewModel)
		{
			if (scheduleDay.SignificantPartForDisplay() != SchedulePartView.MainShift)
			{
				return;
			}
			var shiftCategory = scheduleDay.PersonAssignment().ShiftCategory;
			
			shiftTradeScheduleDayDetailViewModel.Name = shiftCategory.Description.Name;
			//ROBTODO: is the short name actually being set???
			shiftTradeScheduleDayDetailViewModel.Color = shiftCategory.DisplayColor.ToHtml();
			shiftTradeScheduleDayDetailViewModel.ShortName= shiftCategory.Description.ShortName;
		}

		private static void mapDayOffFields(IScheduleDay scheduleDay, ShiftTradeScheduleDayDetailViewModel shiftTradeScheduleDayDetailViewModel)
		{
			var dayOff = scheduleDay.PersonAssignment(false).DayOff();

			shiftTradeScheduleDayDetailViewModel.Name = dayOff.Description.Name;
			//ROBTODO: is the short name actually being set???
			shiftTradeScheduleDayDetailViewModel.ShortName= dayOff.Description.ShortName;
			shiftTradeScheduleDayDetailViewModel.Color = dayOff.DisplayColor.ToHtml();
		}
	}
}