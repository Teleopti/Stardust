using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
    public class MonthScheduleViewModelMappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            CreateMap<MonthScheduleDomainData, MonthScheduleViewModel>()
                .ForMember(d => d.ScheduleDays, c => c.MapFrom(s => s.Days))
                .ForMember(d => d.FixedDate, c => c.MapFrom(s => s.CurrentDate.ToFixedClientDateOnlyFormat()));
            
            CreateMap<MonthScheduleDayDomainData, MonthDayViewModel>()
                .ForMember(d => d.Date, c => c.MapFrom(s => s.ScheduleDay.DateOnlyAsPeriod.DateOnly))
                .ForMember(d => d.FixedDate, c => c.MapFrom(s => s.ScheduleDay.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
                .ForMember(d => d.IsWorkingDay, c => c.ResolveUsing(
                    s =>
                        {
                                var significantPart =s.ScheduleDay.SignificantPartForDisplay();
                                return (significantPart == SchedulePartView.MainShift);
                        }))
                .ForMember(d => d.IsNotWorkingDay, c => c.ResolveUsing(
                    s =>
                    {
                        var significantPart = s.ScheduleDay.SignificantPartForDisplay();
                        return (significantPart == SchedulePartView.DayOff);
                    }))
                .ForMember(d => d.DisplayColor, c => c.ResolveUsing(
                    s =>
                        {
                                var significantPart = s.ScheduleDay.SignificantPartForDisplay();
                                if (significantPart == SchedulePartView.MainShift)
                                {
                                    return s.ScheduleDay.PersonAssignment().ShiftCategory.DisplayColor.ToHtml();
                                }
                            return string.Empty;
                        }));
        }
    }
}