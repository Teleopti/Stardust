using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public class TeamBlockDayOffFairnessOptimizationServiceFacade
    {
        private readonly IDayOffStep1 _dayOffStep1;
        private readonly IDayOffStep2 _dayOffStep2;
        private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

        public TeamBlockDayOffFairnessOptimizationServiceFacade(IDayOffStep1 dayOffStep1, IDayOffStep2 dayOffStep2, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
        {
            _dayOffStep1 = dayOffStep1;
            _dayOffStep2 = dayOffStep2;
            _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
        }

		public void Execute(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, SchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, 
							ISeniorityWorkDayRanks seniorityWorkDayRanks, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
        {
            var weekDayPoints = new WeekDayPoints();
            _dayOffStep1.BlockSwapped += ReportProgress;
            _dayOffStep1.PerformStep1(allPersonMatrixList, selectedPeriod, selectedPersons,rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(seniorityWorkDayRanks),
                                      optimizationPreferences, dayOffOptimizationPreferenceProvider);
            _dayOffStep1.BlockSwapped -= ReportProgress;

            if (!(_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions) && schedulingOptions.UseSameDayOffs))
            {
		            _dayOffStep2.BlockSwapped += ReportProgress;
		            _dayOffStep2.PerformStep2(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons,
			            rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(seniorityWorkDayRanks), optimizationPreferences, dayOffOptimizationPreferenceProvider);
		            _dayOffStep2.BlockSwapped -= ReportProgress;
            }
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }
}