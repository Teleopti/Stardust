using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class RequestsViewModelMapper
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ILinkProvider _linkProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IToggleManager _toggleManager;

		public RequestsViewModelMapper(IUserTimeZone userTimeZone,
			ILinkProvider linkProvider,
			ILoggedOnUser loggedOnUser,
			IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker,
			IPersonNameProvider personNameProvider,
			IToggleManager toggleManager)
		{
			_userTimeZone = userTimeZone;
			_linkProvider = linkProvider;
			_loggedOnUser = loggedOnUser;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_personNameProvider = personNameProvider;
			_toggleManager = toggleManager;
		}

		public RequestViewModel Map(IPersonRequest s)
		{
			return new RequestViewModel
			{
				Link = new Link
				{
					rel = "self",
					href = s.Id.HasValue ? _linkProvider.RequestDetailLink(s.Id.Value) : null,
					Methods = getAvailableMethods(s)
				},
				Subject = s.GetSubject(new NoFormatting()),
				DateTimeFrom =
					DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.Request.Period.StartDateTime, _userTimeZone.TimeZone()),
				DateTimeTo =
					DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.Request.Period.EndDateTime, _userTimeZone.TimeZone()),
				Status = getStatusText(s),
				IsNextDay = isNextDay(s),
				IsReferred = isReferred(s),
				Text = s.GetMessage(new NoFormatting()),
				Type = s.Request.RequestTypeDescription,
				TypeEnum = s.Request.RequestType,
				UpdatedOnDateTime =
					s.UpdatedOn.HasValue
						? DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.UpdatedOn.Value, _userTimeZone.TimeZone())
						: null,
				DateFromYear =
					CultureInfo.CurrentCulture.Calendar.GetYear(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime,
						_userTimeZone.TimeZone())),
				DateFromMonth =
					CultureInfo.CurrentCulture.Calendar.GetMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime,
						_userTimeZone.TimeZone())),
				DateFromDayOfMonth =
					CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime,
						_userTimeZone.TimeZone())),
				DateToYear =
					CultureInfo.CurrentCulture.Calendar.GetYear(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime,
						_userTimeZone.TimeZone())),
				DateToMonth =
					CultureInfo.CurrentCulture.Calendar.GetMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime,
						_userTimeZone.TimeZone())),
				DateToDayOfMonth =
					CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime,
						_userTimeZone.TimeZone())),
				Payload = s.Request.RequestPayloadDescription.Name,
				PayloadId = resolvePayloadId(s),
				IsFullDay = isRequestFullDay(s),
				IsCreatedByUser = isCreatedByUser(s.Request, _loggedOnUser),
				From = s.Request.PersonFrom == null
					? string.Empty
					: _personNameProvider.BuildNameFromSetting(s.Request.PersonFrom.Name),
				To = s.Request.PersonTo == null
					? string.Empty
					: _personNameProvider.BuildNameFromSetting(s.Request.PersonTo.Name),
				DenyReason = denyReason(s),
				ExchangeOffer = exchangeOffer(s),
				Id = s.Id.ToString(),
				IsDenied = s.IsDenied,
				IsApproved = s.IsApproved,
				IsNew = s.IsNew,
				IsPending = s.IsPending
			};
		}

		private string getAvailableMethods(IPersonRequest personRequest)
		{
			var request = personRequest.Request as ShiftExchangeOffer;
			if (request != null)
			{
				var offer = (IShiftExchangeOffer) personRequest.Request;
				var isRealPending = (offer.Status == ShiftExchangeOfferStatus.Pending) && !offer.IsExpired();
				return isRealPending ? "GET, DELETE, PUT" : "GET, DELETE";
			}

			if (personRequest.IsNew || personRequest.IsPending)
			{
				return "GET, DELETE, PUT";
			}

			if (canCancelPersonRequest(personRequest))
			{
				return "GET, CANCEL";
			}

			return personRequest.IsWaitlisted ? "GET, DELETE" : "GET";
		}

		private bool canCancelPersonRequest(IPersonRequest personRequest)
		{
			if (!personRequest.IsApproved || !_toggleManager.IsEnabled(Toggles.Wfm_Requests_Cancel_Agent_38055))
			{
				return false;
			}

			if (!(personRequest.Request is AbsenceRequest))
			{
				return false;
			}

			var dateforDayCancellationCheck =
				new DateOnly(personRequest.Request.Period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone));
			if (dateforDayCancellationCheck >= DateOnly.Today)
			{
				if (PrincipalAuthorization.Current().IsPermitted(
					DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, new DateOnly(personRequest.RequestedDate),
					personRequest.Person))
				{
					return true;
				}
			}

			return false;
		}

		private string getStatusText(IPersonRequest s)
		{
			var ret = s.StatusText;
			if (s.IsPending)
			{
				var shiftTradeRequest = s.Request as IShiftTradeRequest;
				if (shiftTradeRequest != null)
				{
					ret += ", " +
						   shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)
							   .ToText(isCreatedByUser(s.Request, _loggedOnUser));
					return ret;
				}
			}

			if (s.Request.RequestType == RequestType.ShiftExchangeOffer)
			{
				var shiftExchangeOffer = s.Request as IShiftExchangeOffer;
				ret = shiftExchangeOffer.GetStatusText();
			}
			return ret;
		}

		private bool isRequestFullDay(IPersonRequest personRequest)
		{
			var start =
				TimeZoneInfo.ConvertTimeFromUtc(
					personRequest.Request.Period.StartDateTime, _userTimeZone.TimeZone());
			var end =
				TimeZoneInfo.ConvertTimeFromUtc(
					personRequest.Request.Period.EndDateTime, _userTimeZone.TimeZone());
			var allDayEndDateTime = start.AddDays(1).AddMinutes(-1);
			return start.TimeOfDay == TimeSpan.Zero &&
				   end.TimeOfDay == allDayEndDateTime.TimeOfDay;
		}

		private string resolvePayloadId(IPersonRequest personRequest)
		{
			return personRequest.Request.RequestType != RequestType.AbsenceRequest
				? null
				: ((IAbsenceRequest) personRequest.Request).Absence.Id.GetValueOrDefault().ToString();
		}

		private static bool isCreatedByUser(IRequest request, ILoggedOnUser loggedOnUser)
		{
			return request.PersonFrom != null && request.PersonFrom.Equals(loggedOnUser.CurrentUser());
		}

		private static ShiftExchangeOfferRequestViewModel exchangeOffer(IPersonRequest s)
		{
			if (s.Request.RequestType == RequestType.ShiftExchangeOffer)
			{
				var offer = s.Request as IShiftExchangeOffer;
				return new ShiftExchangeOfferRequestViewModel
				{
					WishShiftType = offer.DayType.ToString(),
					ValidTo = offer.ValidTo.Date
				};
			}

			if (s.Request.RequestType == RequestType.ShiftTradeRequest)
			{
				return new ShiftExchangeOfferRequestViewModel
				{
					IsOfferAvailable = true
				};
			}

			return null;
		}

		private static string denyReason(IPersonRequest s)
		{
			Resources.ResourceManager.IgnoreCase = true;
			var result = Resources.ResourceManager.GetString(s.DenyReason);
			if (string.IsNullOrEmpty(result))
			{
				result = s.DenyReason;
			}
			return result;
		}

		private bool isReferred(IPersonRequest s)
		{
			if (!s.IsPending) return false;

			var shiftTradeRequest = s.Request as IShiftTradeRequest;
			return shiftTradeRequest != null &&
				   shiftTradeRequest.GetShiftTradeStatus(
					   _shiftTradeRequestStatusChecker) == ShiftTradeStatus.Referred;
		}

		private bool isNextDay(IPersonRequest s)
		{
			var shiftExchangeOffer = s.Request as IShiftExchangeOffer;
			if (shiftExchangeOffer != null)
			{
				var timeZone = _userTimeZone.TimeZone();
				return shiftExchangeOffer.Period.ToDateOnlyPeriod(timeZone).DayCount() > 1;
			}
			return false;
		}
	}
}