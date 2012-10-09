using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceAndScheduleDayViewModelMappingProfile : Profile
	{
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
				;

			CreateMap<IPersonAbsence, AbsenceDayViewModel>()
				.ForMember(s => s.Absence, o => o.MapFrom(s => s.Layer.Payload.Description.Name))
				;
		}
	}
}