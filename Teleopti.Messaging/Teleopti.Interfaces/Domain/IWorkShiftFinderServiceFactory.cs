
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Create IWorkShiftFinderService
    /// </summary>
    public interface IWorkShiftFinderServiceFactory
    {
        /// <summary>
        /// Creates the finder service with scheduling object container.
        /// </summary>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="options">The options.</param>
        /// <param name="mainShiftOptimizeActivitiesSpecification">The main shift optimize activities spcification.</param>
        /// <param name="shiftCategoryFairnessShiftValueCalculator">The shift category fairness shift value calculator.</param>
        /// <param name="ruleSetProjectionEntityService">The rule set projection service.</param>
        /// <param name="groupShiftCategoryFairnessCreator"></param>
        /// <returns></returns>
        IWorkShiftFinderService CreateFinderServiceWithSchedulingObjectContainer(ISchedulingResultStateHolder
                                                                                     stateHolder,
                                                                                 ISchedulingOptions options,
                                                                                 ISpecification<IMainShift>
                                                                                     mainShiftOptimizeActivitiesSpecification,
                                                                                 IShiftCategoryFairnessShiftValueCalculator 
                                                                                     shiftCategoryFairnessShiftValueCalculator,
            IRuleSetProjectionEntityService ruleSetProjectionEntityService, IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator);
    }
}