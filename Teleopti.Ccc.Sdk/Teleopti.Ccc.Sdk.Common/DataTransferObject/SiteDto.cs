using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a SiteDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SiteDto:Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDto"/> class.
        /// </summary>
        /// <param name="site">The site.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public SiteDto(ISite site)
        {
            Id = site.Id;
            DescriptionName = site.Description.Name;
        }

        /// <summary>
        /// Gets or sets the name of the description.
        /// </summary>
        /// <value>The name of the description.</value>
        [DataMember]
        public string DescriptionName { get; set; }
    }
}