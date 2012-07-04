using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDomainDataMappingProfile : Profile
	{
		private readonly IResolve<IScheduleProvider> _scheduleProvider;
		private readonly IResolve<IProjectionProvider> _projectionProvider;
		private readonly IResolve<IPersonRequestProvider> _personRequestProvider;
		private readonly IResolve<IUserTimeZone> _userTimeZone;

		public WeekScheduleDomainDataMappingProfile(IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider, IResolve<IPersonRequestProvider> personRequestProvider, IResolve<IUserTimeZone> userTimeZone)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, WeekScheduleDomainData>()
				.ConvertUsing(s =>
								{
									var date = s;
									var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(s, CultureInfo.CurrentCulture));
									var week = new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(6));

									var scheduleDays = _scheduleProvider.Invoke().GetScheduleForPeriod(week) ?? new IScheduleDay[] { };
									var personRequests = _personRequestProvider.Invoke().RetrieveRequests(week);

									var earliest =
										scheduleDays.Min(
											x =>
												{
													var period = _projectionProvider.Invoke().Projection(x).Period();
													return (period != null && _projectionProvider.Invoke().Projection(x).HasLayers
													        	? period.Value.TimePeriod(
													        		TeleoptiPrincipal.Current.Regional.TimeZone).StartTime
													        	: new TimeSpan(23, 59, 59));
												});
									var latest =
										scheduleDays.Max(
											x =>
												{
													var period = _projectionProvider.Invoke().Projection(x).Period();
													return (period != null && _projectionProvider.Invoke().Projection(x).HasLayers
													        	? period.Value.TimePeriod(
													        		TeleoptiPrincipal.Current.Regional.TimeZone).EndTime
													        	: new TimeSpan(0, 0, 0));
												});
									TimePeriod minMaxTime = earliest > latest
									                        	? new TimePeriod(latest, earliest)
									                        	: new TimePeriod(earliest, latest);

									var days = (from day in firstDayOfWeek.Date.DateRange(7)
												let scheduleDay = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day)
												let projection = scheduleDay == null ? null : _projectionProvider.Invoke().Projection(scheduleDay)
												let personRequestsForDay = personRequests == null ? null : (from i in personRequests where _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(i.Request.Period.StartDateTime).Date == day select i).ToArray()
												select new WeekScheduleDayDomainData
														{
															Date = new DateOnly(day),
															PersonRequests = personRequestsForDay,
															Projection = projection,
															ScheduleDay = scheduleDay,
															MinMaxTime = minMaxTime,
														}
											   ).ToArray();

									var colorSource = new ScheduleColorSource
									                  	{
															ScheduleDays = scheduleDays,
															Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
									                  	};
									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
												ColorSource = colorSource,
												MinMaxTime = minMaxTime
											};
								});

		}
	}
}