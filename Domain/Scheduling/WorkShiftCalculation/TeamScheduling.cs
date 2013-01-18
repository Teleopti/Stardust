using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface ITeamScheduling
    {
        void Execute(IList<DateOnly> selectedDays, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson, IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache);
    }

    public  class TeamScheduling : ITeamScheduling
    {
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly ITeamSteadyStateHolder _teamSteadyStateHolder;
        private readonly ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public TeamScheduling(ISchedulingOptions schedulingOptions, 
                              ITeamSteadyStateHolder teamSteadyStateHolder, 
                              ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
                              IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,IGroupSchedulingService groupSchedulingService)
        {
            _schedulingOptions = schedulingOptions;
            _teamSteadyStateHolder = teamSteadyStateHolder;
            _teamSteadyStateMainShiftScheduler = teamSteadyStateMainShiftScheduler;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _groupSchedulingService = groupSchedulingService;
        }


        public void  Execute(IList<DateOnly  > selectedDays, IList<IScheduleMatrixPro> matrixList,
                IGroupPerson groupPerson,IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache   )
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");

            _groupSchedulingService.ExecuteForAdvanceSchedulingService(selectedDays, matrixList, _schedulingOptions, groupPerson,
                                            new BackgroundWorker(), _teamSteadyStateHolder,
                                            _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization, effectiveRestriction, shiftProjectionCache);
        }
    }
}