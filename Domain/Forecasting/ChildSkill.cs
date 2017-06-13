using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class ChildSkill : Skill, IChildSkill
    {
        private readonly IMultisiteSkill _parentSkill;

        /// <summary>
        /// For NHibernate
        /// </summary>
        protected ChildSkill()
        {
        }
		
	    public ChildSkill(string name, string description, Color displayColor, IMultisiteSkill parentSkill) :
		    base(name, description, displayColor, parentSkill.DefaultResolution, parentSkill.SkillType)
	    {
		    _parentSkill = parentSkill;
			_parentSkill.AddChildSkill(this);
	    }
		
	    public override IActivity Activity
	    {
		    get { return _parentSkill.Activity; }
		    set { }
	    }

	    public override TimeSpan MidnightBreakOffset
	    {
		    get { return _parentSkill.MidnightBreakOffset; }
		    set {}
	    }

	    protected internal override string TimeZoneId
	    {
		    get { return _parentSkill.TimeZone.Id; }
		    set { }
	    }

	    /// <summary>
        /// Gets the parent skill.
        /// </summary>
        /// <value>The parent skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        public virtual IMultisiteSkill ParentSkill => _parentSkill;
    }
}