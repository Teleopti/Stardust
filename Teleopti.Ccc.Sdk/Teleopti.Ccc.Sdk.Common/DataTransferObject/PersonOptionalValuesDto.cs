using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Contains the optional column values for one person.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
	public class PersonOptionalValuesDto : Dto
	{
		public PersonOptionalValuesDto()
		{
			OptionalValueCollection = new Collection<OptionalValueDto>();
		}

		/// <summary>
		/// Gets the list of optional values.
		/// </summary>
		[DataMember]
		public ICollection<OptionalValueDto> OptionalValueCollection { get; private set; }

		/// <summary>
		/// Gets and sets the person Id.
		/// </summary>
		/// <value>The person's Id.</value>
		[DataMember]
		public Guid PersonId { get; set; }
	}
}