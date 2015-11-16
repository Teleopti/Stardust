using System;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleDomainDataMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<ISchedulePersonProvider> _personProvider;
		private readonly Func<IScheduleProvider> _scheduleProvider;
		private readonly Func<ITeamScheduleProjectionForMTWProvider> _projectionProvider;
		private readonly Func<IUserTimeZone> _userTimeZone;

		public TeamScheduleDomainDataMappingProfile(Func<IMappingEngine> mapper, Func<ISchedulePersonProvider> personProvider, Func<IScheduleProvider> scheduleProvider, Func<ITeamScheduleProjectionForMTWProvider> projectionProvider, Func<IUserTimeZone> userTimeZone)
		{
			_mapper = mapper;
			_personProvider = personProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			// entry for testing purposes
			CreateMap<DateOnly, TeamScheduleDomainData>()
				.ConvertUsing(s => _mapper().Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(s, Guid.Empty)))
				;



			CreateMap<Tuple<DateOnly, Guid>, TeamScheduleDomainData>()
				.ForMember(d => d.Date, o => o.MapFrom(source => source.Item1))
				.ForMember(d => d.DisplayTimePeriod, o => o.Ignore())
				.ForMember(d => d.TeamOrGroupId, o => o.MapFrom(s => s.Item2))
				.ForMember(d => d.Days, o => o.ResolveUsing(source =>
				                                       	{
															var date = source.Item1;
															var teamOrGroupId = source.Item2;
															var persons = _personProvider().GetPermittedPersonsForGroup(date, teamOrGroupId, DefinedRaptorApplicationFunctionPaths.ViewSchedules);
															var scheduleDays = _scheduleProvider().GetScheduleForPersons(date, persons);

															if (scheduleDays == null)
																scheduleDays = new IScheduleDay[] {};
															if (persons == null)
																persons = new IPerson[] {};

															return (from p in persons
				                                       		       let scheduleDay = (from s in scheduleDays where s.Person == p select s).SingleOrDefault()
				                                       		       select new Tuple<IPerson, IScheduleDay>(p, scheduleDay)).ToArray();
				                                       	}))
				.AfterMap((source, target) =>
				          	{
				          		target.Days = target.Days.OrderBy(d => d.Projection == null ? DateTime.MaxValue : d.Projection.SortDate).ToArray();
				          		target.DisplayTimePeriod = _mapper().Map<TeamScheduleDomainData, DateTimePeriod>(target);
				          		target.Days.ForEach(day => day.DisplayTimePeriod = target.DisplayTimePeriod);
				          	})
				;

			CreateMap<Tuple<IPerson, IScheduleDay>, TeamScheduleDayDomainData>()
				.ForMember(d => d.Person, o => o.MapFrom(s => s.Item1))
				.ForMember(d => d.Projection, o => o.MapFrom(s => s.Item2 == null ? null : _projectionProvider().Projection(s.Item2)))
				.ForMember(d => d.DisplayTimePeriod, o => o.Ignore())
				.ForMember(d => d.HasDayOffUnder, o => o.MapFrom(s => s.Item2 != null && s.Item2.SignificantPartForDisplay() == SchedulePartView.ContractDayOff))
				;

			CreateMap<TeamScheduleDomainData, DateTimePeriod>()
				.ConstructUsing(s =>
				                	{
				                		var periods = (from day in s.Days
				                		               let projection = day.Projection
				                		               where projection != null
														   && projection.Layers != null
				                		               from l in projection.Layers
				                		               where l.Period.HasValue
				                		               select l.Period.Value)
				                			.ToArray();

				                		var minLocal = s.Date.Date;
				                		var maxLocal = s.Date.Date;
				                		minLocal = minLocal.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime);
				                		maxLocal = maxLocal.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime);

				                		var timeZone = _userTimeZone().TimeZone();
				                		var min = timeZone.SafeConvertTimeToUtc(minLocal);
				                		var max = timeZone.SafeConvertTimeToUtc(maxLocal);

				                		if (periods.Any())
				                		{
				                			min = periods.Select(p => p.StartDateTime).Min();
				                			max = periods.Select(p => p.EndDateTime).Max();
				                		}
				                		var timePeriod = new DateTimePeriod(min, max);
				                		return roundToWholeQuarters(timePeriod);
				                	});
		}

		private static DateTimePeriod roundToWholeQuarters(DateTimePeriod displayedTimePeriod)
		{
			var startTime = displayedTimePeriod.StartDateTime;
			var startMinutes = ((startTime.Minute - 15) / 15) * 15;
			startTime = startTime.Subtract(TimeSpan.FromMinutes(startTime.Minute)).AddMinutes(startMinutes);

			var endTime = displayedTimePeriod.EndDateTime;
			var endMinutes = ((endTime.Minute + 29) / 15) * 15;
			endTime = endTime.Subtract(TimeSpan.FromMinutes(endTime.Minute)).AddMinutes(endMinutes);

			return new DateTimePeriod(startTime, endTime);
		}
	}
}