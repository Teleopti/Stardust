using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
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

		public WeekScheduleViewModelMappingProfile(Func<IMappingEngine> mapper, Func<IPeriodSelectionViewModelFactory> periodSelectionViewModelFactory, Func<IPeriodViewModelFactory> periodViewModelFactory, Func<IHeaderViewModelFactory> headerViewModelFactory)
		{
			_mapper = mapper;
			_periodSelectionViewModelFactory = periodSelectionViewModelFactory;
			_periodViewModelFactory = periodViewModelFactory;
			_headerViewModelFactory = headerViewModelFactory;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<WeekScheduleDomainData, WeekScheduleViewModel>()
				.ForMember(d => d.PeriodSelection, c => c.MapFrom(s => _periodSelectionViewModelFactory.Invoke().CreateModel(s.Date)))
				.ForMember(d => d.Styles, o => o.MapFrom(s =>
				                                         	{
				                                         		if (s.Days == null) return null;
				                                         		var shiftCategoryColors =
				                                         			from d in s.Days
				                                         			let isMainShift = d.ScheduleDay != null && d.ScheduleDay.AssignmentHighZOrder() != null
				                                         			where isMainShift
				                                         			select d.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor;
				                                         		var visualLayerColors =
				                                         			from d in s.Days
				                                         			let enumerableProjection = d.Projection as IEnumerable<IVisualLayer>
				                                         			let projection = enumerableProjection ?? new IVisualLayer[] {}
				                                         			from l in projection
				                                         			select l.DisplayColor();
				                                         		var absenceColors =
				                                         			from d in s.Days
				                                         			let isAbsence = d.ScheduleDay != null && d.ScheduleDay.PersonAbsenceCollection() != null
				                                         			where isAbsence
				                                         			from a in d.ScheduleDay.PersonAbsenceCollection()
				                                         			select a.Layer.Payload.DisplayColor;
				                                         		return shiftCategoryColors
				                                         			.Union(visualLayerColors)
				                                         			.Union(absenceColors)
				                                         			.Distinct()
				                                         			.ToArray();
				                                         	}))
				;

			CreateMap<Color, StyleClassViewModel>()
				.ForMember(d => d.Name, o => o.MapFrom(s => s.ToStyleClass()))
				.ForMember(d => d.ColorHex, o => o.MapFrom(s => s.ToHtml()))
				;

			CreateMap<WeekScheduleDayDomainData, DayViewModel>()
				.ForMember(d => d.Date, c => c.MapFrom(s => s.Date.ToShortDateString()))
				.ForMember(d => d.FixedDate, c => c.MapFrom(s => s.Date.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Periods, c => c.MapFrom(s => _periodViewModelFactory.Invoke().CreatePeriodViewModels(s.Projection)))
				.ForMember(d => d.TextRequestCount, o => o.MapFrom(s => s.PersonRequests == null ? 0 : s.PersonRequests.Count(r => r.Request is TextRequest)))
				.ForMember(d => d.State, o => o.MapFrom(s =>
				                                        	{
				                                        		if (s.Date == DateOnly.Today)
				                                        			return SpecialDateState.Today;
				                                        		return (SpecialDateState) 0;
				                                        	}))
				.ForMember(d => d.Header, o => o.MapFrom(s => _headerViewModelFactory.Invoke().CreateModel(s.ScheduleDay)))
				.ForMember(d => d.Note, o => o.MapFrom(s => s.ScheduleDay.PublicNoteCollection()))
				.ForMember(d => d.Summary, c => c.MapFrom(
					s =>
						{
							var mappingEngine = _mapper();
							var significantPart = s.ScheduleDay.SignificantPartForDisplay();
							if (significantPart == SchedulePartView.DayOff)
								return mappingEngine.Map<WeekScheduleDayDomainData, PersonDayOffPeriodViewModel>(s);
							if (significantPart == SchedulePartView.MainShift)
								return mappingEngine.Map<WeekScheduleDayDomainData, PersonAssignmentPeriodViewModel>(s);
							if (significantPart == SchedulePartView.FullDayAbsence)
								return mappingEngine.Map<WeekScheduleDayDomainData, FullDayAbsencePeriodViewModel>(s);
							return mappingEngine.Map<WeekScheduleDayDomainData, PeriodViewModel>(s);
						}))
				;

			CreateMap<IEnumerable<IPublicNote>, NoteViewModel>()
				.ForMember(d => d.Message, c => c.MapFrom(
					s =>
						{
							var publicNote = s.FirstOrDefault();
							return publicNote != null ? publicNote.ScheduleNote : string.Empty;
						}))
				;

			CreateMap<WeekScheduleDayDomainData, PeriodViewModel>()
				.ForMember(d => d.Title, c => c.Ignore())
				.ForMember(d => d.Summary, c => c.Ignore())
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.Ignore())
				;

			CreateMap<WeekScheduleDayDomainData, PersonDayOffPeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.PersonDayOffCollection().First().DayOff.Description.Name))
				.ForMember(d => d.Summary, c => c.Ignore())
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.Ignore())
				;

			CreateMap<WeekScheduleDayDomainData, PersonAssignmentPeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.Description.Name))
				.ForMember(d => d.Summary, c => c.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().Period.TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString()))
				.ForMember(d => d.StyleClassName, c => c.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor.ToStyleClass()))
				;

			CreateMap<WeekScheduleDayDomainData, FullDayAbsencePeriodViewModel>()
				.ForMember(d => d.Title, c => c.MapFrom(s => s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.Description.Name))
				.ForMember(d => d.Summary, c => c.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.Meeting, c => c.Ignore())
				.ForMember(d => d.TimeSpan, c => c.Ignore())
				.ForMember(d => d.StyleClassName, c => c.MapFrom(s => s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToStyleClass()))
				;
		}
	}
}