using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ITeamBlockDayOffFairnessOptimizationService
    {
        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator);
    }

    public class TeamBlockDayOffFairnessOptimizationService : ITeamBlockDayOffFairnessOptimizationService
    {
        private readonly IDayOffStep1 _dayOffStep1;
        private readonly IDayOffStep2 _dayOffStep2;

        public TeamBlockDayOffFairnessOptimizationService(IDayOffStep1 dayOffStep1, IDayOffStep2 dayOffStep2)
        {
            _dayOffStep1 = dayOffStep1;
            _dayOffStep2 = dayOffStep2;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
        {
            IPrincipalAuthorization instance = PrincipalAuthorization.Instance();
            if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction)) return;
            var weekDayPoints = new WeekDayPoints();
            _dayOffStep1.PerformStep1(allPersonMatrixList, selectedPeriod, selectedPersons,rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
                                      optimizationPreferences,teamBlockRestrictionOverLimitValidator );

            if (schedulingOptions.UseSameDayOffs)
                _dayOffStep2.PerformStep2(schedulingOptions, allPersonMatrixList, selectedPeriod, selectedPersons,rollbackService, scheduleDictionary, weekDayPoints.GetWeekDaysPoints(),
                                          optimizationPreferences);
        }
    }
}