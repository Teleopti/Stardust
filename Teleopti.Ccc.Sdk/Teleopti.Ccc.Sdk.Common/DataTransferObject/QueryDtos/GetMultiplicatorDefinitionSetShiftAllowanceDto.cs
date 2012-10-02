using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for Multiplicator definition set for shift allowance.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class GetMultiplicatorDefinitionSetShiftAllowanceDto
	{
		/// <summary>
		/// Gets or sets date time period.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto Period
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets time zone.
		/// </summary>
		[DataMember]
		public string TimeZoneId
		{
			get;
			set;
		}
	}
}
