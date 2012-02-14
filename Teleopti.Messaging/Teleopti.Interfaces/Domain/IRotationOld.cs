using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class for rotations
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-25
    /// </remarks>
    public interface IRotation : IRotationBase<IRotationDayRestriction>,
                                    IChangeInfo
    {
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