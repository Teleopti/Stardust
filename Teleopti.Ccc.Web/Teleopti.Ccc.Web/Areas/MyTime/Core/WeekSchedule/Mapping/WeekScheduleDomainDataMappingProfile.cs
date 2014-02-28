using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

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
		private readonly IAbsenceRequestProbabilityProvider _absenceRequestProbabilityProvider;

		public WeekScheduleDomainDataMappingProfile(IScheduleProvider scheduleProvider, IProjectionProvider projectionProvider, 
			IPersonRequestProvider personRequestProvider, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider, INow now, 
			IAbsenceRequestProbabilityProvider absenceRequestProbabilityProvider)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_now = now;
			_absenceRequestProbabilityProvider = absenceRequestProbabilityProvider;
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
									var requestProbability = _absenceRequestProbabilityProvider.GetAbsenceRequestProbabilityForPeriod(week);
									var earliest =
										scheduleDays.Min(
											x =>
												{
													var period = _projectionProvider.Projection(x).Period();
													var earlyStart = new TimeSpan(23, 59, 59);
													if (period != null && _projectionProvider.Projection(x).HasLayers)
													{
														var startTime = period.Value.TimePeriod(_userTimeZone.TimeZone()).StartTime;
														var endTime = period.Value.TimePeriod(_userTimeZone.TimeZone()).EndTime;
														var localEndDate = new DateOnly(period.Value.EndDateTimeLocal(_userTimeZone.TimeZone()).Date);
														if (endTime.Days > startTime.Days && week.Contains(localEndDate))
															earlyStart = TimeSpan.Zero;
														else if (x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
															earlyStart = startTime;
													}

													if (x.OvertimeAvailablityCollection() == null)
														return earlyStart;
													var overtimeAvailability = x.OvertimeAvailablityCollection().FirstOrDefault();
													if (overtimeAvailability == null)
														return earlyStart;
													var earlyStartOvertimeAvailability = new TimeSpan(23, 59, 59);
													var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
													var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;
													if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days && week.Contains(x.DateOnlyAsPeriod.DateOnly.AddDays(1)))
														earlyStartOvertimeAvailability = TimeSpan.Zero;
													else if (x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
														earlyStartOvertimeAvailability = overtimeAvailabilityStart;
													return earlyStart < earlyStartOvertimeAvailability ? earlyStart : earlyStartOvertimeAvailability;
												});

									var latest =
										scheduleDays.Max(
											x =>
												{
													var period = _projectionProvider.Projection(x).Period();
													var lateEnd = new TimeSpan(0, 0, 0);
													if (period != null && _projectionProvider.Projection(x).HasLayers)
													{
														var startTime = period.Value.TimePeriod(_userTimeZone.TimeZone()).StartTime;
														var endTime = period.Value.TimePeriod(_userTimeZone.TimeZone()).EndTime;
														if (endTime.Days > startTime.Days && x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
															lateEnd = new TimeSpan(23, 59, 59);
														else
															lateEnd = endTime.Days == 1 ? endTime.Add(new TimeSpan(-1, 0, 0, 0)) : endTime;
													}
													if (x.OvertimeAvailablityCollection() == null)
														return lateEnd;
													var overtimeAvailability = x.OvertimeAvailablityCollection().FirstOrDefault();
													if (overtimeAvailability == null)
														return lateEnd;

													TimeSpan lateEndOvertimeAvailability;
													var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
													var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;
													if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days && x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
														lateEndOvertimeAvailability = new TimeSpan(23, 59, 59);
													else
														lateEndOvertimeAvailability = overtimeAvailabilityEnd.Days == 1 ? overtimeAvailabilityEnd.Add(new TimeSpan(-1, 0, 0, 0)) : overtimeAvailabilityEnd;

													return lateEnd > lateEndOvertimeAvailability ? lateEnd : lateEndOvertimeAvailability;
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
												let overtimeAvailability = scheduleDay == null || scheduleDay.OvertimeAvailablityCollection() == null ? null : scheduleDay.OvertimeAvailablityCollection().FirstOrDefault()
												let overtimeAvailabilityYesterday = scheduleYesterday == null || scheduleYesterday.OvertimeAvailablityCollection() == null ? null : scheduleYesterday.OvertimeAvailablityCollection().FirstOrDefault()
												let personRequestsForDay = personRequests == null ? null : (from i in personRequests where TimeZoneInfo.ConvertTimeFromUtc(i.Request.Period.StartDateTime, _userTimeZone.TimeZone()).Date == day select i).ToArray()
												let availabilityForDay = requestProbability != null && requestProbability.First(a => a.Item1 == day).Item4
												let probabilityClass = requestProbability == null ? "" : requestProbability.First(a => a.Item1 == day).Item2
												let probabilityText = requestProbability == null ? "" : requestProbability.First(a => a.Item1 == day).Item3
												
												select new WeekScheduleDayDomainData
														{
															Date = new DateOnly(day),
															PersonRequests = personRequestsForDay,
															Projection = projection,
															ProjectionYesterday = projectionYesterday,
															OvertimeAvailability = overtimeAvailability,
															OvertimeAvailabilityYesterday = overtimeAvailabilityYesterday,
															ScheduleDay = scheduleDay,
															MinMaxTime = minMaxTime,
															Availability = availabilityForDay,
															ProbabilityClass = probabilityClass,
															ProbabilityText = probabilityText
														}
											   ).ToArray();

									var colorSource = new ScheduleColorSource
														{
															ScheduleDays = scheduleDays,
															Projections = (from d in days where d.Projection != null select d.Projection).ToArray()
														};

									var asmPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
                                    var underConstructionPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.UnderConstruction);
                                    var monthSchedulerPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MonthSchedule);
                                    var absenceRequestPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
									var isCurrentWeek = week.Contains(_now.LocalDateOnly());

									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
												ColorSource = colorSource,
												MinMaxTime = minMaxTime,
												AsmPermission = asmPermission,
                                                UnderConstructionPermission = underConstructionPermission,
                                                MonthSchedulePermission = monthSchedulerPermission,
												AbsenceRequestPermission = absenceRequestPermission,
												IsCurrentWeek = isCurrentWeek
											};
								});
		}
	}
}