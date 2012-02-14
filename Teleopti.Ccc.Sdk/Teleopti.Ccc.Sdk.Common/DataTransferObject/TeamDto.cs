using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

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
        /// Initializes a new instance of the <see cref="TeamDto"/> class.
        /// </summary>
        /// <param name="team">The team.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public TeamDto(ITeam team)
        {
            Id = team.Id;
            Description = team.Description.Name;
            _siteAndTeam = team.SiteAndTeam;
        }

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