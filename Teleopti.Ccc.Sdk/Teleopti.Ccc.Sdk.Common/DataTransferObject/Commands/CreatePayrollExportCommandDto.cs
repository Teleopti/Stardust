using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command runs a payroll export according to the parameters specified in <see cref="PayrollExportDto"/>.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class CreatePayrollExportCommandDto : CommandDto
    {
        /// <summary>
        /// PayrollExportDto
        /// </summary>
        [DataMember]
        public PayrollExportDto PayrollExportDto { get; set; }
    }
}
