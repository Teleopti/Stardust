using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory
{
	public class AsmViewModelFactory : IAsmViewModelFactory
	{
		private readonly INow _now;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IMappingEngine _mapper;

		public AsmViewModelFactory(INow now, IScheduleProvider scheduleProvider, IMappingEngine mapper)
		{
			_now = now;
			_scheduleProvider = scheduleProvider;
			_mapper = mapper;
		}

		public AsmViewModel CreateViewModel()
		{
			var today = _now.Date();
			var loadPeriod = new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1));
			var schedules = _scheduleProvider.GetScheduleForPeriod(loadPeriod);
			return _mapper.Map<IEnumerable<IScheduleDay>, AsmViewModel>(schedules);
		}
	}
}