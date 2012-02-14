using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class RuleSetProjectionService : IRuleSetProjectionService
    {
        private readonly IShiftCreatorService _shiftCreatorService;

        public RuleSetProjectionService(IShiftCreatorService shiftCreatorService)
        {
            _shiftCreatorService = shiftCreatorService;
        }

        public virtual IEnumerable<IWorkShiftVisualLayerInfo> ProjectionCollection(IWorkShiftRuleSet ruleSet)
        {
            var retList = new List<IWorkShiftVisualLayerInfo>();

            var workShifts = _shiftCreatorService.Generate(ruleSet);
            foreach (var workShift in workShifts)
            {
                retList.Add(new WorkShiftVisualLayerInfo(workShift, workShift.Projection));
            }

            return retList;
        }
    }
}
