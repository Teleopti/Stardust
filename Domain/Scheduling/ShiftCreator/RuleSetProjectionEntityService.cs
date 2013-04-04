using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class RuleSetProjectionEntityService : IRuleSetProjectionEntityService
    {
        private readonly IShiftCreatorService _shiftCreatorService;

        public RuleSetProjectionEntityService(IShiftCreatorService shiftCreatorService)
        {
            _shiftCreatorService = shiftCreatorService;
        }

	    public IEnumerable<IWorkShiftVisualLayerInfo> ProjectionCollection(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback)
	    {
			var retList = new List<IWorkShiftVisualLayerInfo>();

			var workShifts = _shiftCreatorService.Generate(ruleSet,callback);
			foreach (var workShift in workShifts)
			{
				if (callback != null && callback.IsCanceled)
					break;
				retList.Add(new WorkShiftVisualLayerInfo(workShift, workShift.Projection));
			}

			return retList;
	    }
    }
}
