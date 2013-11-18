using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.Fairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
    
    public interface IFairnessOptimizationCommand
    {
        void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson, IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories);
    }
    public class FairnessOptimizationCommand : IFairnessOptimizationCommand 
    {
        private readonly IFairnessOptimization  _fairnessOptmization;

        public FairnessOptimizationCommand(IFairnessOptimization fairnessOptmization)
        {
            _fairnessOptmization = fairnessOptmization;
        }

        public void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson, IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories)
        {
            _fairnessOptmization.Execute(selectedPeriod, selectedPerson, scheduleDays,shiftCategories );
        }
    }
}
