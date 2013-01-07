using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Sends a direct message to force a read model update.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class DenormalizeScheduleCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory Scenario id. Usually comes from a <see cref="ScenarioDto"/> loaded using the <see cref="ITeleoptiOrganizationService.GetScenariosByQuery"/> method.
		/// </summary>
		[DataMember]
		public Guid ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory Person id.
		/// </summary>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory start time for read model update.
		/// </summary>
		[DataMember]
		public DateTime StartDateTime { get; set; }

		/// <summary>
		/// Gets or sets the mandatory end time for read model update.
		/// </summary>
		[DataMember]
		public DateTime EndDateTime { get; set; }
	}
}