﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ITeamBlockDayOffFairnessOptimizationServiceFacade
    {
        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, bool scheduleSeniority11111);

        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }

	public class TeamBlockDayOffFairnessOptimizationServiceFacadeSeniorityTurnedOff :
		ITeamBlockDayOffFairnessOptimizationServiceFacade
	{
		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, bool scheduleSeniority11111)
		{
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress {add{} remove{}}
	}

    public class TeamBlockDayOffFairnessOptimizationServiceFacade : ITeamBlockDayOffFairnessOptimizationServiceFacade
    {
        private readonly IDayOffStep1 _dayOffStep1;
        private readonly IDayOffStep2 _dayOffStep2;
        private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
	    private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

        public TeamBlockDayOffFairnessOptimizationServiceFacade(IDayOffStep1 dayOffStep1, IDayOffStep2 dayOffStep2, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
        {
            _dayOffStep1 = dayOffStep1;
            _dayOffStep2 = dayOffStep2;
            _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
        }

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, bool scheduleSeniority11111)
        {
            _cancelMe = false;
	        _progressEvent = null;
            var weekDayPoints = new WeekDayPoints();
            _dayOffStep1.BlockSwapped += ReportProgress;
            _dayOffStep1.PerformStep1(allPersonMatrixList, selectedPeriod, selectedPersons,rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
                                      optimizationPreferences, scheduleSeniority11111);
            _dayOffStep1.BlockSwapped -= ReportProgress;

            if (
                !(_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions) && schedulingOptions.UseSameDayOffs) &&
                !_cancelMe)
            {

	            if (_progressEvent == null || !_progressEvent.UserCancel)
	            {
		            _dayOffStep2.BlockSwapped += ReportProgress;
		            _dayOffStep2.PerformStep2(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons,
			            rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
						optimizationPreferences, scheduleSeniority11111);
		            _dayOffStep2.BlockSwapped -= ReportProgress;
	            }
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

			if (_progressEvent != null && _progressEvent.UserCancel) return;
	        _progressEvent = eventArgs;
        }

    }
}