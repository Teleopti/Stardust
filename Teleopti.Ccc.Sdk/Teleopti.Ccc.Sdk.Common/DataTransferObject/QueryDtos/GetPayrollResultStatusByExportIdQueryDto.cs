﻿using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Get the status of <see cref="PayrollResultDto"/> based on the payroll export id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class GetPayrollResultStatusByExportIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the mandatory payroll export id.
        /// </summary>
        [DataMember]
        public Guid PayrollExportId
        {
            get;
            set;
        }
    }
}
