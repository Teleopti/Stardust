﻿using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class UpdateSelectionForShiftDistribution
    {
        private readonly DistributionInformationExtractor _distributionInformationExtractor;

        public UpdateSelectionForShiftDistribution(DistributionInformationExtractor distributionInformationExtractor )
        {
            _distributionInformationExtractor = distributionInformationExtractor;
        }

        public void Update(IList<IScheduleDay> allSchedules, ScheduleViewBase scheduleView, 
                            ShiftDistributionControl shiftDistributionControl, 
            ShiftFairnessAnalysisControl shiftFairnessAnalysisControl, ShiftPerAgentControl shiftPerAgentControl)
        {
            if (scheduleView != null)
            {
                //var shiftCategoryDistributionExtractor = new DistributionInformationExtractor(allSchedules);
                _distributionInformationExtractor.ExtractDistributionInfo(allSchedules);
                shiftDistributionControl.UpdateModel(_distributionInformationExtractor);
                shiftFairnessAnalysisControl.UpdateModel(_distributionInformationExtractor);
                shiftPerAgentControl.UpdateModel(_distributionInformationExtractor);
            }
        }
    }
}
