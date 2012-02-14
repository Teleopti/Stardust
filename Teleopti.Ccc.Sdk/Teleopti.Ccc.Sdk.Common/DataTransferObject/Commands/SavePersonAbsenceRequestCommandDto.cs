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
        /// Gets and sets the person request
        /// </summary>
        /// <value>PersonRequestDto</value>
        [DataMember]
        public PersonRequestDto PersonRequestDto
        {
            get;
            set;
        }
    }
}
