using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for Multiplicator overtime.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class GetMultiplicatorOvertimeQueryDto
	{
		/// <summary>
		/// Gets or sets include deleted.
		/// </summary>
		[DataMember]
		public bool LoadDeleted
		{
			get;
			set;
		}

	}
}
