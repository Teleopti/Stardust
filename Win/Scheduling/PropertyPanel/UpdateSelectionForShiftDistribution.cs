﻿using System;
using System.Collections.Generic;
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

        public void Update(ShiftDistributionControl shiftDistributionControl, ShiftFairnessAnalysisControl shiftFairnessAnalysisControl, ShiftPerAgentControl shiftPerAgentControl)
        {
			//if (scheduleView != null)
			//{
                //_distributionInformationExtractor.ExtractDistributionInfo(allSchedules);
				//_distributionInformationExtractor.ExtractDistributionInfo(allSchedules, lastModifiedPart, timeZoneInfo);
                //shiftDistributionControl.UpdateModel(_distributionInformationExtractor);
                //shiftFairnessAnalysisControl.UpdateModel(_distributionInformationExtractor);
                //shiftPerAgentControl.UpdateModel(_distributionInformationExtractor);
			//}
        }
    }
}
