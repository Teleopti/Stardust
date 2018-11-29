using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class SkillFilter : Entity, IFilter
	{
		public virtual ISkill Skill { get; protected set; }

		protected SkillFilter()
		{
		}

		public SkillFilter(ISkill skill)
		{
			Skill = skill;
		}

		public virtual bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && personPeriod.PersonSkillCollection.Any(x => x.Active && x.Skill.Equals(Skill));
		}

		public virtual string FilterType => "skill";

		public override bool Equals(IEntity other)
		{
			return other is SkillFilter otherSkillFilter && Skill.Equals(otherSkillFilter.Skill);
		}

		public override int GetHashCode()
		{
			return Skill.GetHashCode();
		}
	}
}