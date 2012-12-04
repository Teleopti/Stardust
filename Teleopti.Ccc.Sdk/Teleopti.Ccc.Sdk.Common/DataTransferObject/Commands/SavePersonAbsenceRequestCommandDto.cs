using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command saves a person absence request <see cref="PersonRequestDto"/>, the AffectedId of command result is the saved person absence request Id. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class SavePersonAbsenceRequestCommandDto : CommandDto
    {
        /// <summary>
        /// Gets and sets the mandatory person request to save.
        /// </summary>
        /// <value>The person request dto.</value>
        /// <remarks>The person request must be of type absence request to perform this command.</remarks>
        [DataMember]
        public PersonRequestDto PersonRequestDto
        {
            get;
            set;
        }
    }
}
