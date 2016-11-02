﻿using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a <see cref="PersonDto"/> by employment number.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetPersonByEmploymentNumberQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets the mandatory person employment number.
        /// </summary>
        /// <value>The person's employment number.</value>
        [DataMember]
        public string EmploymentNumber
        {
            get; set;
        }
    }
}
