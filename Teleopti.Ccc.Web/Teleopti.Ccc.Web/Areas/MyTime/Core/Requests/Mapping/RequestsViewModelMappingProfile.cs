using System;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class RequestsViewModelMappingProfile : Profile
	{
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly Func<ILinkProvider> _linkProvider;

		public RequestsViewModelMappingProfile(Func<IUserTimeZone> userTimeZone, Func<ILinkProvider> linkProvider)
		{
			_userTimeZone = userTimeZone;
			_linkProvider = linkProvider;
		}

		protected override void Configure()
		{
			CreateMap<IPersonRequest, RequestViewModel>()
				.ForMember(d => d.Link, o => o.MapFrom(s => s))
				.ForMember(d => d.Subject, o => o.MapFrom(s => s.GetSubject(new NoFormatting())))
				.ForMember(d => d.Dates, o => o.MapFrom(s => s.Request.Period.ToShortDateTimeString(_userTimeZone.Invoke().TimeZone())))
				.ForMember(d => d.Status, o => o.MapFrom(s => s.StatusText))
				.ForMember(d => d.Text, o => o.MapFrom(s => s.GetMessage(new NoFormatting())))
				.ForMember(d => d.Type, o => o.MapFrom(s => s.Request.RequestTypeDescription))
				.ForMember(d => d.UpdatedOn, o => o.MapFrom(s => s.UpdatedOn.HasValue
																					? _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(s.UpdatedOn.Value).ToShortDateTimeString()
																					: null))
				.ForMember(d => d.RawDateFrom, o => o.MapFrom(s => _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(s.Request.Period.StartDateTime).ToShortDateString()))
				.ForMember(d => d.RawDateTo, o => o.MapFrom(s => _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(s.Request.Period.EndDateTime).ToShortDateString()))
				.ForMember(d => d.RawTimeFrom, o => o.MapFrom(s => _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(s.Request.Period.StartDateTime).ToShortTimeString()))
				.ForMember(d => d.RawTimeTo, o => o.MapFrom(s => _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(s.Request.Period.EndDateTime).ToShortTimeString()))

				;

			CreateMap<IPersonRequest, Link>()
				.ForMember(d => d.rel, o => o.UseValue("self"))
				.ForMember(d => d.href, o => o.MapFrom(s => s.Id.HasValue ? 
																			_linkProvider.Invoke().TextRequestLink(s.Id.Value) : 
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
	}
}