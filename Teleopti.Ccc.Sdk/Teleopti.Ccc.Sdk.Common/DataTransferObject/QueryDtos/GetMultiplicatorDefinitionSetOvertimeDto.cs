using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for <see cref="DefinitionSetDto"/> for overtime.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
    public class GetMultiplicatorDefinitionSetOvertimeDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the mandatory period to get the projected definition set for.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto Period
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the mandatory time zone id.
		/// </summary>
		[DataMember]
		public string TimeZoneId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the optional flag to determine if deleted definition sets should be included in the result.
		/// </summary>
		[DataMember(IsRequired = false, Order = 1)]
		public bool LoadDeleted { get; set; }
	}
}
