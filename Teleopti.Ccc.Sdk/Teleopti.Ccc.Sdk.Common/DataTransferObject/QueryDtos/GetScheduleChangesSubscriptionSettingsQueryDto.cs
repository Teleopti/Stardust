using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get the subscription settings for schedule changes.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class GetScheduleChangesSubscriptionSettingsQueryDto : QueryDto
	{
	}
}