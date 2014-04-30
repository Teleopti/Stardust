using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Logging
{
    public class SchedulingOptionsValueExtractor
    {
        private readonly ISchedulingOptions _schedulingOptions;

        public SchedulingOptionsValueExtractor(ISchedulingOptions schedulingOptions)
        {
            _schedulingOptions = schedulingOptions;
        }

        public string GetTeamOptions()
        {
            var result = _schedulingOptions.GroupOnGroupPageForTeamBlockPer.Name;
            if (_schedulingOptions.TeamSameShiftCategory)
                result += ",Same shift category";
            if (_schedulingOptions.TeamSameStartTime)
                result += ",Same start time";
            if (_schedulingOptions.TeamSameActivity)
                result += ",Same activity";
            if (_schedulingOptions.TeamSameEndTime)
                result += ",Same end time";
            return result;
        }

        public string GetBlockOptions()
        {
            var result = _schedulingOptions.BlockFinderTypeForAdvanceScheduling.ToString();
            if (_schedulingOptions.BlockSameShiftCategory)
                result += ",Same shift category";
				if (_schedulingOptions.BlockSameShift)
                result += ",Same shift";
            if (_schedulingOptions.BlockSameStartTime)
                result += ",Same start time";
            return result;
        }

        
        public string GetGeneralSchedulingOptions()
        {
            var result = "Scheduling";
            if (_schedulingOptions.UsePreferences)
                result += ",Prefrences";
            if (_schedulingOptions.UseRotations)
                result += ",Rotations";
            return result;
        }

        public string GetGeneralOptimizationOptions(IOptimizationPreferences optimizationPreferences)
        {
            var result = optimizationPreferences.Advanced.TargetValueCalculation.ToString();
            if (optimizationPreferences.General.OptimizationStepDaysOff)
                result += ",Dayoff Optimization";
            if (optimizationPreferences.General.OptimizationStepShiftsWithinDay )
                result += ",Shift within day";
            if (optimizationPreferences.General.OptimizationStepTimeBetweenDays )
                result += ",Intraday";
            if (optimizationPreferences.General.UsePreferences )
                result += ",Prefrences";
            if (optimizationPreferences.General.UseMustHaves )
                result += ",Must Have";
            if (optimizationPreferences.General.UseRotations )
                result += ",Rotations";
            if (optimizationPreferences.General.UseAvailabilities )
                result += ",Availability";
            return result;
        }
       
    }
}
