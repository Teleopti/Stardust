using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCacheFetcher
	{
		private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;
		private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public ShiftProjectionCacheFetcher(IShiftFromMasterActivityService shiftFromMasterActivityService,
			IRuleSetProjectionEntityService ruleSetProjectionEntityService,
			IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_shiftFromMasterActivityService = shiftFromMasterActivityService;
			_ruleSetProjectionEntityService = ruleSetProjectionEntityService;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}
		
		public IEnumerable<ShiftProjectionCache> Execute(IWorkShiftRuleSet ruleSet)
		{
			var callback = new WorkShiftAddStopperCallback();
			callback.StartNewRuleSet(ruleSet);
			var tmpList = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, callback).Select(s => s.WorkShift).ToArray();
                
			return tmpList.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift))
				.Select(workShift => new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker)).ToList();
		}
	}
}