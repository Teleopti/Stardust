using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
    public class MonthScheduleDomainDataMappingProfile : Profile
    {
        private readonly IScheduleProvider _scheduleProvider;

        public MonthScheduleDomainDataMappingProfile(IScheduleProvider scheduleProvider)
        {
            _scheduleProvider = scheduleProvider;
        }

        protected override void Configure()
        {
            base.Configure();

            CreateMap<DateOnly, MonthScheduleDomainData>()
                .ForMember(d => d.CurrentDate, c => c.MapFrom(s => s))
                .ForMember(d=>d.Days, c => c.ResolveUsing(s =>
                    {
                        var firstDate = DateHelper.GetFirstDateInMonth(s, CultureInfo.CurrentCulture);
                        firstDate = DateHelper.GetFirstDateInWeek(firstDate, CultureInfo.CurrentCulture);
                        var lastDate = DateHelper.GetLastDateInMonth(s, CultureInfo.CurrentCulture);
                        lastDate = DateHelper.GetLastDateInWeek(lastDate, CultureInfo.CurrentCulture);
                        var period = new DateOnlyPeriod(new DateOnly(firstDate), new DateOnly(lastDate) );
                        return _scheduleProvider.GetScheduleForPeriod(period).Select(r => new MonthScheduleDayDomainData{ScheduleDay = r});
                    }));
        }
    }
}