using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;
using IAllowanceProvider = Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider.IAllowanceProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDomainDataMappingProfile : Profile
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IProjectionProvider _projectionProvider;
		private readonly IPersonRequestProvider _personRequestProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPermissionProvider _permissionProvider;
		private readonly INow _now;
		private readonly IResolve<IAllowanceProvider> _allowanceProvider;
		private readonly IResolve<IAbsenceTimeRandomProvider> _absenceTimeProvider;

		public WeekScheduleDomainDataMappingProfile(IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider, IResolve<IPersonRequestProvider> personRequestProvider, IResolve<IUserTimeZone> userTimeZone, IResolve<IPermissionProvider> permissionProvider, IResolve<INow> now, IResolve<IAllowanceProvider> allowanceProvider, IResolve<IAbsenceTimeRandomProvider> absenceTimeProvider)
		//public WeekScheduleDomainDataMappingProfile(IResolve<IScheduleProvider> scheduleProvider, IResolve<IProjectionProvider> projectionProvider, IResolve<IPersonRequestProvider> personRequestProvider, IResolve<IUserTimeZone> userTimeZone, IResolve<IPermissionProvider> permissionProvider, IResolve<INow> now, IResolve<IAllowanceProvider> allowanceProvider)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_now = now;
			_allowanceProvider = allowanceProvider;
			_absenceTimeProvider = absenceTimeProvider;
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

                                    var scheduleDays = _scheduleProvider.GetScheduleForPeriod(weekWithPreviousDay) ?? new IScheduleDay[] { };
									var personRequests = _personRequestProvider.RetrieveRequests(week);
									var allowanceCollection = _allowanceProvider.Invoke().GetAllowanceForPeriod(week);
									var absenceTimeCollection = _absenceTimeProvider.Invoke().GetAbsenceTimeForPeriod(week);

									var earliest =
										scheduleDays.Min(
											x =>
												{
													var period = _projectionProvider.Projection(x).Period();
												   	var earlyStart = new TimeSpan(23, 59, 59);
													if (period != null && _projectionProvider.Projection(x).HasLayers)
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
													var period = _projectionProvider.Projection(x).Period();
												    var lateEnd = new TimeSpan(0, 0, 0);
													if (period != null && _projectionProvider.Projection(x).HasLayers)
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

									early = early.Ticks > TimeSpan.Zero.Add(new TimeSpan(0, margin, 0)).Ticks ? early.Subtract(new TimeSpan(0, margin, 0)) : TimeSpan.Zero;
									late = late.Ticks < new TimeSpan(23, 59, 59).Subtract(new TimeSpan(0, margin, 0)).Ticks ? late.Add(new TimeSpan(0, margin, 0)) : new TimeSpan(23, 59, 59);

									var minMaxTime = new TimePeriod(early, late);

									var days = (from day in firstDayOfWeek.Date.DateRange(7)
												let scheduleDay = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day)
												let scheduleYesterday = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day.AddDays(-1))
												let projection = scheduleDay == null ? null : _projectionProvider.Projection(scheduleDay)
                                                let projectionYesterday = scheduleYesterday == null ? null : _projectionProvider.Projection(scheduleYesterday)
                                                let personRequestsForDay = personRequests == null ? null : (from i in personRequests where TimeZoneInfo.ConvertTimeFromUtc(i.Request.Period.StartDateTime, _userTimeZone.TimeZone()).Date == day select i).ToArray()
												let allowanceForDay = allowanceCollection == null ? 0 : allowanceCollection.First(a=>a.Date==day).Allowance
												let absenceTimeForDay = absenceTimeCollection == null ? 0 : absenceTimeCollection.First(a => a.Date == day).AbsenceTime
												
												select new WeekScheduleDayDomainData
														{
															Date = new DateOnly(day),
															PersonRequests = personRequestsForDay,
															Projection = projection,
															ProjectionYesterday = projectionYesterday,
															ScheduleDay = scheduleDay,
															MinMaxTime = minMaxTime,
															Allowance = allowanceForDay,
															AbsenceTime = absenceTimeForDay
														}
											   ).ToArray();

									var colorSource = new ScheduleColorSource
									                  	{
															ScheduleDays = scheduleDays,
															Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
									                  	};

									var asmPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
									var absenceRequestPermission = _permissionProvider.Invoke().HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);

									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
												ColorSource = colorSource,
												MinMaxTime = minMaxTime,
												AsmPermission = asmPermission,
												AbsenceRequestPermission = absenceRequestPermission,
												IsCurrentWeek = isCurrentWeek
											};
								});

		}
	}
}