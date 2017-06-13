
using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class ChildSkill : Skill, IChildSkill
    {
        private IMultisiteSkill _parentSkill;

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

		    Activity = _parentSkill.Activity;
		    TimeZone = _parentSkill.TimeZone;
		    MidnightBreakOffset = _parentSkill.MidnightBreakOffset;
	    }

		/// <summary>
		/// Sets the parent skill.
		/// </summary>
		/// <param name="parentSkill">The parent skill.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-21
		/// </remarks>
		public virtual void SetParentSkill(IMultisiteSkill parentSkill)
        {
            _parentSkill = parentSkill;
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