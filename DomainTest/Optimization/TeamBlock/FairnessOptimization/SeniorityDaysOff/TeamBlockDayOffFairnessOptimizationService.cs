using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public  interface ITeamBlockDayOffFairnessOptimizationService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                                     IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);
    }

    public class TeamBlockDayOffFairnessOptimizationService : ITeamBlockDayOffFairnessOptimizationService
    {
        private bool _cancelMe;
        private readonly IDayOffStep1 _dayOffStep1;
        private readonly IDayOffStep2 _dayOffStep2;

        public TeamBlockDayOffFairnessOptimizationService(IDayOffStep1 dayOffStep1, IDayOffStep2 dayOffStep2)
        {
            _dayOffStep1 = dayOffStep1;
            _dayOffStep2 = dayOffStep2;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _cancelMe = false;
            var instance = PrincipalAuthorization.Instance();
            if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction)) return;
            
            var weekDayPoints = new WeekDayPoints();
            
            _dayOffStep1.PerformStep1(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons,
                                      rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints());

            if (!schedulingOptions.UseSameDayOffs)
                _dayOffStep2.PerformStep2();
            

        }
        
        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public virtual void OnReportProgress(string message)
        {
            var handler = ReportProgress;
            if (handler == null) return;
            var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
            handler(this, args);
            if (args.Cancel) _cancelMe = true;
        }
        
    }
}
