using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get a collection of <see cref="PersonDto"/> based on a <see cref="GroupPageGroupDto"/> inside a <see cref="GroupPageDto"/> on a specified date.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetPeopleByGroupPageGroupQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the mandatory id of the <see cref="GroupPageGroupDto"/> inside the <see cref="GroupPageDto"/>.
        /// </summary>
        [DataMember]
        public Guid GroupPageGroupId{get; set;}

        /// <summary>
        /// Gets or sets the mandatory date to query for.
        /// </summary>
		[DataMember]
		public DateOnlyDto QueryDate { get; set; }
    }
}
