using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleDomainDataMapper
	{
		private readonly ISchedulePersonProvider _personProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionForMTWProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZone;

		public TeamScheduleDomainDataMapper(ISchedulePersonProvider personProvider, IScheduleProvider scheduleProvider, ITeamScheduleProjectionForMTWProvider projectionProvider, IUserTimeZone userTimeZone)
		{
			_personProvider = personProvider;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_userTimeZone = userTimeZone;
		}

		public TeamScheduleDomainData Map(DateOnly date, Guid teamOrGroupId)
		{
			var persons = _personProvider.GetPermittedPersonsForGroup(date, teamOrGroupId, DefinedRaptorApplicationFunctionPaths.ViewSchedules) ?? new IPerson[0];
			var scheduleDays = (_scheduleProvider.GetScheduleForPersons(date, persons) ?? new IScheduleDay[0]).ToLookup(s => s.Person);
			
			var days = (from p in persons let scheduleDay = scheduleDays[p].FirstOrDefault() select map(new Tuple<IPerson, IScheduleDay>(p, scheduleDay))).OrderBy(d => d.Projection?.SortDate ?? DateTime.MaxValue).ToArray();
			
			var periods = (from day in days
						   let projection = day.Projection
						   where projection?.Layers != null
						   from l in projection.Layers
						   where l.Period.HasValue
						   select l.Period.Value).ToArray();

			var minLocal = date.Date;
			var maxLocal = date.Date;
			minLocal = minLocal.Add(TeamScheduleDomainData.DefaultDisplayTime.StartTime);
			maxLocal = maxLocal.Date.Add(TeamScheduleDomainData.DefaultDisplayTime.EndTime);

			var timeZone = _userTimeZone.TimeZone();
			var min = timeZone.SafeConvertTimeToUtc(minLocal);
			var max = timeZone.SafeConvertTimeToUtc(maxLocal);

			if (periods.Any())
			{
				min = periods.Select(p => p.StartDateTime).Min();
				max = periods.Select(p => p.EndDateTime).Max();
			}
			var timePeriod = new DateTimePeriod(min, max);
			var displayTimePeriod = roundToWholeQuarters(timePeriod);

			Array.ForEach(days, day => day.DisplayTimePeriod = displayTimePeriod);

			return new TeamScheduleDomainData
			{
				Date = date,
				TeamOrGroupId = teamOrGroupId,
				DisplayTimePeriod = displayTimePeriod,
				Days = days
			};
		}

		private TeamScheduleDayDomainData map(Tuple<IPerson, IScheduleDay> s)
		{
			return new TeamScheduleDayDomainData
			{
				Person = s.Item1,
				Projection = s.Item2 == null ? null : _projectionProvider.Projection(s.Item2),
				HasDayOffUnder = s.Item2 != null && s.Item2.SignificantPartForDisplay() == SchedulePartView.ContractDayOff,
			};
		}
		
		private static DateTimePeriod roundToWholeQuarters(DateTimePeriod displayedTimePeriod)
		{
			var startTime = displayedTimePeriod.StartDateTime;
			var startMinutes = (startTime.Minute - 15) / 15 * 15;
			startTime = startTime.Subtract(TimeSpan.FromMinutes(startTime.Minute)).AddMinutes(startMinutes);

			var endTime = displayedTimePeriod.EndDateTime;
			var endMinutes = (endTime.Minute + 29) / 15 * 15;
			endTime = endTime.Subtract(TimeSpan.FromMinutes(endTime.Minute)).AddMinutes(endMinutes);

			return new DateTimePeriod(startTime, endTime);
		}
	}
}