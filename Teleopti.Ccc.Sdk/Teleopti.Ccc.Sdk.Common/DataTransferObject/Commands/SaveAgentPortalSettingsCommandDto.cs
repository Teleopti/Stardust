using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Saves the settings used in the MyTime client (Agent Portal).
	/// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class SaveAgentPortalSettingsCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory resolution in minutes to use in calendar views.
        /// </summary>
        /// <value>The date of schedule.</value>
        /// <remarks>Default is 15 minutes.</remarks>
        [DataMember]
        public int Resolution { get; set; }
    }
}
