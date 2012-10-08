using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessPersonsSwappableChecker
	{
		bool PersonsAreSwappable(IPerson personOne, IPerson personTwo, DateOnly onDate);
	}
	public class ShiftCategoryFairnessPersonsSwappableChecker : IShiftCategoryFairnessPersonsSwappableChecker
	{
		private readonly IShiftCategoryFairnessPersonsSkillChecker _personsSkillChecker;
	    private readonly IShiftCategoryFairnessRuleSetChecker _ruleSetChecker;
	    private readonly IShiftCategoryFairnessContractTimeChecker _contractTimeChecker;

		public ShiftCategoryFairnessPersonsSwappableChecker(IShiftCategoryFairnessPersonsSkillChecker personsSkillChecker,
            IShiftCategoryFairnessRuleSetChecker ruleSetChecker,
            IShiftCategoryFairnessContractTimeChecker contractTimeChecker)
		{
			_personsSkillChecker = personsSkillChecker;
		    _ruleSetChecker = ruleSetChecker;
		    _contractTimeChecker = contractTimeChecker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool PersonsAreSwappable(IPerson personOne, IPerson personTwo, DateOnly onDate)
		{
			//check Skills (each check in own class later)
			var personPeriodOne = personOne.Period(onDate);
			var personPeriodTwo = personTwo.Period(onDate);
			if (personPeriodOne == null || personPeriodTwo == null)
				return false;
			if (!_personsSkillChecker.PersonsHaveSameSkills(personPeriodOne, personPeriodTwo))
				return false;
            if (!_ruleSetChecker.Check(personPeriodOne, personPeriodTwo))
                return false;
            if (!_contractTimeChecker.Check(personPeriodOne, personPeriodTwo))
                return false;
            

			return true;
		}
	}
}