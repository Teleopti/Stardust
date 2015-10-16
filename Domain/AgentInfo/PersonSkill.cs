using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

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
			InParameter.NotNull("skill", skill);
			InParameter.NotNull("skillPercentage", skillPercentage);

			_skill = skill;
			_skillPercentage = skillPercentage;
			_active = true;
		}

        public virtual ISkill Skill
        {
            get { return _skill; }
        }

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
    }
}
