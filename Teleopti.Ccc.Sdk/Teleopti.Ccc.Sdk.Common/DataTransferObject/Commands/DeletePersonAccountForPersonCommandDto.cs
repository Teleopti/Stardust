using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command deletes an existing person account for a person of <see cref="PersonId"/>. To indicate the person account, it is compulsory to set the absence type of person account by <see cref="AbsenceId"/> and the start date <see cref="DateFrom"/> of person account.
    /// </summary>
    /// <remarks>This command requires the user to have permissions to open the People module for the given person. If no person account is found, zero items affected will be returned.</remarks>
    public class DeletePersonAccountForPersonCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person id.
        /// </summary>
        /// <value>The person id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory start date of person account.
        /// </summary>
        /// <value>The start date.</value>
        [DataMember]
        public DateOnlyDto DateFrom { get; set; }

        /// <summary>
        /// Gets or sets the mandatory absence id. Usually comes from an <see cref="AbsenceDto"/> loaded using the <see cref="ITeleoptiSchedulingService.GetAbsences"/> method.
        /// </summary>
        /// <value>The absence id.</value>
        [DataMember]
        public Guid AbsenceId { get; set; }
    }
}
