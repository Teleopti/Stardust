using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMappingProfile : Profile
	{
		private readonly IPermissionProvider _permissionProvider;

		public PersonScheduleViewModelMappingProfile(IPermissionProvider permissionProvider)
		{
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
						var result = new List<PersonScheduleViewModelLayer>();
						var isPermitted = _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
						                                                          DateOnly.Today, s.Person);

						foreach (var layer in layers)
						{
							string startStr;
							if (layer.Start == null)
								startStr = null;
							else
							{
								DateTime start = layer.Start;
								startStr = start == DateTime.MinValue ? null : TimeZoneInfo.ConvertTimeFromUtc(start, s.Person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat();
							}

							var vmLayer = new PersonScheduleViewModelLayer
								{
									Color = isPermitted || !layer.IsAbsenceConfidential
										        ? layer.Color
										        : ConfidentialPayloadValues.DisplayColor.ToHtml(),
									Start = startStr,
									Minutes = (int) layer.Minutes
								};
							result.Add(vmLayer);
						}
						return result;
					}))
				.ForMember(x => x.PersonAbsences, o => o.ResolveUsing(s =>
					{
						var isPermitted = _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
						                                                          DateOnly.Today, s.Person);
						return (from personAbsence in s.PersonAbsences
						        let timeZoneInfo = personAbsence.Person.PermissionInformation.DefaultTimeZone()
						        let startTime =
							        personAbsence.Layer.Period.StartDateTime == DateTime.MinValue
								        ? null
								        : TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.StartDateTime, timeZoneInfo)
								                      .ToFixedDateTimeFormat()
						        let endTime =
							        personAbsence.Layer.Period.EndDateTime == DateTime.MinValue
								        ? null
								        : TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.EndDateTime, timeZoneInfo)
								                      .ToFixedDateTimeFormat()
						        select new PersonScheduleViewModelPersonAbsence
							        {
								        Id = personAbsence.Id.ToString(),
								        Color =
									        isPermitted || !personAbsence.Layer.Payload.Confidential
										        ? personAbsence.Layer.Payload.DisplayColor.ToHtml()
										        : ConfidentialPayloadValues.DisplayColor.ToHtml(),
								        Name =
									        isPermitted || !personAbsence.Layer.Payload.Confidential
										        ? personAbsence.Layer.Payload.Description.Name
										        : ConfidentialPayloadValues.Description.Name,
								        StartTime = startTime,
								        EndTime = endTime
							        }).ToArray();
					}))
				;

			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();

		}
	}
}