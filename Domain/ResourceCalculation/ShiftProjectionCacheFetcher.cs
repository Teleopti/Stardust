﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
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
			var baseIsMasterActivity = ruleSet.TemplateGenerator.BaseActivity is IMasterActivity;
			return _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, null)
				.Select(s => s.WorkShift)
				.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift, baseIsMasterActivity))
				.Select(workShift => new ShiftProjectionCache(workShift, _personalShiftMeetingTimeChecker))
				.ToArray();
		}
	}
}