using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IRuleSetToShiftsGenerator
	{
		IEnumerable<IShiftProjectionCache> Generate(IWorkShiftRuleSet ruleSet);
	}
	
	public class RuleSetToShiftsGenerator : IRuleSetToShiftsGenerator
	{
		private readonly IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
		private readonly IShiftFromMasterActivityService _shiftFromMasterActivityService;

		public RuleSetToShiftsGenerator(IRuleSetProjectionEntityService ruleSetProjectionEntityService,
		                                IShiftFromMasterActivityService shiftFromMasterActivityService)
		{
			_ruleSetProjectionEntityService = ruleSetProjectionEntityService;
			_shiftFromMasterActivityService = shiftFromMasterActivityService;
		}

		public IEnumerable<IShiftProjectionCache> Generate(IWorkShiftRuleSet ruleSet)
		{
			var callback = new WorkShiftAddStopperCallback();
			callback.StartNewRuleSet(ruleSet);
			var infoList = _ruleSetProjectionEntityService.ProjectionCollection(ruleSet, callback);
			IList<IWorkShift> tmpList = infoList.Select(workShiftVisualLayerInfo => workShiftVisualLayerInfo.WorkShift).ToList();

			IList<IShiftProjectionCache> retList = new List<IShiftProjectionCache>();
			foreach (var shift in tmpList)
			{
				var shiftsFromMasterActivity = getShiftFromMasterActivity(shift);
				if (shiftsFromMasterActivity == null)
					retList.Add(new ShiftProjectionCache(shift, new PersonalShiftMeetingTimeChecker()));
				else
				{
					foreach (IWorkShift workShift in shiftsFromMasterActivity)
					{
						retList.Add(new ShiftProjectionCache(workShift, new PersonalShiftMeetingTimeChecker()));
					}
				}
			}
			return retList;
		}

		private IEnumerable<IWorkShift> getShiftFromMasterActivity(IWorkShift workShift)
		{
			return _shiftFromMasterActivityService.Generate(workShift);
		}
	}
}
