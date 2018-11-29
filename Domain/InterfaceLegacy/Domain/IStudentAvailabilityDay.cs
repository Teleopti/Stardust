using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary> 
    public interface IStudentAvailabilityRestriction : IRestrictionBase, ICloneableEntity<IStudentAvailabilityRestriction>
    {// Because this is empty I place it in the same file
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IStudentAvailabilityDay : IPersistableScheduleData, IVersioned
    {
        /// <summary>
        /// Gets or sets a value indicating whether [not available].
        /// </summary>
        /// <value><c>true</c> if [not available]; otherwise, <c>false</c>.</value>
        bool NotAvailable { get; set; }
        /// <summary>
        /// Gets the restriction collection.
        /// </summary>
        /// <value>The restriction collection.</value>
        ReadOnlyCollection<IStudentAvailabilityRestriction> RestrictionCollection { get; }
        /// <summary>
        /// Gets the restriction date.
        /// </summary>
        /// <value>The restriction date.</value>
        DateOnly RestrictionDate { get; }

		/// <summary>
		/// Change the availability to this new time range
		/// </summary>
	    void Change(TimePeriod range);
    }
    
}
