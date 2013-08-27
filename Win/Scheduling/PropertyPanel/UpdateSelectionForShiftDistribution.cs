using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class UpdateSelectionForShiftDistribution
    {
        public void Update(IList<IScheduleDay> allSchedules, ScheduleViewBase scheduleView, ShiftDistributionAnalysisControl shiftDistributionAnalysisControl, ShiftFairnessAnalysisControl shiftFairnessAnalysisControl)
        {
            if (scheduleView != null)
            {
                var shiftCategoryDistributionExtractor = new DistributionInformationExtractor(allSchedules);
                shiftDistributionAnalysisControl.UpdateModel(shiftCategoryDistributionExtractor );
                shiftFairnessAnalysisControl.UpdateModel(shiftCategoryDistributionExtractor );
            }
        }
    }
}
