using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a SiteDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SiteDto:Dto
    {
        /// <summary>
        /// Gets or sets the name of the description.
        /// </summary>
        /// <value>The name of the description.</value>
        [DataMember]
        public string DescriptionName { get; set; }
    }
}