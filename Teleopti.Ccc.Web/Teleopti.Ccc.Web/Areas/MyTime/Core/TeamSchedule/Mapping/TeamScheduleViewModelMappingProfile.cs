using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleViewModelMappingProfile : Profile
	{
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly ICreateHourText _createHourText;
		private readonly ITeamScheduleBadgeProvider _badgeProvider;

		public TeamScheduleViewModelMappingProfile(Func<IUserTimeZone> userTimeZone, ICreateHourText createHourText, ITeamScheduleBadgeProvider badgeProvider)
		{
			_userTimeZone = userTimeZone;
			_createHourText = createHourText;
			_badgeProvider = badgeProvider;
		}

		private class TimeLineMappingData
		{
			public DateTime Time { get; set; }
			public DateTimePeriod DisplayTimePeriod { get; set; }
		}

		private class LayerMappingData
		{
			public DateTimePeriod DisplayTimePeriod { get; set; }
			public ITeamScheduleLayer Layer { get; set; }
		}

		protected override void Configure()
		{
			CreateMap<TeamScheduleDomainData, TeamScheduleViewModel>()
				.ForMember(d=>d.ShiftTradePermisssion, o => o.UseValue(false))
				.ForMember(d=>d.AgentBadgeEnabled, o => o.UseValue(false))
				.ForMember(d => d.AgentSchedules, o => o.MapFrom(s => s.Days))
				.ForMember(d => d.PeriodSelection, o => o.MapFrom(s => s))
				.ForMember(d => d.TeamSelection, o => o.MapFrom(s => s.TeamOrGroupId))
				.ForMember(d => d.TimeLine, o => o.ResolveUsing(s =>
				                                           	{
				                                           		var startTime = s.DisplayTimePeriod.StartDateTime;
				                                           		var endTime = s.DisplayTimePeriod.EndDateTime;
				                                           		var firstHour = startTime
				                                           			.Subtract(new TimeSpan(0, 0, startTime.Minute, startTime.Second, startTime.Millisecond))
				                                           			.AddHours(1);
				                                           		var times = firstHour
				                                           			.TimeRange(endTime, TimeSpan.FromHours(1))
				                                           			.Union(new[] {startTime})
				                                           			.Union(new[] {endTime})
				                                           			.Union(new[] {firstHour})
				                                           			.OrderBy(t => t)
				                                           			.Distinct()
				                                           			;
				                                           		return from t in times
				                                           		       select new TimeLineMappingData
				                                           		              	{
				                                           		              		DisplayTimePeriod = s.DisplayTimePeriod,
				                                           		              		Time = t
				                                           		              	};
				                                           	}))
				;

			CreateMap<TimeLineMappingData, TimeLineViewModel>()
				.ForMember(d => d.ShortTime, o => o.ResolveUsing(s => _createHourText.CreateText(s.Time)))
				.ForMember(d => d.PositionPercent, o => o.ResolveUsing(s =>
																					{
																						var displayedTicks = (decimal)s.DisplayTimePeriod.EndDateTime.Ticks - s.DisplayTimePeriod.StartDateTime.Ticks;
																						var positionTicks = (decimal)s.Time.Ticks - s.DisplayTimePeriod.StartDateTime.Ticks;
																						return positionTicks / displayedTicks;
																					}))
				.ForMember(d => d.IsFullHour, o => o.ResolveUsing( s => TimeZoneInfo.ConvertTimeFromUtc(s.Time, _userTimeZone().TimeZone()).TimeOfDay.Minutes == 0))
				;

			CreateMap<TeamScheduleDayDomainData, AgentScheduleViewModel>()
				.ForMember(d => d.AgentName, o => o.MapFrom(s => s.Person.Name.ToString()))
				.ForMember(d => d.DayOffText, o => o.ResolveUsing(s =>
																				{
																					if (s.Projection == null)
																						return null;
																					if (s.Projection.DayOff == null)
																						return null;
																					return s.Projection.DayOff.Description.Name;
																				}))
				.ForMember(d => d.Layers, o => o.ResolveUsing(s =>
																		{
																			if (s.Projection == null || s.Projection.Layers == null)
																				return new LayerMappingData[] { };
																			return from l in s.Projection.Layers
																					 select new LayerMappingData
																								{
																									Layer = l,
																									DisplayTimePeriod = s.DisplayTimePeriod,
																								};
																		}))
				.ForMember(d => d.Badges, o => o.ResolveUsing(s =>
				{
					var badges = _badgeProvider.GetBadges(s.Person.Id);
					if (badges == null)
						return null;

					return badges.Select(x => new BadgeViewModel
					{
						BadgeType = x.BadgeType,
						BronzeBadge = x.BronzeBadge,
						SilverBadge = x.SilverBadge,
						GoldBadge = x.GoldBadge
					});
				}))
				;

			CreateMap<LayerMappingData, LayerViewModel>()
				.ForMember(d => d.PositionPercent, o => o.ResolveUsing(s =>
				                                                  	{
				                                                  		if (!s.Layer.Period.HasValue)
				                                                  			return 0;
				                                                  		if (s.DisplayTimePeriod.StartDateTime == DateTime.MinValue)
				                                                  			return 0;
				                                                  		decimal positionTicks = (s.Layer.Period.Value.StartDateTime - s.DisplayTimePeriod.StartDateTime).Ticks;
				                                                  		decimal displayedTicks = s.DisplayTimePeriod.ElapsedTime().Ticks;
				                                                  		return positionTicks/displayedTicks;
				                                                  	}
				                                        	))
				.ForMember(d => d.EndPositionPercent, o => o.ResolveUsing(s =>
				                                                     	{
				                                                     		if (!s.Layer.Period.HasValue)
				                                                     			return 0;
				                                                     		if (s.DisplayTimePeriod.StartDateTime == DateTime.MinValue)
				                                                     			return 0;
				                                                     		decimal endPositionTicks = (s.Layer.Period.Value.EndDateTime - s.DisplayTimePeriod.StartDateTime).Ticks;
				                                                     		decimal displayedTicks = s.DisplayTimePeriod.ElapsedTime().Ticks;
				                                                     		return endPositionTicks/displayedTicks;
				                                                     	}
				                                           	))
				.ForMember(d => d.Color, o => o.MapFrom(s => s.Layer.DisplayColor.ToHtml()))
				.ForMember(d => d.StartTime, o => o.ResolveUsing(s =>
				                                       	{
				                                       		if (!s.Layer.Period.HasValue)
				                                       			return null;
															return s.Layer.Period.Value.StartDateTimeLocal(_userTimeZone.Invoke().TimeZone()).ToShortTimeString();
				                                       	}))
				.ForMember(d => d.EndTime, o => o.ResolveUsing(s =>
														{
															if (!s.Layer.Period.HasValue)
																return null;
															return s.Layer.Period.Value.EndDateTimeLocal(_userTimeZone.Invoke().TimeZone()).ToShortTimeString();
														}))
				.ForMember(d => d.ActivityName, o => o.MapFrom(s => s.Layer.ActivityName))
				;

			CreateMap<TeamScheduleDomainData, PeriodSelectionViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Date.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Display, o => o.MapFrom(s => s.Date.ToShortDateString()))
				.ForMember(d => d.SelectableDateRange, o => o.MapFrom(s => new DateOnlyPeriod(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime), new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime))))
				.ForMember(d => d.SelectedDateRange, o => o.MapFrom(s => new DateOnlyPeriod(s.Date, s.Date)))
				.ForMember(d => d.PeriodNavigation, o => o.MapFrom(s => s))
				;

			CreateMap<TeamScheduleDomainData, PeriodNavigationViewModel>()
				.ForMember(d => d.CanPickPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasNextPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasPrevPeriod, o => o.UseValue(true))
				.ForMember(d => d.NextPeriod, o => o.MapFrom(s => s.Date.AddDays(1).ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.PrevPeriod, o => o.MapFrom(s => s.Date.AddDays(-1).ToFixedClientDateOnlyFormat()))
				;

			CreateMap<DateOnlyPeriod, PeriodDateRangeViewModel>()
				.ForMember(d => d.MaxDate, o => o.MapFrom(s => s.EndDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.MinDate, o => o.MapFrom(s => s.StartDate.ToFixedClientDateOnlyFormat()))
				;
		}
	}
}
