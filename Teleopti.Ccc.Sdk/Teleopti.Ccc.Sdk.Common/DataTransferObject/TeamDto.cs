using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a TeamDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class TeamDto : Dto
    {
        private string _siteAndTeam;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the site and team.
        /// </summary>
        /// <value>The site and team.</value>
        [DataMember]
        public string SiteAndTeam
        {
            get { return _siteAndTeam; }
            set  { _siteAndTeam = value;}
        }
    }
}