﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleViewModelMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<IPeriodSelectionViewModelFactory> _periodSelectionViewModelFactory;
		private readonly Func<IPeriodViewModelFactory> _periodViewModelFactory;
		private readonly Func<IHeaderViewModelFactory> _headerViewModelFactory;
		private readonly Func<IScheduleColorProvider> _scheduleColorProvider;
		private readonly Func<IPermissionProvider> _permissionProvider;
		private readonly Func<ILoggedOnUser> _loggedOnUser;

		public WeekScheduleViewModelMappingProfile(Func<IMappingEngine> mapper, Func<IPeriodSelectionViewModelFactory> periodSelectionViewModelFactory, Func<IPeriodViewModelFactory> periodViewModelFactory, Func<IHeaderViewModelFactory> headerViewModelFactory, Func<IScheduleColorProvider> scheduleColorProvider, Func<IPermissionProvider> permissionProvider, Func<ILoggedOnUser> loggedOnUser)
		{
			_mapper = mapper;
			_periodSelectionViewModelFactory = periodSelectionViewModelFactory;
			_periodViewModelFactory = periodViewModelFactory;
			_headerViewModelFactory = headerViewModelFactory;
			_scheduleColorProvider = scheduleColorProvider;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<WeekScheduleDomainData, WeekScheduleViewModel>()
				.ForMember(d => d.PeriodSelection, c => c.MapFrom(s => _periodSelectionViewModelFactory.Invoke().CreateModel(s.Date)))
				.ForMember(d => d.Styles, o => o.MapFrom(s => s.Days == null ? null : _scheduleColorProvider.Invoke().GetColors(s.ColorSource)))
				.ForMember(d => d.TimeLine, o => o.MapFrom(s =>
				{
					var startTime = s.MinMaxTime.StartTime;
					var endTime = s.MinMaxTime.EndTime;
					var firstHour = startTime
						.Subtract(new TimeSpan(0, 0, startTime.Minutes, startTime.Seconds, startTime.Milliseconds))
						.Add(new TimeSpan(1, 0, 0));
					var lastHour = endTime
						.Subtract(new TimeSpan(0, 0, endTime.Minutes, endTime.Seconds, endTime.Milliseconds));
					var times = firstHour
						.TimeRange(lastHour, TimeSpan.FromHours(1))
						.Union(new[] { startTime })
						.Union(new[] { endTime })
						.Union(new[] { firstHour })
						.OrderBy(t => t)
						.Distinct()
						;
					return from t in times
						   select new TimeLineViewModel
									{
										PositionPercentage = (decimal)(t - startTime).Ticks / (endTime - startTime).Ticks,
										Culture = _loggedOnUser.Invoke().CurrentUser().PermissionInformation.Culture().ToString(),
										Time = t
									};
				}))
				.ForMember(d => d.RequestPermission, c => c.MapFrom(s => new RequestPermission
				{
					TextRequestPermission = _permissionProvider.Invoke().HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests),
					AbsenceRequestPermission = _permissionProvider.Invoke().HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb),
				}));

			CreateMap<WeekScheduleDayDomainData, DayViewModel>()
				.ForMember(d => d.Date, c => c.MapFrom(s => s.Date.ToShortDateString()))
				.ForMember(d => d.FixedDate, c => c.MapFrom(s => s.Date.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Periods, c => c.MapFrom(s =>
															{
																var projectionList = new List<IVisualLayer>();
																if (s.ProjectionYesterday != null)
																	projectionList.AddRange(s.ProjectionYesterday);
																if (s.Projection != null)
																	projectionList.AddRange(s.Projection);
																return _periodViewModelFactory.Invoke().CreatePeriodViewModels(projectionList,
																																s.MinMaxTime,
																																s.Date,
																																s.ScheduleDay == null ? null : s.ScheduleDay.TimeZone);
															}))
				.ForMember(d => d.TextRequestCount, o => o.MapFrom(s => s.PersonRequests == null ? 0 : s.PersonRequests.Count(r => (r.Request is TextRequest || r.Request is AbsenceRequest))))
				.ForMember(d => d.Allowance, c => c.MapFrom(s => s.Allowance))

				.ForMember(d => d.State, o => o.MapFrom(s =>
															{
																if (s.Date == DateOnly.Today)
																	return SpecialDateState.Today;
																return (SpecialDateState)0;
															}))
				.ForMember(d => d.Header, o => o.MapFrom(s => _headerViewModelFactory.Invoke().CreateModel(s.ScheduleDay)))
				.ForMember(d => d.Note, o => o.MapFrom(s => s.ScheduleDay.PublicNoteCollection()))
				.ForMember(d => d.Summary, c => c.MapFrom(
					s =>
					{
						var mappingEngine = _mapper();
						var significantPart = s.ScheduleDay.SignificantPartForDisplay();
						if (significantPart == SchedulePartView.ContractDayOff)
						{
							var periodViewModel = mappingEngine.Map<WeekScheduleDayDomainData, FullDayAbsencePeriodViewModel>(s);
							periodViewModel.StyleClassName += " " + StyleClasses.Striped;
							return periodViewModel;
						}
						if (significantPart == SchedulePartView.DayOff)
							return mappingEngine.Map<WeekScheduleDayDomainData, PersonDayOffPeriodViewModel>(s);
						if (significantPart == SchedulePartView.MainShift)
							return mappingEngine.Map<WeekScheduleDayDomainData, PersonAssignmentPeriodViewModel>(s);
						if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
							return mappingEngine.Map<WeekScheduleDayDomainData, FullDayAbsencePeriodViewModel>(s);
						return mappingEngine.Map<WeekScheduleDayDomainData, PeriodViewModel>(s);
					}))
				;

			CreateMap<IEnumerable<IPublicNote>, NoteViewModel>()
				.ForMember(d => d.Message, c => c.MapFrom(
					s =>
					{
						var publicNote = s.FirstOrDefault();
						return publicNote != null ? publicNote.GetScheduleNote(new NoFormatting()) : string.Empty;
					}))
				;

			CreateMap<WeekScheduleDayDomainData, PeriodViewModel>()
				.ForMember(d => d.Title, c => c.Ignore())
				.ForMember(d => d.Summary, c => c.Ignore())
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.Ignore())
				.ForMember(d => d.StartPositionPercentage, c => c.Ignore())
				.ForMember(d => d.EndPositionPercentage, c => c.Ignore())
				.ForMember(d => d.Color, c => c.Ignore())
				;

			CreateMap<WeekScheduleDayDomainData, PersonDayOffPeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.PersonDayOffCollection().First().DayOff.Description.Name))
				.ForMember(d => d.Summary, c => c.Ignore())
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.UseValue(StyleClasses.DayOff + " " + StyleClasses.Striped))
				.ForMember(d => d.StartPositionPercentage, c => c.Ignore())
				.ForMember(d => d.EndPositionPercentage, c => c.Ignore())
				.ForMember(d => d.Color, c => c.Ignore())
				;

			CreateMap<WeekScheduleDayDomainData, PersonAssignmentPeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.Description.Name))
				.ForMember(d => d.Summary, c => c.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().Period.TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString()))
				.ForMember(d => d.StyleClassName, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor.ToStyleClass()))
				.ForMember(d => d.StartPositionPercentage, c => c.Ignore())
				.ForMember(d => d.EndPositionPercentage, c => c.Ignore())
				.ForMember(d => d.Color, c => c.Ignore())
				;

			CreateMap<WeekScheduleDayDomainData, FullDayAbsencePeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.Description.Name))
				.ForMember(d => d.Summary, c => c.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.MapFrom(s => s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToStyleClass()))
				.ForMember(d => d.StartPositionPercentage, c => c.Ignore())
				.ForMember(d => d.EndPositionPercentage, c => c.Ignore())
				.ForMember(d => d.Color, c => c.Ignore())
				;

			CreateMap<IAbsence, AbsenceTypeViewModel>()
				.ForMember(d => d.Name, c => c.MapFrom(
					s => s.Description.Name))
				;
		}
	}
}