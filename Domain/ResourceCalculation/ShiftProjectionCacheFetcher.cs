using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		
		public virtual IEnumerable<ShiftProjectionCache> Execute(IWorkShiftRuleSet ruleSet)
		{
			return _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, null)
				.Select(s => s.WorkShift)
				.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift))
				.Select(workShift => new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker));
			var baseIsMasterActivity = ruleSet.TemplateGenerator.BaseActivity is IMasterActivity;
			return tmpList.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift, baseIsMasterActivity))
		}
	}
}