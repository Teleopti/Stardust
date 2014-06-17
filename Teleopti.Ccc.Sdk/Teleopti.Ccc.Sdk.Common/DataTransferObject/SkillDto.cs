using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Class representing a Teleopti WFM Skill
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SkillDto: Dto
    {
        /// <summary>
        /// Gets the name of the Skill.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets the display color of the Skill.
        /// </summary>
        /// <value>The display color.</value>
        [DataMember]
        public ColorDto DisplayColor { get; set; }

        /// <summary>
        /// Gets the type of the skill.
        /// </summary>
        /// <value>The type of the skill.</value>
        [DataMember]
        public string SkillType { get; set; }

        /// <summary>
        /// Gets the description of the Skill.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets the minimum resolution (in minutes) the Skill data can be devided into.
        /// </summary>
        /// <value>The resolution.</value>
        [DataMember]
        public int Resolution { get; set; }

        /// <summary>
        /// Gets the activity that is connected to the Skill.
        /// </summary>
        /// <value>The activity.</value>
        [DataMember]
        public ActivityDto Activity { get; set; }
    }
}