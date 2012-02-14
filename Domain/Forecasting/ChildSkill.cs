
using System.Drawing;
using Teleopti.Interfaces.Domain;

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="displayColor">The display color.</param>
        /// <param name="defaultSolution">The default solution.</param>
        /// <param name="skillType">Type of the skill.</param>
        public ChildSkill(string name, string description, Color displayColor, int defaultSolution, ISkillType skillType) : 
            base(name,description,displayColor,defaultSolution,skillType)
        {
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
        public virtual IMultisiteSkill ParentSkill
        {
            get { return _parentSkill; }
        }
    }
}