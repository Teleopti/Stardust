using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Represents an WorkloadOnSkillSelectionDto object.
	/// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
	public class WorkloadOnSkillSelectionDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the skill id.
		/// </summary>
		/// <value>The skill id.</value>
		[DataMember]
		public Guid SkillId { get; set; }

		/// <summary>
		/// Gets or sets the list containing the workload ids.
		/// </summary>
		/// <value>The list with workload ids.</value>
        [DataMember]
        public List<Guid> WorkloadId { get; set; }
	}

	/// <summary>
	/// Represents an RecalculateForecastOnSkillCollectionCommandDto object.
	/// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class RecalculateForecastOnSkillCollectionCommandDto : CommandDto
    {
		/// <summary>
		/// Gets or sets the list containing the WorkloadOnSkillSelectionDto objects.
		/// </summary>
		/// <value>The list with WorkloadOnSKillSelectionDto objects.</value>
        [DataMember]
		public List<WorkloadOnSkillSelectionDto> WorkloadOnSkillSelectionDtos { get; set; }

		/// <summary>
		/// Gets or sets the owner person id.
		/// </summary>
		/// <value>The owner person id.</value>
		[DataMember]
		public Guid OwnerPersonId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id.
		/// </summary>
		/// <value>The scenario id.</value>
		[DataMember]
		public Guid ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the business unit id.
		/// </summary>
		/// <value>The business unit id.</value>
		[DataMember]
		public Guid BusinessUnitId { get; set; }

		/// <summary>
		/// Gets or sets the data source.
		/// </summary>
		/// <value>The data source.</value>
		[DataMember]
		public string DataSource { get; set; }
    }
}