using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class RuleSetProjectionEntityService : IRuleSetProjectionEntityService
    {
        private readonly IShiftCreatorService _shiftCreatorService;

        public RuleSetProjectionEntityService(IShiftCreatorService shiftCreatorService)
        {
            _shiftCreatorService = shiftCreatorService;
        }

	    public IEnumerable<WorkShiftVisualLayerInfo> ProjectionCollection(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback)
	    {
			var retList = new List<WorkShiftVisualLayerInfo>();

			var workShifts = _shiftCreatorService.Generate(ruleSet,callback);
			foreach (var workShift in workShifts)
			{
				foreach (var w in workShift)
				{
					if (callback != null && callback.IsCanceled)
						break;
					retList.Add(new WorkShiftVisualLayerInfo(w, w.Projection));
				}
			}

			return retList;
	    }
    }
}
