using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceAndScheduleDayViewModelMappingProfile : Profile
	{
		private readonly IResolve<IProjectionProvider> _projectionProvider;
		private readonly IResolve<IPreferenceFulfilledChecker> _preferenceFulfilledChecker;

		public PreferenceAndScheduleDayViewModelMappingProfile(IResolve<IProjectionProvider> projectionProvider, IResolve<IPreferenceFulfilledChecker> preferenceFulfilledChecker)
		{
			_projectionProvider = projectionProvider;
			_preferenceFulfilledChecker = preferenceFulfilledChecker;
		}

		protected override void Configure()
		{
			CreateMap<IScheduleDay, PreferenceAndScheduleDayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
				.ForMember(s => s.Preference,
				           o =>
				           o.MapFrom(
					           s =>
					           s.PersonRestrictionCollection() == null
						           ? null
						           : s.PersonRestrictionCollection().OfType<IPreferenceDay>().SingleOrDefault()))
				.ForMember(s => s.DayOff,
				           o =>
				           o.MapFrom(s => s.PersonDayOffCollection() == null ? null : s.PersonDayOffCollection().SingleOrDefault()))
				.ForMember(s => s.Absence, o => o.MapFrom(s => (s.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence ||
				                                                s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff) &&
				                                               s.PersonAbsenceCollection() != null
					                                               ? s.PersonAbsenceCollection().First()
					                                               : null))
				.ForMember(s => s.PersonAssignment,
				           o => o.MapFrom(s => s.SignificantPartForDisplay() == SchedulePartView.MainShift ? s : null))
				.ForMember(d => d.Fulfilled, o => o.MapFrom(s =>
					{
						if (s != null && s.IsScheduled())
							return _preferenceFulfilledChecker.Invoke().IsPreferenceFulfilled(s);
						return null;
					}))
				.ForMember(d => d.Feedback, o => o.MapFrom(s => s == null || !s.IsScheduled()))
				.ForMember(d => d.StyleClassName, o => o.MapFrom(s =>
					{
						if (s != null)
						{
							if (s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff)
								return StyleClasses.Striped;
							if (s.SignificantPartForDisplay() == SchedulePartView.DayOff)
								return StyleClasses.DayOff + " " + StyleClasses.Striped;
						}
						return null;
					}))
				.ForMember(d => d.BorderColor, o => o.MapFrom(s =>
					{
						if (s != null)
						{
							if((s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff))
							{
								if (s.PersonAbsenceCollection() != null)
									return s.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToHtml();
								
							}
							if (s.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence)
								return s.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToHtml();
							if (s.SignificantPartForDisplay() == SchedulePartView.MainShift)
								return s.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor.ToHtml();
							if (s.SignificantPartForDisplay() == SchedulePartView.DayOff)
								return null;
						}
						return null;
					}))
				;

			CreateMap<IPersonDayOff, DayOffDayViewModel>()
				.ForMember(d => d.DayOff, o => o.MapFrom(s => s.DayOff.Description.Name))
				;

			CreateMap<IPersonAbsence, AbsenceDayViewModel>()
				.ForMember(s => s.Absence, o => o.MapFrom(s => s.Layer.Payload.Description.Name))
				;

			CreateMap<IScheduleDay, PersonAssignmentDayViewModel>()
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => s.AssignmentHighZOrder().MainShift.ShiftCategory.Description.Name))
				.ForMember(d => d.ContractTime, o => o.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(_projectionProvider.Invoke().Projection(s).ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.TimeSpan, o => o.MapFrom(s => s.AssignmentHighZOrder().Period.TimePeriod(s.TimeZone).ToShortTimeString()))
				.ForMember(d => d.ContractTimeMinutes, o => o.MapFrom(s => _projectionProvider.Invoke().Projection(s).ContractTime().TotalMinutes));
		}
	}
}