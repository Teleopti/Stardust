using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Query to get the current <see cref="BusinessUnitDto"/>.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class GetCurrentBusinessUnitQueryDto : QueryDto
    {
    }
}