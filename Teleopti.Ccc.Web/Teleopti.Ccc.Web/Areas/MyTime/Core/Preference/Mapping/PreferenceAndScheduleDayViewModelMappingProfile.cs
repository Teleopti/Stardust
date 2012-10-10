using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceAndScheduleDayViewModelMappingProfile : Profile
	{
		private readonly IResolve<IProjectionProvider> _projectionProvider;

		public PreferenceAndScheduleDayViewModelMappingProfile(IResolve<IProjectionProvider> projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			CreateMap<IScheduleDay, PreferenceAndScheduleDayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
				.ForMember(s => s.Preference, o => o.MapFrom(s => s.PersonRestrictionCollection() == null ? null : s.PersonRestrictionCollection().OfType<IPreferenceDay>().SingleOrDefault()))
				.ForMember(s => s.DayOff, o => o.MapFrom(s => s.PersonDayOffCollection() == null ? null : s.PersonDayOffCollection().SingleOrDefault()))
				.ForMember(s => s.Absence, o => o.MapFrom(s => s.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence ||
				                                               s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff
					                                               ? s.PersonAbsenceCollection().First()
					                                               : null))
				.ForMember(s => s.PersonAssignment, o => o.MapFrom(s => s.SignificantPartForDisplay() == SchedulePartView.MainShift ? s : null))
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