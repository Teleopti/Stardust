using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get the current <see cref="AgentPortalSettingsDto"/> for the MyTime client (Agent Portal).
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class GetAgentPortalSettingsQueryDto : QueryDto
    {
    }
}