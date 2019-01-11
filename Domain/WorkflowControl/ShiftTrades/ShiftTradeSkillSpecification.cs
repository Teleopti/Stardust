using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeSkillSpecification : Specification<ShiftTradeAvailableCheckItem>, IShiftTradeLightSpecification
	{
		public override bool IsSatisfiedBy(ShiftTradeAvailableCheckItem obj)
		{
			var controlSetFrom = obj.PersonFrom.WorkflowControlSet;
			var controlSetTo = obj.PersonTo.WorkflowControlSet;
			if (controlSetFrom == null || controlSetTo == null) return false;

			var mustMatchingSkills = getListOfSkills(controlSetFrom.MustMatchSkills, controlSetTo.MustMatchSkills);
			if (mustMatchingSkills.Count == 0)
				return true;
			var periodFrom = obj.PersonFrom.Period(obj.DateOnly);
			var periodTo = obj.PersonTo.Period(obj.DateOnly);
			if (periodFrom == null || periodTo == null)
				return false;
			ICollection<ISkill> skills = new HashSet<ISkill>();
			foreach (var personSkill in periodFrom.PersonSkillCollection.Where(personSkill => mustMatchingSkills.Contains(personSkill.Skill)))
			{
				skills.Add(personSkill.Skill);
			}

			foreach (var personSkill in periodTo.PersonSkillCollection.Where(personSkill => mustMatchingSkills.Contains(personSkill.Skill)))
			{
				if (skills.Contains(personSkill.Skill))
				{
					skills.Remove(personSkill.Skill);
				}
				else
				{
					skills.Add(personSkill.Skill);
				}
			}
			return skills.Count == 0;
		}

		private static IList<ISkill> getListOfSkills(IEnumerable<ISkill> listOne, IEnumerable<ISkill> listTwo)
		{
			return listOne.Concat(listTwo).Distinct().ToList();
		}

		public string DenyReason => nameof(Resources.ShiftTradeSkillDenyReason);
	}
}
