using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command deletes an existing person account for a person of <see cref="PersonId"/>. To indicate the person account, it is compulsory to set the absence type of person account by <see cref="AbsenceId"/> and the start date <see cref="DateFrom"/> of person account.
    /// </summary>
    public class DeletePersonAccountForPersonCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>The person id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the start date of person account.
        /// </summary>
        /// <value>The start date.</value>
        [DataMember]
        public DateOnlyDto DateFrom { get; set; }

        /// <summary>
        /// Gets or sets the absence id.
        /// </summary>
        /// <value>The absence id.</value>
        [DataMember]
        public Guid AbsenceId { get; set; }
    }
}
