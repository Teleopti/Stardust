using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessPersonsSkillChecker
	{
		bool PersonsHaveSameSkills(IPersonPeriod personPeriodOne, IPersonPeriod personPeriodTwo);
	}
	public class ShiftCategoryFairnessPersonsSkillChecker : IShiftCategoryFairnessPersonsSkillChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool PersonsHaveSameSkills(IPersonPeriod personPeriodOne, IPersonPeriod personPeriodTwo)
		{
			var personSkills1 = personPeriodOne.PersonSkillCollection;
			var personSkills2 = personPeriodTwo.PersonSkillCollection;

			// ??? måste vi ta hänsyn till procenten, nej, Anders säger att vi skiter i det.
			var skills1 = personSkills1.Select(personSkill => personSkill.Skill).ToList();
			var skills2 = personSkills2.Select(personSkill => personSkill.Skill).ToList();
			if (skills1.Any(skill => !skills2.Contains(skill)))
			{
				return false;
			}
			if (skills2.Any(skill => !skills1.Contains(skill)))
			{
				return false;
			}

			return true;
		}

	}
}