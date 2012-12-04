﻿using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Query for the <see cref="GroupPageGroupDto"/> inside a group page at a specific date.
    /// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/09/")]
	public class GetGroupsForGroupPageAtDateQueryDto : QueryDto
	{
        /// <summary>
        /// Gets or sets the mandatory date to query for.
        /// </summary>
		[DataMember]
		public DateOnlyDto QueryDate { get; set; }

        /// <summary>
        /// Gets or sets the mandatory group page id to list the groups for.
        /// </summary>
		[DataMember]
		public Guid PageId { get; set; }
	}
}