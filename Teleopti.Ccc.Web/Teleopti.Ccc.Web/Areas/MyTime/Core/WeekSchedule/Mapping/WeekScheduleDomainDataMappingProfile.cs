using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		private readonly IToggleManager _toggleManager;

		public WeekScheduleDomainDataMappingProfile(IScheduleProvider scheduleProvider, IProjectionProvider projectionProvider, 
			IPersonRequestProvider personRequestProvider, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider, INow now, 
			IAbsenceRequestProbabilityProvider absenceRequestProbabilityProvider, IToggleManager toggleManager)
		{
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_now = now;
			_absenceRequestProbabilityProvider = absenceRequestProbabilityProvider;
			_toggleManager = toggleManager;
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

									var scheduleDays = _scheduleProvider.GetScheduleForPeriod(weekWithPreviousDay).ToList();
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
                                                        //if (endTime.Days > startTime.Days && x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
                                                        //    lateEnd = new TimeSpan(23, 59, 59);
                                                        //else
                                                        //    lateEnd = endTime.Days == 1 ? endTime.Add(new TimeSpan(-1, 0, 0, 0)) : endTime;
                                                        
                                                        //for the day before current week, only if end time crosses midnihgt, 
                                                        //then it is a valid end time to be carried over to first week day (endTime.Days == 1)
                                                        if( x.DateOnlyAsPeriod.DateOnly == firstDayOfWeek.AddDays(-1) && endTime.Days == 1)
                                                            lateEnd = endTime.Add(new TimeSpan(-1, 0, 0, 0));
                                                        else if (x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1) ) //for the days of current week
                                                        {
                                                            //if end time cross midnight, then max. time is of course used, otherwise use the end time as it is
                                                            if (endTime.Days > startTime.Days)                                                             
                                                                lateEnd = new TimeSpan(23, 59, 59);
                                                            else
                                                                lateEnd = endTime;
                                                        }

													}
													if (x.OvertimeAvailablityCollection() == null)
														return lateEnd;
													var overtimeAvailability = x.OvertimeAvailablityCollection().FirstOrDefault();
													if (overtimeAvailability == null)
														return lateEnd;

													TimeSpan lateEndOvertimeAvailability = new TimeSpan(0, 0, 0);
													var overtimeAvailabilityStart = overtimeAvailability.StartTime.Value;
													var overtimeAvailabilityEnd = overtimeAvailability.EndTime.Value;
													
                                                    //if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days && x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))
                                                    //    lateEndOvertimeAvailability = new TimeSpan(23, 59, 59);
                                                    //else
                                                    //    lateEndOvertimeAvailability = overtimeAvailabilityEnd.Days == 1 ? overtimeAvailabilityEnd.Add(new TimeSpan(-1, 0, 0, 0)) : overtimeAvailabilityEnd;
                                                    
                                                    
                                                    //for the day before current week, only if end time of OT Availability crosses midnight (ie., overtimeAvailabilityEnd.Days == 1), 
                                                    //then it is a valid end time to be carried over to the first week day 
                                                    if (x.DateOnlyAsPeriod.DateOnly == firstDayOfWeek.AddDays(-1) && overtimeAvailabilityEnd.Days == 1)
                                                        lateEndOvertimeAvailability = overtimeAvailabilityEnd.Add(new TimeSpan(-1, 0, 0, 0));
                                                    else if (x.DateOnlyAsPeriod.DateOnly != firstDayOfWeek.AddDays(-1))  //for the days of current week
                                                    {
                                                        //if end time of OT Availability crosses midnight, then max. time is of course used, otherwise use its end time as it is
                                                        if (overtimeAvailabilityEnd.Days > overtimeAvailabilityStart.Days)                                                            
                                                            lateEndOvertimeAvailability = new TimeSpan(23, 59, 59);
                                                        else
                                                            lateEndOvertimeAvailability = overtimeAvailabilityEnd;

                                                    }												

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
												let availabilityForDay = requestProbability != null && requestProbability.First(a => a.Date == day).Availability
												let probabilityClass = requestProbability == null ? "" : requestProbability.First(a => a.Date == day).CssClass
												let probabilityText = requestProbability == null ? "" : requestProbability.First(a => a.Date == day).Text
												
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

									var asmPermission =
										_permissionProvider.HasApplicationFunctionPermission(
											DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
									var textRequestPermission =
										_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests);
									var overtimeAvailabilityPermission =
										_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
									var absenceRequestPermission =
										_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
									var absenceReportPermission =
										_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceReport) && _toggleManager.IsEnabled(Toggles.MyTimeWeb_AbsenceReport_31011);
									var shiftTradeBulletinBoardPermission =
										_permissionProvider.HasApplicationFunctionPermission(
											DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
									var shiftExchangePermission =
										_permissionProvider.HasApplicationFunctionPermission(
											DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard) &&
										_toggleManager.IsEnabled(Toggles.MyTimeWeb_ShiftTradeExchangeBulletin_31296);
									var personalAccountPermission =
										_permissionProvider.HasApplicationFunctionPermission(
											DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
									var isCurrentWeek = week.Contains(_now.LocalDateOnly());

									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
												ColorSource = colorSource,
												MinMaxTime = minMaxTime,
												AsmPermission = asmPermission,
												TextRequestPermission = textRequestPermission,
												OvertimeAvailabilityPermission = overtimeAvailabilityPermission,
												AbsenceRequestPermission = absenceRequestPermission,
												AbsenceReportPermission = absenceReportPermission,
												ShiftExchangePermission = shiftExchangePermission,
												ShiftTradeBulletinBoardPermission = shiftTradeBulletinBoardPermission,
												PersonAccountPermission = personalAccountPermission,
												IsCurrentWeek = isCurrentWeek,
											};
								});
		}
	}
}