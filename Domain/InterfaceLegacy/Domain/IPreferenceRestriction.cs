using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPreferenceRestriction : IRestrictionBase, ICloneableEntity<IPreferenceRestriction>
    {
        /// <summary>
        /// Gets or sets the absence
        /// </summary>
        IAbsence Absence { get; set; }
        /// <summary>
        /// Gets or sets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        IShiftCategory ShiftCategory { get; set; }
        /// <summary>
        /// Gets or sets the day off template.
        /// </summary>
        /// <value>The day off template.</value>
        IDayOffTemplate DayOffTemplate { get; set; }
        /// <summary>
        /// Gets the activity restriction collection.
        /// </summary>
        /// <value>The activity restriction collection.</value>
        ReadOnlyCollection<IActivityRestriction> ActivityRestrictionCollection { get; }
        /// <summary>
        /// Adds the activity restriction.
        /// </summary>
        /// <param name="activityRestriction">The activity restriction.</param>
        void AddActivityRestriction(IActivityRestriction activityRestriction);
        /// <summary>
        /// Removes the activity restriction.
        /// </summary>
        /// <param name="activityRestriction">The activity restriction.</param>
        void RemoveActivityRestriction(IActivityRestriction activityRestriction);
        /// <summary>
        /// Gets or sets a value indicating whether [must have].
        /// </summary>
        /// <value><c>true</c> if [must have]; otherwise, <c>false</c>.</value>
        bool MustHave { get; set; }
    }
}
