
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAvailabilityRestriction : IRestrictionBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether [not available].
        /// </summary>
        /// <value><c>true</c> if [not available]; otherwise, <c>false</c>.</value>
        bool NotAvailable { get; set; }
    }
}
