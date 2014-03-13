using System;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory
{
	public class AsmViewModelFactory : IAsmViewModelFactory
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IAsmViewModelMapper _mapper;
		private readonly IPushMessageProvider _pushMessageProvider;

		public AsmViewModelFactory(IScheduleProvider scheduleProvider, IAsmViewModelMapper mapper, IPushMessageProvider pushMessageProvider)
		{
			_scheduleProvider = scheduleProvider;
			_mapper = mapper;
			_pushMessageProvider = pushMessageProvider;
		}

		public AsmViewModel CreateViewModel(DateTime asmZeroLocal)
		{
			var theDate = new DateOnly(asmZeroLocal);
			var loadPeriod = new DateOnlyPeriod(theDate, theDate.AddDays(2));
			var schedules = _scheduleProvider.GetScheduleForPeriod(loadPeriod);
			return _mapper.Map(asmZeroLocal, schedules, _pushMessageProvider.UnreadMessageCount);
		}
	}
}