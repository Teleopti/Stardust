using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Query for all the saved <see cref="PayrollExportDto"/> definitions.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetAllPayrollExportsQueryDto : QueryDto
    {
    }
}
