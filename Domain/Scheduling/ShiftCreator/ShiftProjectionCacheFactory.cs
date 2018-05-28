using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class ShiftProjectionCacheFactory
	{
		private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;
		private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public ShiftProjectionCacheFactory(IShiftFromMasterActivityService shiftFromMasterActivityService,
			IRuleSetProjectionEntityService ruleSetProjectionEntityService,
			IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_shiftFromMasterActivityService = shiftFromMasterActivityService;
			_ruleSetProjectionEntityService = ruleSetProjectionEntityService;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}
		
		public virtual IEnumerable<ShiftProjectionCache> Create(IWorkShiftRuleSet ruleSet, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			var baseIsMasterActivity = ruleSet.TemplateGenerator.BaseActivity is IMasterActivity;
			var ret = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, null)
				.Select(s => s.WorkShift)
				.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift, baseIsMasterActivity))
				.Select(workShift => new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker))
				.ToArray();
			
			foreach (var shiftProjectionCache in ret)
			{
				shiftProjectionCache.SetDate(dateOnlyAsDateTimePeriod);
			}

			return ret;
		}
	}
}