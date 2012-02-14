
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRotationRestriction : IRestrictionBase
    {
        /// <summary>
        /// Gets or sets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        IShiftCategory ShiftCategory { get; set; }
        /// <summary>
        /// Gets or sets the day off.
        /// </summary>
        /// <value>The day off.</value>
        IDayOffTemplate DayOffTemplate { get; set; }
    }
}
