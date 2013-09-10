using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class UpdateSelectionForShiftDistribution
    {
        public void Update(IList<IScheduleDay> allSchedules, ScheduleViewBase scheduleView, 
                            ShiftDistributionControl shiftDistributionControl, 
            ShiftFairnessAnalysisControl shiftFairnessAnalysisControl, ShiftPerAgentControl shiftPerAgentControl)
        {
            if (scheduleView != null)
            {
                var shiftCategoryDistributionExtractor = new DistributionInformationExtractor(allSchedules);
                shiftDistributionControl.UpdateModel(shiftCategoryDistributionExtractor );
                shiftFairnessAnalysisControl.UpdateModel(shiftCategoryDistributionExtractor );
                shiftPerAgentControl.UpdateModel(shiftCategoryDistributionExtractor);
            }
        }
    }
}
