using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCacheFetcher
	{
		private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;
		private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;

		public ShiftProjectionCacheFetcher(IShiftFromMasterActivityService shiftFromMasterActivityService,
			IRuleSetProjectionEntityService ruleSetProjectionEntityService)
		{
			_shiftFromMasterActivityService = shiftFromMasterActivityService;
			_ruleSetProjectionEntityService = ruleSetProjectionEntityService;
		}

		public IEnumerable<ShiftProjectionCache> Execute(IWorkShiftRuleSet ruleSet)
		{
			var baseIsMasterActivity = ruleSet.TemplateGenerator.BaseActivity is IMasterActivity;
			return _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, null)
				.Select(s => s.WorkShift)
				.SelectMany(shift => _shiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(shift, baseIsMasterActivity))
				.Select(workShift => new ShiftProjectionCache(workShift))
				.ToArray();
		}
	}
}