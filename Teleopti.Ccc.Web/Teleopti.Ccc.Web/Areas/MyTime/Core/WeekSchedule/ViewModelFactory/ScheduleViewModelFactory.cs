using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
	    private readonly IMappingEngine _mapper;
		private readonly IScheduleDomainDataProvider _scheduleDomainDataProvider;


		public ScheduleViewModelFactory(IMappingEngine mapper, IScheduleDomainDataProvider scheduleDomainDataProvider)
		{
			_mapper = mapper;
			_scheduleDomainDataProvider = scheduleDomainDataProvider;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
	    {
            var domainData = _mapper.Map<DateOnly, MonthScheduleDomainData>(dateOnly);
            return _mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(domainData);
        }

	    public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly)
	    {
		    var domainData = _scheduleDomainDataProvider.GetWeekScheduleDomainData(dateOnly);
			return _mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
		}
	}
}	