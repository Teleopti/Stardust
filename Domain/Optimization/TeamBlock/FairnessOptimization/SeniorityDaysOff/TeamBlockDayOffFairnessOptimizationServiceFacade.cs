using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ITeamBlockDayOffFairnessOptimizationServiceFacade
    {
        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences);

        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }

    public class TeamBlockDayOffFairnessOptimizationServiceFacade : ITeamBlockDayOffFairnessOptimizationServiceFacade
    {
        private readonly IDayOffStep1 _dayOffStep1;
        private readonly IDayOffStep2 _dayOffStep2;
        private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
	    private readonly IToggleManager _toggleManager;
	    private bool _cancelMe;

        public TeamBlockDayOffFairnessOptimizationServiceFacade(IDayOffStep1 dayOffStep1, IDayOffStep2 dayOffStep2, ITeamBlockSchedulingOptions teamBlockSchedulingOptions, IToggleManager toggleManager)
        {
            _dayOffStep1 = dayOffStep1;
            _dayOffStep2 = dayOffStep2;
            _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
	        _toggleManager = toggleManager;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences)
        {
            _cancelMe = false;
			if(!_toggleManager.IsEnabled(Toggles.TeamBlue_Seniority_Temporay)) return;
            var weekDayPoints = new WeekDayPoints();
            _dayOffStep1.BlockSwapped += ReportProgress;
            _dayOffStep1.PerformStep1(allPersonMatrixList, selectedPeriod, selectedPersons,rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
                                      optimizationPreferences );
            _dayOffStep1.BlockSwapped -= ReportProgress;

            if (
                !(_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions) && schedulingOptions.UseSameDayOffs) &&
                !_cancelMe)
            {
                _dayOffStep2.BlockSwapped += ReportProgress;
                _dayOffStep2.PerformStep2(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons, rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
                                          optimizationPreferences);
                _dayOffStep2.BlockSwapped -= ReportProgress;
            }
                
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public virtual void OnReportProgress(ResourceOptimizerProgressEventArgs eventArgs)
        {
            EventHandler<ResourceOptimizerProgressEventArgs> temp = ReportProgress;
            if (temp != null)
            {
                temp(this, eventArgs);
            }
            if (eventArgs.Cancel)
                _cancelMe = true;
        }

    }
}