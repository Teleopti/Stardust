using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Gets the status of a payroll result by the id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class GetPayrollResultStatusByIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the payroll result id.
        /// </summary>
        [DataMember]
        public Guid PayrollResultId
        {
            get;
            set;
        }
    }
}
