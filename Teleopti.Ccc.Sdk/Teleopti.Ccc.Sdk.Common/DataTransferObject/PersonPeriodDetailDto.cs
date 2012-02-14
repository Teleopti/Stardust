using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details of the person period.
    /// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/10/")]
	public class PersonPeriodDetailDto : Dto
	{
		public PersonPeriodDetailDto()
		{
			ExternalLogOn = new List<ExternalLogOnDto>();
		}

		/// <summary>
		/// Gets or sets person id
		/// </summary>
		/// <remarks>Can be used to match with the correct person.</remarks>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the start date
		/// </summary>
		[DataMember]
		public DateOnlyDto StartDate { get; set; }

		/// <summary>
		/// Gets or sets team
		/// </summary>
		[DataMember]
		public TeamDto Team { get; set; }

		/// <summary>
		/// Gets or sets the note
		/// </summary>
		[DataMember]
		public string Note { get; set; }

		/// <summary>
		/// Gets or sets the contract
		/// </summary>
		[DataMember]
		public Guid ContractId { get; set; }

		/// <summary>
		/// Gets or sets the part time percentage
		/// </summary>
		[DataMember]
		public Guid PartTimePercentageId { get; set; }

		/// <summary>
		/// Gets or sets the contract schedule
		/// </summary>
		[DataMember]
		public Guid ContractScheduleId { get; set; }

		/// <summary>
		/// Gets or sets external logon list
		/// </summary>
		[DataMember]
		public IList<ExternalLogOnDto> ExternalLogOn { get; private set; }
	}
}
