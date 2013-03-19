using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class RequestsViewModelMappingProfile : Profile
	{
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly Func<ILinkProvider> _linkProvider;
		private readonly Func<ILoggedOnUser> _loggedOnUser;
		private readonly IResolve<IShiftTradeRequestStatusChecker> _shiftTradeRequestStatusChecker;

		public RequestsViewModelMappingProfile(Func<IUserTimeZone> userTimeZone,
																			Func<ILinkProvider> linkProvider,
																			Func<ILoggedOnUser> loggedOnUser,
																			IResolve<IShiftTradeRequestStatusChecker> shiftTradeRequestStatusChecker)
		{
			_userTimeZone = userTimeZone;
			_linkProvider = linkProvider;
			_loggedOnUser = loggedOnUser;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
		}

		protected override void Configure()
		{
			CreateMap<IPersonRequest, RequestViewModel>()
				.ForMember(d => d.Link, o => o.MapFrom(s => s))
				.ForMember(d => d.Subject, o => o.MapFrom(s => s.GetSubject(new NoFormatting())))
				.ForMember(d => d.Dates,
									 o => o.MapFrom(s => s.Request.Period.ToShortDateTimeString(_userTimeZone.Invoke().TimeZone())))
				.ForMember(d => d.Status, o => o.MapFrom(s =>
					{
						var ret = s.StatusText;
						if (s.IsPending)
						{
							var shiftTradeRequest = s.Request as IShiftTradeRequest;
							if (shiftTradeRequest != null)
							{
								ret += ", " + shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker.Invoke()).ToText(isCreatedByUser(s.Request, _loggedOnUser));
							}							
						}
						return ret;
					}))
				.ForMember(d => d.Text, o => o.MapFrom(s => s.GetMessage(new NoFormatting())))
				.ForMember(d => d.Type, o => o.MapFrom(s => s.Request.RequestTypeDescription))
				.ForMember(d => d.TypeEnum, o => o.MapFrom(s => s.Request.RequestType))
				.ForMember(d => d.UpdatedOn, o => o.MapFrom(s => s.UpdatedOn.HasValue
																													? TimeZoneInfo.ConvertTimeFromUtc(s.UpdatedOn.Value, _userTimeZone.Invoke().TimeZone()).ToShortDateTimeString()
																													: null))
				.ForMember(d => d.DateFromYear, 
									o => 
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetYear(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.DateFromMonth,
									o =>
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.DateFromDayOfMonth,
									o =>
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.DateToYear,
									o =>
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetYear(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.DateToMonth,
									o =>
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.DateToDayOfMonth,
									o =>
									o.MapFrom(
									s =>
														CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.Invoke().TimeZone()))))
				.ForMember(d => d.RawTimeFrom,
									 o =>
									 o.MapFrom(
										s =>
														TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone()).ToShortTimeString()))
				.ForMember(d => d.RawTimeTo,
									 o =>
									 o.MapFrom(
										s =>
																TimeZoneInfo.ConvertTimeFromUtc(s.Request.Period.EndDateTime, _userTimeZone.Invoke().TimeZone()).ToShortTimeString()))
				.ForMember(d => d.Payload, o => o.MapFrom(s => s.Request.RequestPayloadDescription.Name))
				.ForMember(d => d.IsFullDay, o => o.MapFrom(s =>
																											{
																												var start =
																													TimeZoneInfo.ConvertTimeFromUtc(
																																						s.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone());
																												var end =
																													TimeZoneInfo.ConvertTimeFromUtc(
																																						s.Request.Period.EndDateTime, _userTimeZone.Invoke().TimeZone());
																												var allDayEndDateTime = start.AddDays(1).AddMinutes(-1);
																												return start.TimeOfDay == TimeSpan.Zero &&
																															 end.TimeOfDay == allDayEndDateTime.TimeOfDay;
																											}))
				.ForMember(d => d.IsCreatedByUser, o => o.MapFrom(s => isCreatedByUser(s.Request,_loggedOnUser)))
				.ForMember(d => d.From, o => o.MapFrom(s => s.Request.PersonFrom == null ? string.Empty : s.Request.PersonFrom.Name.ToString()))
				.ForMember(d => d.To, o => o.MapFrom(s => s.Request.PersonTo == null ? string.Empty : s.Request.PersonTo.Name.ToString()))
				.ForMember(d => d.DenyReason, o => o.MapFrom(s =>
																											{
																												Resources.ResourceManager.IgnoreCase = true;
				                                             	    var result =  Resources.ResourceManager.GetString(s.DenyReason);
																												if (string.IsNullOrEmpty(result))
																												{
																													result = s.DenyReason;
																												}
																												return result;
																											}))
				;

			CreateMap<IPersonRequest, Link>()
				.ForMember(d => d.rel, o => o.UseValue("self"))
				.ForMember(d => d.href, o => o.MapFrom(s => s.Id.HasValue ?
																			_linkProvider.Invoke().RequestDetailLink(s.Id.Value) :
																			null))
				.ForMember(d => d.Methods, o => o.MapFrom(s =>
																			{
																				var stateId = PersonRequest.GetUnderlyingStateId(s);
																				//0: Pending
																				//1: Denied
																				//2: Approved
																				//3: New
																				return new[] { 0, 3 }.Contains(stateId) ? "GET, DELETE, PUT" : "GET";
																			}))
				;
		}

		private static bool isCreatedByUser(IRequest request, Func<ILoggedOnUser> loggedOnUser)
		{
			return  request.PersonFrom  == null || request.PersonFrom == loggedOnUser.Invoke().CurrentUser();
		}
	}
}