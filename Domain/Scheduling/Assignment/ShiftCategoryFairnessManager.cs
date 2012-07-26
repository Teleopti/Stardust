using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IShiftCategoryFairnessManager
    {
        IShiftCategoryFairnessFactors GetFactorsForPersonOnDate(IPerson person, DateOnly dateOnly);
    }

    public class ShiftCategoryFairnessManager : IShiftCategoryFairnessManager
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
        private readonly IShiftCategoryFairnessCalculator _fairnessCalculator;

        public ShiftCategoryFairnessManager(ISchedulingResultStateHolder schedulingResultStateHolder, IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator,
            IShiftCategoryFairnessCalculator fairnessCalculator)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _groupShiftCategoryFairnessCreator = groupShiftCategoryFairnessCreator;
            _fairnessCalculator = fairnessCalculator;
        }

        private IScheduleDictionary ScheduleDictionary
        {
            get { return _schedulingResultStateHolder.Schedules; }
        }

        public IShiftCategoryFairnessFactors GetFactorsForPersonOnDate(IPerson person, DateOnly dateOnly)
        {
			var personFairness = ScheduleDictionary[person].CachedShiftCategoryFairness();
			var groupFairness = _groupShiftCategoryFairnessCreator.CalculateGroupShiftCategoryFairness(person, dateOnly);

			//bool useShiftCategoryFairness = false;
			//if (person.WorkflowControlSet != null)
			//    useShiftCategoryFairness = person.WorkflowControlSet.UseShiftCategoryFairness;
			return _fairnessCalculator.ShiftCategoryFairnessFactors(groupFairness, personFairness);
        }
    }
}