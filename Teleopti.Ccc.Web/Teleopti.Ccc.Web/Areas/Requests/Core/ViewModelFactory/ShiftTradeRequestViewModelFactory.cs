using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class ShiftTradeRequestViewModelFactory : IShiftTradeRequestViewModelFactory
	{
		private readonly IRequestsProvider _requestsProvider;
		private readonly IRequestViewModelMapper<ShiftTradeRequestViewModel> _requestViewModelMapper;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IUserCulture _userCulture;
		private readonly IRequestFilterCreator _requestFilterCreator;
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;
		private readonly IUserTimeZone _userTimeZone;

		private const int maxSearchPersonCount = 5000;

		public ShiftTradeRequestViewModelFactory(IRequestsProvider requestsProvider, IRequestViewModelMapper<ShiftTradeRequestViewModel> requestViewModelMapper, IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider, IScheduleProvider scheduleProvider, IUserCulture userCulture
			, IRequestFilterCreator requestFilterCreator, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings, IUserTimeZone userTimeZone)
		{
			_requestsProvider = requestsProvider;
			_requestViewModelMapper = requestViewModelMapper;
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_scheduleProvider = scheduleProvider;
			_userCulture = userCulture;
			_requestFilterCreator = requestFilterCreator;
			_nameFormatSettings = nameFormatSettings;
			_userTimeZone = userTimeZone;
		}

		public ShiftTradeRequestListViewModel CreateRequestListViewModel(AllRequestsFormData input)
		{
			var requestListModel = new ShiftTradeRequestListViewModel
			{
				FirstDayOfWeek = toIso8601DayNumber(_userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek),
				Requests = new ShiftTradeRequestViewModel[] { }
			};

			if (input == null || input.SelectedGroupIds.Length == 0)
			{
				return requestListModel;
			}

			requestListModel.MinimumDateTime = input.StartDate.Date;
			requestListModel.MaximumDateTime = input.EndDate.Date;

			int totalCount;

			var requestFilter = _requestFilterCreator.Create(input, new[] { RequestType.ShiftTradeRequest });

			if (requestFilter.Persons != null && requestFilter.Persons.Distinct().Count() > maxSearchPersonCount)
			{
				return new ShiftTradeRequestListViewModel
				{
					Requests = new ShiftTradeRequestViewModel[] {},
					IsSearchPersonCountExceeded = true,
					MaxSearchPersonCount = maxSearchPersonCount
				};
			}

			requestFilter.OnlyIncludeRequestsStartingWithinPeriod = true;
			requestFilter.ExcludeInvalidShiftTradeRequest = true;

			var requests = _requestsProvider.RetrieveShiftTradeRequests(requestFilter, out totalCount).ToArray();

			requestListModel.TotalCount = totalCount;
			requestListModel.Skip = input.Paging.Skip;
			requestListModel.Take = input.Paging.Take;

			var nameFormatSettings = _nameFormatSettings.Get();

			if (requests.Any())
			{
				requestListModel.Requests = requests.Select(request => _requestViewModelMapper.Map(createShiftTradeRequestViewModel(request, nameFormatSettings), request, nameFormatSettings)).ToList();
				var maximumRequestDate = requests.Max(r => r.Request.Period.EndDateTimeLocal(_userTimeZone.TimeZone()));
				if (maximumRequestDate > requestListModel.MaximumDateTime)
				{
					requestListModel.MaximumDateTime = maximumRequestDate;
				}
				
			}

			return requestListModel;
		}

		private ShiftTradeRequestViewModel createShiftTradeRequestViewModel(IPersonRequest request, NameFormatSettings nameFormatSettings)
		{
			var shiftTradeRequest = (IShiftTradeRequest)request.Request;
			var personTo = shiftTradeRequest.PersonTo;
			var personToTeam = personTo.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
			var shiftTradeDays = shiftTradeRequest.ShiftTradeSwapDetails.Select(createShiftTradeDayViewModel);

			return new ShiftTradeRequestViewModel
			{
				PersonTo = _personNameProvider.BuildNameFromSetting(personTo.Name, nameFormatSettings),
				PersonIdTo = personTo.Id.GetValueOrDefault(),
				PersonToTeam = personToTeam?.SiteAndTeam,
				PersonToTimeZone = _ianaTimeZoneProvider.WindowsToIana(personTo.PermissionInformation.DefaultTimeZone().Id),
				ShiftTradeDays = shiftTradeDays.ToList(),
				BrokenRules = NewBusinessRuleCollection.GetRuleDescriptionsFromFlag(request.BrokenBusinessRules.GetValueOrDefault(0))
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


			var shiftTradeDayViewModel = new ShiftTradeDayViewModel
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
			if (absenceCollection.Length > 0)
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

		private int toIso8601DayNumber(DayOfWeek dayOfWeek)
		{
			//ISO8601 = 1-7, 1 = Monday and 7 = Sunday.
			if (dayOfWeek == DayOfWeek.Sunday)
			{
				return 7;
			}
			else
			{
				return (int)dayOfWeek;
			}

		}

	}
}