using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for Multiplicator definition set for shift allowance.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class GetMultiplicatorDefinitionSetOvertimeDto
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
