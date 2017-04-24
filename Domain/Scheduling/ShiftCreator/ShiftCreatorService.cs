using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class ShiftCreatorService : IShiftCreatorService
    {
        private readonly ICreateWorkShiftsFromTemplate _shiftGenerator;

        public ShiftCreatorService(ICreateWorkShiftsFromTemplate shiftGenerator)
        {
            _shiftGenerator = shiftGenerator;
        }
		
		public IList<WorkShiftCollection> Generate(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback)
	    {
			var retColl = new List<WorkShiftCollection>();
			using (PerformanceOutput.ForOperation("Generating workshifts for " + ruleSet.Description.Name))
			{
				var templates = ruleSet.TemplateGenerator.Generate();
				
				foreach (var template in templates)
				{
					if (callback != null && callback.IsCanceled)
						break;
					retColl.Add(_shiftGenerator.Generate(template, ruleSet.ExtenderCollection,
															  ruleSet.LimiterCollection, callback));
				}
			}
			return retColl;
	    }
    }
}