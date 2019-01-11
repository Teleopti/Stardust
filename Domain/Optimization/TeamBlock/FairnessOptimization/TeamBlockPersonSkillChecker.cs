using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockPersonsSkillChecker
	{
		bool PersonsHaveSameSkills(IPersonPeriod personPeriodOne, IPersonPeriod personPeriodTwo);
	}
	public class TeamBlockPersonsSkillChecker : ITeamBlockPersonsSkillChecker
	{
		public bool PersonsHaveSameSkills(IPersonPeriod personPeriodOne, IPersonPeriod personPeriodTwo)
		{
			var personSkills1 = personPeriodOne.PersonSkillCollection.Where(ps => ps.Active);
			var personSkills2 = personPeriodTwo.PersonSkillCollection.Where(ps => ps.Active);

			// ??? måste vi ta hänsyn till procenten, nej, Anders säger att vi skiter i det.
			var skills1 = personSkills1.Select(personSkill => personSkill.Skill).ToArray();
			var skills2 = personSkills2.Select(personSkill => personSkill.Skill).ToArray();
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
