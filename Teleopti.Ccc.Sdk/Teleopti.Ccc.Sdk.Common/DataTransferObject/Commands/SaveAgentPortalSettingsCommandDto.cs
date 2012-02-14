using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class SaveAgentPortalSettingsCommandDto : CommandDto
    {
        /// <summary>
        /// Saves the Resolution in Agent Portal.
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public int Resolution { get; set; }
    }
}
