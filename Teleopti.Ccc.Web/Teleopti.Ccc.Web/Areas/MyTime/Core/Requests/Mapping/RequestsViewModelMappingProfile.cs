using AutoMapper;
using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
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
	public class RequestsViewModelMappingProfile : Profile
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ILinkProvider _linkProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IToggleManager _toggleManager;

		public RequestsViewModelMappingProfile(IUserTimeZone userTimeZone,
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


		protected override void Configure()
		{
			CreateMap<IPersonRequest, RequestViewModel>()
				.ForMember(d => d.Link, o => o.MapFrom(s => s))
				.ForMember(d => d.Subject, o => o.MapFrom(s => s.GetSubject(new NoFormatting())))
				.ForMember(d => d.DateTimeFrom, o => o.ResolveUsing(
					s => DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.Request.Period.StartDateTime, _userTimeZone.TimeZone())))
				.ForMember(d => d.DateTimeTo, o => o.ResolveUsing(
					s => DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.Request.Period.EndDateTime, _userTimeZone.TimeZone())))
				.ForMember(d => d.Status, o => o.ResolveUsing(s =>
				{
					return getStatusText(s);
				}))
				.ForMember(d => d.IsNextDay, o => o.ResolveUsing((s =>
				{
					var shiftExchangeOffer = s.Request as IShiftExchangeOffer;
					if (shiftExchangeOffer != null)
					{
						var timeZone = _userTimeZone.TimeZone();
						var localStart = TimeZoneHelper.ConvertFromUtc(shiftExchangeOffer.Period.StartDateTime, timeZone);
						var localEnd = TimeZoneHelper.ConvertFromUtc(shiftExchangeOffer.Period.EndDateTime, timeZone);
						return !localStart.Date.Equals(localEnd.Date);
					}
					return false;
				})))
				.ForMember(d => d.IsReferred, o => o.ResolveUsing(s =>
				{
					if (!s.IsPending) return false;

					var shiftTradeRequest = s.Request as IShiftTradeRequest;
					return (shiftTradeRequest != null &&
							shiftTradeRequest.GetShiftTradeStatus(
								_shiftTradeRequestStatusChecker) == ShiftTradeStatus.Referred);
				}))
				.ForMember(d => d.Text, o => o.MapFrom(s => s.GetMessage(new NoFormatting())))
				.ForMember(d => d.Type, o => o.MapFrom(s => s.Request.RequestTypeDescription))
				.ForMember(d => d.TypeEnum, o => o.MapFrom(s => s.Request.RequestType))
				.ForMember(d => d.UpdatedOnDateTime, o => o.MapFrom(s => s.UpdatedOn.HasValue
					? DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(s.UpdatedOn.Value, _userTimeZone.TimeZone())
					: null))
				.ForMember(d => d.DateFromYear,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetYear(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.DateFromMonth,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetMonth(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.DateFromDayOfMonth,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.DateToYear,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetYear(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.DateToMonth,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetMonth(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.DateToDayOfMonth,
					o =>
						o.MapFrom(
							s =>
								CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(
									TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.TimeZone()))))
				.ForMember(d => d.Payload, o => o.MapFrom(s => s.Request.RequestPayloadDescription.Name))
				.ForMember(d => d.PayloadId, o => o.ResolveUsing(s => resolvePayloadId(s)))
				.ForMember(d => d.IsFullDay, o => o.ResolveUsing(s => isRequestFullDay(s)))
				.ForMember(d => d.IsCreatedByUser, o => o.MapFrom(s => isCreatedByUser(s.Request, _loggedOnUser)))
				.ForMember(d => d.From,
					o =>
						o.MapFrom(
							s =>
								s.Request.PersonFrom == null
									? string.Empty
									: _personNameProvider.BuildNameFromSetting(s.Request.PersonFrom.Name)))
				.ForMember(d => d.To,
					o =>
						o.MapFrom(
							s =>
								s.Request.PersonTo == null
									? string.Empty
									: _personNameProvider.BuildNameFromSetting(s.Request.PersonTo.Name)))
				.ForMember(d => d.DenyReason, o => o.ResolveUsing(s =>
				{
					Resources.ResourceManager.IgnoreCase = true;
					var result = Resources.ResourceManager.GetString(s.DenyReason);
					if (string.IsNullOrEmpty(result))
					{
						result = s.DenyReason;
					}
					return result;
				}))

				.ForMember(d => d.ExchangeOffer, o => o.ResolveUsing((s =>
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
				})));

			CreateMap<IPersonRequest, Link>()
				.ForMember(d => d.rel, o => o.UseValue("self"))
				.ForMember(d => d.href, o => o.MapFrom(personRequest => personRequest.Id.HasValue
					? _linkProvider.RequestDetailLink(personRequest.Id.Value)
					: null))
				.ForMember(d => d.Methods, o => o.ResolveUsing(getAvailableMethods));
		}

		private object getAvailableMethods (IPersonRequest personRequest)
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

		private bool canCancelPersonRequest (IPersonRequest personRequest)
		{
			if (!personRequest.IsApproved || !_toggleManager.IsEnabled (Toggles.Wfm_Requests_Cancel_Agent_38055))
			{
				return false;
			}


			if (!(personRequest.Request is AbsenceRequest))
			{
				return false;
			}


			var dateforDayCancellationCheck = new DateOnly (personRequest.Request.Period.LocalStartDateTime);
			if ( dateforDayCancellationCheck >= DateOnly.Today)
			{
				if (PrincipalAuthorization.Current().IsPermitted (
					DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, new DateOnly (personRequest.RequestedDate),
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
				: ((IAbsenceRequest)personRequest.Request).Absence.Id.GetValueOrDefault().ToString();
		}

		private static bool isCreatedByUser(IRequest request, ILoggedOnUser loggedOnUser)
		{
			return request.PersonFrom != null && request.PersonFrom.Equals(loggedOnUser.CurrentUser());
		}
	}
}