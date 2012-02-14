

using System;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    /// <summary>
    /// This class is used for skill viewer 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-03-17
    /// </remarks>
    public class PersonSkillViewer
    {
        private Skill _skill;
        private int _triState;
        private string _triStateValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSkillViewer"/> class.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-17
        /// </remarks>
        public PersonSkillViewer(Skill skill)
        {
            _skill = skill;
        }


        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-17
        /// </remarks>
        public Skill Skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        /// <summary>
        /// Gets the name of the skill.
        /// </summary>
        /// <value>The name of the skill.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-17
        /// </remarks>
        public string SkillName
        {
            get { return _skill.Name; }
        }

        /// <summary>
        /// Gets the skill identifier.
        /// </summary>
        /// <value>The skill identifier.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-17
        /// </remarks>
        public  Guid? SkillIdentifier
        {
            get { return _skill.Id; }
        }


        /// <summary>
        /// Gets or sets the state of the tri.
        /// </summary>
        /// <value>The state of the tri.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-17
        /// </remarks>
        public int TriState
        {
            get { return _triState; }
            set { _triState = value; }
        }

        /// <summary>
        /// Gets or sets the tri state value.
        /// </summary>
        /// <value>The tri state value.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-04-01
        /// </remarks>
        public string TriStateValue
        {
            get
            {
                return _triStateValue;
            }
            set
            {
                _triStateValue = value;
            }
        }



    }
}
