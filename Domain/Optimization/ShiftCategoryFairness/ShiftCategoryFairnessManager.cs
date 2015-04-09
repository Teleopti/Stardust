using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessManager
    {
        IShiftCategoryFairnessFactors GetFactorsForPersonOnDate(IPerson person, DateOnly dateOnly);
    }

    public class ShiftCategoryFairnessManager : IShiftCategoryFairnessManager
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
        private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
        private readonly IShiftCategoryFairnessCalculator _fairnessCalculator;

        public ShiftCategoryFairnessManager(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator,
            IShiftCategoryFairnessCalculator fairnessCalculator)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _groupShiftCategoryFairnessCreator = groupShiftCategoryFairnessCreator;
            _fairnessCalculator = fairnessCalculator;
        }

        public IShiftCategoryFairnessFactors GetFactorsForPersonOnDate(IPerson person, DateOnly dateOnly)
        {
			var personFairness = _schedulingResultStateHolder().Schedules[person].CachedShiftCategoryFairness();
			var groupFairness = _groupShiftCategoryFairnessCreator.CalculateGroupShiftCategoryFairness(person, dateOnly);

			return _fairnessCalculator.ShiftCategoryFairnessFactors(groupFairness, personFairness);
        }
    }
}