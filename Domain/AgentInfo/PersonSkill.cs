using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    public class PersonSkill : AggregateEntity, IPersonSkill, IPersonSkillModify
    {
        private readonly ISkill _skill;
        private Percent _skillPercentage;
    	private bool _active;

		protected PersonSkill()
		{
		}

		public PersonSkill(ISkill skill, Percent skillPercentage)
			: this()
		{
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(skillPercentage), skillPercentage);

			_skill = skill;
			_skillPercentage = skillPercentage;
			_active = true;
		}

		public virtual bool HasActivity(IActivity activity)
		{
			return Skill.Activity?.Equals(activity) ?? Skill.Activity == null;
		}

		public virtual ISkill Skill => _skill;

	    public virtual Percent SkillPercentage
        {
            get { return _skillPercentage; }
            set
            {
                _skillPercentage = value;
            }
        }

		public virtual bool Active
    	{
			get { return _active; }
    		set { _active = value; }
    	}

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

		public virtual IPersonSkill NoneEntityClone()
        {
            var retobj = (PersonSkill)MemberwiseClone();
            retobj.SetId(null);
            return retobj;
        }

        public virtual IPersonSkill EntityClone()
        {
            var retobj = (PersonSkill)MemberwiseClone();
            return retobj;
        }

		public override bool Equals(IEntity other)
		{
			if (!(other is IPersonSkill otherPersonSkill))
				return false;

			return _skill.Equals(otherPersonSkill.Skill);
		}

		public override int GetHashCode()
		{
			return _skill.GetHashCode();
		}
	}
}
