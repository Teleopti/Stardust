using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get <see cref="SkillDayDto"/> by period.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class GetSkillDaysByPeriodQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the period to get skill days for.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the timezone to display the results in.
		/// </summary>
		[DataMember]
		public string TimeZoneId { get; set; }
	}
}