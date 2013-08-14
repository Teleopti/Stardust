﻿using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleViewModelFactory : IScheduleViewModelFactory
	{
	    private readonly IMappingEngine _mapper;

	    public ScheduleViewModelFactory(IMappingEngine mapper)
	    {
	        _mapper = mapper;
	    }

	    public WeekScheduleViewModel CreateWeekViewModel(DateOnly dateOnly)
	    {
	    	var domainData = _mapper.Map<DateOnly, WeekScheduleDomainData>(dateOnly);
			return _mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
		}
	}
}	