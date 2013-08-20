using System;
using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMappingProfile : Profile
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;

		public PersonScheduleViewModelMappingProfile(ILoggedOnUser loggedOnUser, IPermissionProvider permissionProvider)
		{
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
		}

		protected override void Configure()
		{
			CreateMap<PersonScheduleData, PersonScheduleViewModel>()
				.ForMember(x => x.Name, o => o.MapFrom(s => s.Person.Name.ToString()))
				.ForMember(x => x.Team, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Description.Name))
				.ForMember(x => x.Site, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Site.Description.Name))
				.ForMember(x => x.Layers, o => o.ResolveUsing(s =>
					{
						if (s.Shift == null)
							return null;
						var layers = s.Shift.Projection as IEnumerable<dynamic>;
						layers.ForEach(l =>
							{
								if (s.Person == null)
									return;
								l.TimeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
							});
						return layers;
					}))
				;

			CreateMap<IPersonAbsence, PersonScheduleViewModelPersonAbsence>()
				.ForMember(x => x.Color, o => o.ResolveUsing(s =>
					{
						var isPermitted = _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
						                                                          DateOnly.Today,
						                                                          _loggedOnUser.CurrentUser());
						return isPermitted || !s.Layer.Payload.Confidential
							       ? s.Layer.Payload.DisplayColor.ToHtml()
							       : ConfidentialPayloadValues.DisplayColor.ToHtml();
					}))
				.ForMember(x => x.Name, o => o.ResolveUsing(s =>
					{
						var isPermitted = _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
						                                                          DateOnly.Today,
						                                                          _loggedOnUser.CurrentUser());
						return isPermitted || !s.Layer.Payload.Confidential
							       ? s.Layer.Payload.Description.Name
							       : ConfidentialPayloadValues.Description.Name;
					}))
				.ForMember(x => x.StartTime, o => o.ResolveUsing(s =>
					{
						if (s.Layer.Period.StartDateTime == DateTime.MinValue)
							return null;
						TimeZoneInfo timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
						return TimeZoneInfo.ConvertTimeFromUtc(s.Layer.Period.StartDateTime, timeZoneInfo).ToFixedDateTimeFormat();
					}))
				.ForMember(x => x.EndTime, o => o.ResolveUsing(s =>
					{
						if (s.Layer.Period.EndDateTime == DateTime.MinValue)
							return null;
						TimeZoneInfo timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
						return TimeZoneInfo.ConvertTimeFromUtc(s.Layer.Period.EndDateTime, timeZoneInfo).ToFixedDateTimeFormat();
					}))
				;

			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();

			CreateMap<dynamic, PersonScheduleViewModelLayer>()
				.ForMember(x => x.Color, o => o.ResolveUsing(s =>
					{
						var isPermitted =
							_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
							                                        DateOnly.Today,
							                                        _loggedOnUser.CurrentUser());
						return isPermitted || !s.IsAbsenceConfidential
							       ? s.Color
							       : ConfidentialPayloadValues.DisplayColor.ToHtml();
					}))
				.ForMember(x => x.Start, o => o.ResolveUsing(s =>
					{
						if (s.Start == null)
							return null;
						DateTime start = s.Start;
						if (start == DateTime.MinValue)
							return null;
						TimeZoneInfo timeZoneInfo = s.TimeZoneInfo;
						return TimeZoneInfo.ConvertTimeFromUtc(start, timeZoneInfo).ToFixedDateTimeFormat();
					}))
				.ForMember(x => x.Minutes, o => o.ResolveUsing(s => s.Minutes))
				;

		}
	}
}