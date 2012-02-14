
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// This object belongs to a specific business unit
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-08-24
    /// </remarks>
    public interface IBelongsToBusinessUnit
    {

        /// <summary>
        /// Gets the business unit this root belongs to.
        /// </summary>
        /// <value>The business unit.</value>
        IBusinessUnit BusinessUnit { get; }
    }
}
