using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Data about one skill day and also containing the intraday skill data
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SkillDayDto
    {
        /// <summary>
        /// Gets or sets the skill id.
        /// </summary>
        /// <value>The skill id.</value>
        [DataMember]
        public Guid SkillId { get; set; }

        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        /// <value>The name of the skill.</value>
        [DataMember]
        public string SkillName { get; set; }


        /// <summary>
        /// Gets or sets the display date.
        /// </summary>
        /// <value>The display date.</value>
        [DataMember]
        public DateOnlyDto DisplayDate { get; set; }

        /// <summary>
        /// Gets or sets the Estimated Service Level (ESL).
        /// </summary>
        /// <value>The esl.</value>
        [DataMember]
        public double Esl { get; set; }

        /// <summary>
        /// Gets or sets the skill data collection.
        /// </summary>
        /// <value>The skill data collection.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public ICollection<SkillDataDto> SkillDataCollection { get; set; }
    }
}
