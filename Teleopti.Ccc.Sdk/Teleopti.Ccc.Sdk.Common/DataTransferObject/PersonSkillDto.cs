using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Detailed information about a skill connected to a person period.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
    public class PersonSkillDto : Dto
    {
        /// <summary>
        /// Gets or sets the skill id.
        /// </summary>
        [DataMember]
        public Guid SkillId { get; set; }

        /// <summary>
        /// Gets or sets the proficiency.
        /// </summary>
        /// <remarks>1.0 equals 100%, 0.5 equals 50%.</remarks>
        [DataMember]
        public double Proficiency { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if this skill is active during the person period.
        /// </summary>
        [DataMember]
        public bool Active { get; set; }
    }
}