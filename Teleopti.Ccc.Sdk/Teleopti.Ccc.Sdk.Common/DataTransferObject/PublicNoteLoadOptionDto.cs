using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Load options for public note
	/// </summary>
	/// <remarks>Only one option is allowed at a time. To load one site and one team two calls must be made to the SDK.</remarks>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/01/")]
	public class PublicNoteLoadOptionDto
	{
		/// <summary>
		/// Gets or sets for what site public notes should be loaded
		/// </summary>
		[DataMember]
		public SiteDto LoadSite { get; set; }

		/// <summary>
		/// Gets or sets for what team public notes should be loaded
		/// </summary>
		[DataMember]
		public TeamDto LoadTeam { get; set; }

		/// <summary>
		/// Gets or sets for what person public notes should be loaded
		/// </summary>
		[DataMember]
		public PersonDto LoadPerson { get; set; }
	}
}
