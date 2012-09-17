using System;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory
{
	public class AsmViewModelFactory : IAsmViewModelFactory
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IAsmViewModelMapper _mapper;

		public AsmViewModelFactory(IScheduleProvider scheduleProvider, IAsmViewModelMapper mapper)
		{
			_scheduleProvider = scheduleProvider;
			_mapper = mapper;
		}

		public AsmViewModel CreateViewModel(DateTime asmZero)
		{
			var theDate = new DateOnly(asmZero);
			var loadPeriod = new DateOnlyPeriod(theDate, theDate.AddDays(2));
			var schedules = _scheduleProvider.GetScheduleForPeriod(loadPeriod);
			return _mapper.Map(asmZero, schedules);
		}
	}
}