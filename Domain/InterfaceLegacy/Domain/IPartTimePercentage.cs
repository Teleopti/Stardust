namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// PartTimePercentage
    /// </summary>
    public interface IPartTimePercentage : IAggregateRoot,
											IChangeInfo, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Description of PartTimePercentage
        /// </summary>
        Description Description { get; set; }

        /// <summary>
        /// Percentage of full time
        /// </summary>
        Percent Percentage { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        bool IsChoosable { get; }
       
    }
}
