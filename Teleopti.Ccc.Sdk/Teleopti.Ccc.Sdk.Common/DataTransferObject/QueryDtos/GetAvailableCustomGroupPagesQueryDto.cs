using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Query for all available custom group pages.
    /// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/09/")]
	public class GetAvailableCustomGroupPagesQueryDto : QueryDto
	{
	}
}