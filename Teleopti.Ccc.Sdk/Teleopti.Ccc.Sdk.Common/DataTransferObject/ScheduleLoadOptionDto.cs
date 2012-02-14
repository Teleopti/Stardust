using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Load options for schedules
	/// </summary>
	/// <remarks>Only one of the options <see cref="LoadAll"/>, <see cref="LoadSite"/>, <see cref="LoadTeam"/> or <see cref="LoadPerson"/> can be set for each call to the SDK.</remarks>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/10/")]
	public class ScheduleLoadOptionDto
	{
		/// <summary>
		/// Gets or sets if schedules for all people should be loaded
		/// </summary>
		[DataMember]
		public bool LoadAll { get; set; }

		/// <summary>
		/// Gets or sets for what site schedules should be loaded
		/// </summary>
		[DataMember]
		public SiteDto LoadSite { get; set; }

		/// <summary>
		/// Gets or sets for what team schedules should be loaded
		/// </summary>
		[DataMember]
		public TeamDto LoadTeam { get; set; }

		/// <summary>
		/// Gets or sets for what person schedules should be loaded
		/// </summary>
		[DataMember]
		public PersonDto LoadPerson { get; set; }

		/// <summary>
		/// Used if normal projection is not wanted.
		/// <see cref="IProjectionMerger" />
        /// If set to "midnightSplit" or "excludeAbsencesMidnightSplit" it will be used as <see cref="IProjectionMerger"/>.
		/// 
		/// All other values use Teleopti.Ccc.Domain.Scheduling.Assignment.ProjectionPayloadMerger
		/// to build projection.
        /// If set to "excludeAbsencesMidnightSplit" or "excludeAbsences" it will return projections with absences excluded.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string SpecialProjection { get; set; }
	}
}
