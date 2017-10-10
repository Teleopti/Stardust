using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class NightRestWhiteSpotSolverServiceFactory : INightRestWhiteSpotSolverServiceFactory
	{
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IScheduleService _scheduleService;
		private readonly IResourceCalculation _resourceOptimization;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IUserTimeZone _userTimeZone;

		public NightRestWhiteSpotSolverServiceFactory(IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IScheduleService scheduleService, 
			IResourceCalculation resourceOptimization,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			IUserTimeZone userTimeZone)
		{
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_scheduleService = scheduleService;
			_resourceOptimization = resourceOptimization;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_userTimeZone = userTimeZone;
		}

		public INightRestWhiteSpotSolverService Create(bool considerShortBreaks)
		{
			return new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
				_deleteAndResourceCalculateService,
				_scheduleService, 
				new ResourceCalculateDelayer(_resourceOptimization, considerShortBreaks, _schedulingResultStateHolder(), _userTimeZone));
		}
	}
}