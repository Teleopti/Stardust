using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get a collection of <see cref="PersonDto"/> available to trade with.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetPeopleForShiftTradeByGroupPageGroupQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the mandatory id of the group inside the group page.
        /// </summary>
        /// <remarks>This can be set to an Id of <see cref="TeamDto"/> as well.</remarks>
        [DataMember]
        public Guid GroupPageGroupId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory date to query for.
        /// </summary>
        [DataMember]
        public DateOnlyDto QueryDate { get; set; }

        /// <summary>
        /// Gets or sets the mandatory Id of the <see cref="PersonDto"/> that wants to perform the trade.
        /// </summary>
        [DataMember]
        public Guid PersonId { get; set; }
    }
}
