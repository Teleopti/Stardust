using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Gets the status of a <see cref="PayrollResultDto"/> by the id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class GetPayrollResultStatusByIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the mandatory payroll result id.
        /// </summary>
        [DataMember]
        public Guid PayrollResultId
        {
            get;
            set;
        }
    }
}
