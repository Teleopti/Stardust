using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestViewModelMapper : IRequestViewModelMapper<AbsenceAndTextRequestViewModel>,
		IRequestViewModelMapper<ShiftTradeRequestViewModel>,
		IRequestViewModelMapper<OvertimeRequestViewModel>
	{
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;

		public RequestViewModelMapper(IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider,
			IPersonAbsenceAccountProvider personAbsenceAccountProvider, IUserTimeZone userTimeZone, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder, IShiftTradeAddScheduleLayerViewModelMapper layerMapper)
		{
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
			_userTimeZone = userTimeZone;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_layerMapper = layerMapper;
		}

		public AbsenceAndTextRequestViewModel Map(AbsenceAndTextRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			mapAbsenceRequestSpecificFields(requestViewModel, request);

			return requestViewModel;
		}

		public ShiftTradeRequestViewModel Map(ShiftTradeRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			return requestViewModel;
		}

		public OvertimeRequestViewModel Map(OvertimeRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			requestViewModel.OvertimeTypeDescription = (request.Request as IOvertimeRequest)?.MultiplicatorDefinitionSet.Name;

			requestViewModel.BrokenRules = NewBusinessRuleCollection.GetRuleDescriptionsFromFlag(request.BrokenBusinessRules.GetValueOrDefault(0));

			return requestViewModel;
		}

		private void mapRequestViewModel(RequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			var team = request.Person.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
			requestViewModel.Id = request.Id.GetValueOrDefault();
			requestViewModel.Subject = request.GetSubject(new NoFormatting());
			requestViewModel.Message = request.GetMessage(new NoFormatting());
			requestViewModel.DenyReason = Resources.ResourceManager.GetString(request.DenyReason) ?? request.DenyReason;
			requestViewModel.TimeZone =
				_ianaTimeZoneProvider.WindowsToIana(request.Person.PermissionInformation.DefaultTimeZone().Id);
			requestViewModel.PeriodStartTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime,
				request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.PeriodEndTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime,
				request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.CreatedTime = request.CreatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.CreatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
				: (DateTime?)null;
			requestViewModel.UpdatedTime = request.UpdatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.UpdatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
				: (DateTime?)null;
			requestViewModel.AgentName = _personNameProvider.BuildNameFromSetting(request.Person.Name, nameFormatSettings);
			requestViewModel.PersonId = request.Person.Id.GetValueOrDefault();
			requestViewModel.Seniority = request.Person.Seniority;
			requestViewModel.Type = request.Request.RequestType;
			requestViewModel.TypeText = request.Request.RequestTypeDescription;
			requestViewModel.StatusText = request.StatusText;
			requestViewModel.Status = getRequestStatus(request);
			requestViewModel.Payload = request.Request.RequestPayloadDescription;
			requestViewModel.Team = team?.SiteAndTeam;
		}

		private void mapAbsenceRequestSpecificFields (AbsenceAndTextRequestViewModel requestViewModel, IPersonRequest request)
		{
			if (request.Request is IAbsenceRequest absenceRequest)
			{
				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(absenceRequest.Person, new ScheduleDictionaryLoadOptions(false, false),
					absenceRequest.Period, _currentScenario.Current());

				requestViewModel.PersonAccountSummaryViewModel = getPersonalAccountApprovalSummary(absenceRequest);

				requestViewModel.IsFullDay = isFullDay(request);
				var scheduleRange = schedules[absenceRequest.Person];

				requestViewModel.Shifts = mapShifts(scheduleRange, absenceRequest.Person, absenceRequest.Period.ToDateOnlyPeriod(request.Person.PermissionInformation.DefaultTimeZone()));
			}
		}

		private IEnumerable<ShiftViewModel> mapShifts(IScheduleRange schedules, IPerson person, DateOnlyPeriod period)
		{
			var shiftViewModels = new List<ShiftViewModel>();
			foreach (var date in period.DayCollection())
			{
				var scheduleDay = schedules.ScheduledDay(date);
				var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
				var isDayOff = eventScheduleDay.DayOff != null;
				var isFulldayAbsence = eventScheduleDay.IsFullDayAbsence;
				var shiftCategory = scheduleDay.PersonAssignment()?.ShiftCategory;
				var categoryName = shiftCategory?.Description.Name;
				var shortName = shiftCategory?.Description.ShortName;
				string displayColor = null;
				if (shiftCategory != null) displayColor = mapColor(shiftCategory.DisplayColor.ToArgb());
				if (isFulldayAbsence)
				{
					var payload = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload;
					categoryName = payload.ConfidentialDescription(person).Name;
					var absenceColor = payload.ConfidentialDisplayColor(person);
					displayColor = mapColor(absenceColor.ToArgb());
					shortName = payload.Description.ShortName;
				}
				var shiftViewModel = new ShiftViewModel
				{
					ContractTimeInMinute = eventScheduleDay.ContractTime.TotalMinutes,
					DayOffName = eventScheduleDay.Name,
					IsDayOff = isDayOff,
					IsNotScheduled = eventScheduleDay.Shift == null && !isDayOff && !isFulldayAbsence,
					MinStart = date.Date,
					Name = _personNameProvider.BuildNameFromSetting(scheduleDay.Person.Name.FirstName,
						scheduleDay.Person.Name.LastName),
					PersonId = scheduleDay.Person.Id.GetValueOrDefault(),
					ShiftCategory = new ShiftCategoryViewModel { Name = categoryName, ShortName = shortName, DisplayColor = displayColor },
					ScheduleLayers = getScheduleLayers(eventScheduleDay, scheduleDay.PersonAssignment())
				};
				shiftViewModels.Add(shiftViewModel);
			}

			return shiftViewModels;
		}

		private string mapColor(int argb)
		{
			return ColorTranslator.ToHtml(Color.FromArgb(argb));
		}

		private IList<SimpleLayer> mapLayers(ProjectionChangedEventScheduleDay eventScheduleDay)
		{
			var layers = new List<SimpleLayer>();
			if (eventScheduleDay.Shift == null) return layers;

			var ls = from layer in eventScheduleDay.Shift.Layers
				select new SimpleLayer
				{
					Color = mapColor(layer.DisplayColor),
					Description = layer.Name,
					Start = layer.StartDateTime,
					End = layer.EndDateTime,
					Minutes = (int)layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes,
					IsAbsenceConfidential = layer.IsAbsenceConfidential
				};

			layers.AddRange(ls);
			return layers;
		}

		private TeamScheduleLayerViewModel[] getScheduleLayers(ProjectionChangedEventScheduleDay eventScheduleDay, IPersonAssignment personAssignment)
		{
			var layers = mapLayers(eventScheduleDay);
			return _layerMapper.Map(layers, personAssignment?.OvertimeActivities());
		}

		private PersonAccountSummaryViewModel getPersonalAccountApprovalSummary(IAbsenceRequest absenceRequest)
		{
			var personAccountSummaryDetails = new List<PersonAccountSummaryDetailViewModel>();

			var personAbsenceAccount = _personAbsenceAccountProvider.Find(absenceRequest.Person);
			if (personAbsenceAccount != null)
			{
				var accountForPersonAbsence = personAbsenceAccount.Find (absenceRequest.Absence);
				var affectedAccounts =
					accountForPersonAbsence?.Find(absenceRequest.Period.ToDateOnlyPeriod(_userTimeZone.TimeZone()));

				if (affectedAccounts != null)
				{

					var sortedAccounts = affectedAccounts.OrderBy (account => account.StartDate);

					personAccountSummaryDetails.AddRange (
						sortedAccounts.Select (account => getPersonalAccountPeriodViewModelsForRequest (absenceRequest, account)));
				}
			}
			return new PersonAccountSummaryViewModel
			{
				PersonAccountSummaryDetails =  personAccountSummaryDetails
			};
		}

		private static PersonAccountSummaryDetailViewModel getPersonalAccountPeriodViewModelsForRequest(
			IAbsenceRequest absenceRequest, IAccount account)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			return new PersonAccountSummaryDetailViewModel
			{
				StartDate = TimeZoneHelper.ConvertFromUtc(account.StartDate.Date,
					timeZone),
				EndDate = TimeZoneHelper.ConvertFromUtc(account.Period().EndDate.Date,
					timeZone),
				RemainingDescription = convertTimeSpanToString(account.Remaining, absenceRequest.Absence.Tracker),
				TrackingTypeDescription = getDescriptionOfTrackerTimeSpan(absenceRequest.Absence.Tracker.GetType())
			};
		}

		private static string convertTimeSpanToString(TimeSpan ts, ITracker tracker)
		{
			var result = string.Empty;
			
			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				result = ts.TotalDays.ToString(CultureInfo.CurrentCulture);
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				result = TimeHelper.GetLongHourMinuteTimeString(ts, CultureInfo.CurrentCulture);
			}

			return result;
		}

		private static string getDescriptionOfTrackerTimeSpan(Type trackerClassType)
		{
			var trackerType = "";

			if (trackerClassType == Tracker.CreateDayTracker().GetType())
			{
				trackerType = Resources.Days;
			}
			else if (trackerClassType == Tracker.CreateTimeTracker().GetType())
			{
				trackerType = Resources.Hours;
			}
			return trackerType;

		}

		private static RequestStatus getRequestStatus(IPersonRequest request)
		{
			//ROBTODO: review status - should we include waitlisted and cancelled in this ?
			return request.IsApproved
				? RequestStatus.Approved
				: request.IsPending
					? RequestStatus.Pending
					: request.IsDenied
						? RequestStatus.Denied
						: RequestStatus.New;
		}

		private static bool isFullDay(IPersonRequest request)
		{
			// ref: Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.TextRequestFormMappingProfile
			var timeZone = request.Person.PermissionInformation.DefaultTimeZone();
			var startTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime,
				timeZone);
			if (startTime.Hour != 0 || startTime.Minute != 0 || startTime.Second != 0) return false;
			var endTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime,
				timeZone);
			if (endTime.Hour != 23 || endTime.Minute != 59 || endTime.Second != 0) return false;
			return true;
		}
	}
}