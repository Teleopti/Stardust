using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    

    public class WorkShiftFinderServiceFactory : IWorkShiftFinderServiceFactory
    {
        //public IWorkShiftFinderService Create(ISchedulingResultStateHolder stateHolder, ISchedulingObjectContainer schedulingObjectContainer)
        //{
        //    return new WorkShiftFinderService(stateHolder, schedulingObjectContainer);
        //}


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public IWorkShiftFinderService CreateFinderServiceWithSchedulingObjectContainer( ISchedulingResultStateHolder stateHolder,
            ISchedulingOptions options, ISpecification<IMainShift> mainShiftOptimizeActivitiesSpecification, IShiftCategoryFairnessShiftValueCalculator shiftCategoryFairnessShiftValueCalculator,
            IRuleSetProjectionService ruleSetProjectionService, IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator)
        {
            var fairnessValueCalculator = new FairnessValueCalculator(options);
            var personSkillPeriodsDataHolderManager = new PersonSkillPeriodsDataHolderManager(stateHolder);
            var preSchedulingStatusChecker = new PreSchedulingStatusChecker();

            var rules = new LongestPeriodForAssignmentCalculator();

            var shiftProjectionCacheFilter = new ShiftProjectionCacheFilter(rules, mainShiftOptimizeActivitiesSpecification);
            var shiftProjectionCacheManager = new ShiftProjectionCacheManager(new ShiftFromMasterActivityService(), new RuleSetDeletedActivityChecker(), new RuleSetDeletedShiftCategoryChecker(), ruleSetProjectionService);
            var workShiftCalculator = new WorkShiftCalculator();
            
            var fairnessManager = new ShiftCategoryFairnessManager(stateHolder, groupShiftCategoryFairnessCreator,
                                                                   new ShiftCategoryFairnessCalculator()); 

            var finderService = new WorkShiftFinderService(stateHolder,preSchedulingStatusChecker,
                new SeatLimitationWorkShiftCalculator2(new SeatImpactOnPeriodForProjection()),shiftProjectionCacheFilter, personSkillPeriodsDataHolderManager, 
                shiftCategoryFairnessShiftValueCalculator, shiftProjectionCacheManager,workShiftCalculator,fairnessValueCalculator,
                new NonBlendWorkShiftCalculator(new NonBlendSkillImpactOnPeriodForProjection(), workShiftCalculator), fairnessManager);
            
            return finderService;
            
        }
    }
}
