using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class ShiftCreatorService : IShiftCreatorService
    {
        private readonly ICreateWorkShiftsFromTemplate _shiftGenerator;

        public ShiftCreatorService(ICreateWorkShiftsFromTemplate shiftGenerator)
        {
            _shiftGenerator = shiftGenerator;
        }

        public ShiftCreatorService()
        {
            _shiftGenerator = new CreateWorkShiftsFromTemplate();
        }

        public ICreateWorkShiftsFromTemplate WorkShiftGenerator
        {
            get { return _shiftGenerator; }
        }

		public IList<IWorkShift> Generate(IWorkShiftRuleSet ruleSet)
        {
            List<IWorkShift> retColl = new List<IWorkShift>();
            using (PerformanceOutput.ForOperation("Generating workshifts for " + ruleSet.Description.Name))
            {
                IList<IWorkShift> templates = ruleSet.TemplateGenerator.Generate();
                foreach (IWorkShift template in templates)
                {
                    retColl.AddRange(_shiftGenerator.Generate(template, ruleSet.ExtenderCollection,
                                                              ruleSet.LimiterCollection));
                }
            }
		    return retColl;
        }
    }
}