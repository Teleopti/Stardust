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
		private readonly IWeekScheduleDomainDataProvider _weekScheduleDomainDataProvider;
		private readonly IMonthScheduleDomainDataProvider _monthScheduleDomainDataProvider;

		public ScheduleViewModelFactory(IMappingEngine mapper, IWeekScheduleDomainDataProvider weekScheduleDomainDataProvider, IMonthScheduleDomainDataProvider monthScheduleDomainDataProvider)
		{
			_mapper = mapper;
			_weekScheduleDomainDataProvider = weekScheduleDomainDataProvider;
			_monthScheduleDomainDataProvider = monthScheduleDomainDataProvider;
		}

		public MonthScheduleViewModel CreateMonthViewModel(DateOnly dateOnly)
	    {
			var domainData = _monthScheduleDomainDataProvider.Get(dateOnly);
			return _mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(domainData);
        }

	    public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly)
	    {
		    var domainData = _weekScheduleDomainDataProvider.Get(dateOnly);
			return _mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
		}
	}
}	