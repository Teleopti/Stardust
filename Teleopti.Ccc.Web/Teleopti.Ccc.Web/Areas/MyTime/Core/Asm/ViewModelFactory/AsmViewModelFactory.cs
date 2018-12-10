using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory
{
	public class AsmViewModelFactory : IAsmViewModelFactory
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IAsmViewModelMapper _mapper;
		private readonly IPushMessageProvider _pushMessageProvider;
		private readonly IPermissionProvider _permissionProvider;

		public AsmViewModelFactory(IScheduleProvider scheduleProvider, IAsmViewModelMapper mapper,
			IPushMessageProvider pushMessageProvider, IPermissionProvider permissionProvider)
		{
			_scheduleProvider = scheduleProvider;
			_mapper = mapper;
			_pushMessageProvider = pushMessageProvider;
			_permissionProvider = permissionProvider;
		}

		public AsmViewModel CreateViewModel(DateTime asmZeroLocal)
		{
			var theDate = new DateOnly(asmZeroLocal);
			var loadPeriod = new DateOnlyPeriod(theDate, theDate.AddDays(2));
			var schedules = _scheduleProvider.GetScheduleForPeriod(loadPeriod,
				new ScheduleDictionaryLoadOptions(false, false) {LoadAgentDayScheduleTags = false});
			return _mapper.Map(asmZeroLocal, schedules, _pushMessageProvider.UnreadMessageCount);
		}

		public bool HasAsmPermission()
		{
			return _permissionProvider.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
		}


	}
}