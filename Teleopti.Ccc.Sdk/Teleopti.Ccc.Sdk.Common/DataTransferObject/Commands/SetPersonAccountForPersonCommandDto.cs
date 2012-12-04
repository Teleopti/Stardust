using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command will create and add a person account for a person or change an existing person account. A person account is distinguished by <see cref="PersonId"/>, <see cref="AbsenceId"/> and the start date <see cref="DateFrom"/>. If no such person account exists, the command creates and add it for the person; otherwise, the command change values according to the optional values <see cref="Extra"/>,<see cref="Accrued"/> and <see cref="BalanceIn"/>.  
    /// </summary>
	/// <remarks>This command requires the user to have permissions to open the People module for the given person.</remarks>
    public class SetPersonAccountForPersonCommandDto : CommandDto
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
		/// Gets or sets the mandatory absence id. The id usually comes from <see cref="AbsenceDto"/> loaded using the method <see cref="ITeleoptiSchedulingService.GetAbsences"/>.
        /// </summary>
        /// <value>The absence id.</value>
        [DataMember]
        public Guid AbsenceId { get; set; }

        /// <summary>
        /// Gets or sets the optional balance in.
        /// </summary>
        /// <value>The balance in.</value>
        [DataMember]
        public long? BalanceIn { get; set; }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        [DataMember]
        public long? Extra { get; set; }

        /// <summary>
        /// Gets or sets the accrued.
        /// </summary>
        /// <value>The accrued.</value>
        [DataMember]
        public long? Accrued { get; set; }
    }
}
