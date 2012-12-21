using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
		private readonly IResolve<IPermissionProvider> _permissionProvider;
		private readonly IResolve<INow> _now;

		public WeekScheduleDomainDataMappingProfile(IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider, IResolve<IPersonRequestProvider> personRequestProvider, IResolve<IUserTimeZone> userTimeZone, IResolve<IPermissionProvider> permissionProvider, IResolve<INow> now)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_now = now;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, WeekScheduleDomainData>()
				.ConvertUsing(s =>
								{
									var date = s;
									var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
									var week = new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(6));
                                    var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));

                                    var scheduleDays = _scheduleProvider.Invoke().GetScheduleForPeriod(weekWithPreviousDay) ?? new IScheduleDay[] { };
									var personRequests = _personRequestProvider.Invoke().RetrieveRequests(week);

									var earliest =
										scheduleDays.Min(
											x =>
												{
													var period = _projectionProvider.Invoke().Projection(x).Period();
												   	var earlyStart = new TimeSpan(23, 59, 59);
													if (period != null && _projectionProvider.Invoke().Projection(x).HasLayers)
													{
														var startTime = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).StartTime;
														var endTime = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).EndTime;
														var localEndDate =
															new DateOnly(period.Value.EndDateTimeLocal(
																TeleoptiPrincipal.Current.Regional.TimeZone).Date);
														if (endTime.Days > startTime.Days && week.Contains(localEndDate))
															earlyStart = TimeSpan.Zero;
														else if (x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
															earlyStart = startTime;
													}
												    return earlyStart;
												});
									var latest =
										scheduleDays.Max(
											x =>
												{
													var period = _projectionProvider.Invoke().Projection(x).Period();
												    var lateEnd = new TimeSpan(0, 0, 0);
													if (period != null && _projectionProvider.Invoke().Projection(x).HasLayers)
													{
														var startTime = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).StartTime;
														var endTime = period.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).EndTime;
														if (endTime.Days > startTime.Days && x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
															lateEnd = new TimeSpan(23, 59, 59);
														else
														{
															lateEnd = endTime.Days == 1 ? endTime.Add(new TimeSpan(-1, 0, 0, 0)) : endTime;
														}
													}
													return lateEnd;
												});
									
									const int margin = 15;
									var early = earliest;
									var late = latest;

									if (early > late)
									{
										early = latest;
										late = earliest;
									}

									if (early.Ticks > TimeSpan.Zero.Add(new TimeSpan(0, margin, 0)).Ticks)
										early = early.Subtract(new TimeSpan(0, margin, 0));
									else
									{
										early = TimeSpan.Zero;
									}

									if (late.Ticks < new TimeSpan(23, 59, 59).Subtract(new TimeSpan(0, margin, 0)).Ticks)
										late = late.Add(new TimeSpan(0, margin, 0));
									else
									{
										late = new TimeSpan(23, 59, 59);
									}

									var minMaxTime = new TimePeriod(early, late);

									var days = (from day in firstDayOfWeek.Date.DateRange(7)
												let scheduleDay = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day)
												let scheduleYesterday = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day.AddDays(-1))
												let projection = scheduleDay == null ? null : _projectionProvider.Invoke().Projection(scheduleDay)
                                                let projectionYesterday = scheduleYesterday == null ? null : _projectionProvider.Invoke().Projection(scheduleYesterday)
                                                let personRequestsForDay = personRequests == null ? null : (from i in personRequests where TimeZoneInfo.ConvertTimeFromUtc(i.Request.Period.StartDateTime, _userTimeZone.Invoke().TimeZone()).Date == day select i).ToArray()
												select new WeekScheduleDayDomainData
														{
															Date = new DateOnly(day),
															PersonRequests = personRequestsForDay,
															Projection = projection,
															ProjectionYesterday = projectionYesterday,
															ScheduleDay = scheduleDay,
															MinMaxTime = minMaxTime,
														}
											   ).ToArray();

									var colorSource = new ScheduleColorSource
									                  	{
															ScheduleDays = scheduleDays,
															Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
									                  	};

									bool asmPermission = _permissionProvider.Invoke().HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
									bool isCurrentWeek = week.Contains(_now.Invoke().DateOnly());

									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
												ColorSource = colorSource,
												MinMaxTime = minMaxTime,
												AsmPermission = asmPermission,
												IsCurrentWeek = isCurrentWeek
											};
								});

		}
	}
}