using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
    /// <summary>
    /// Represents an enhanced view of restrictions during an availability day.
    /// </summary>
    public sealed class AvailabilityRestrictionView : ScheduleRestrictionBaseView
    {
    	/// <summary>
    	/// Gets or sets a value indicating whether this <see cref="AvailabilityRestrictionView"/> is available.
    	/// </summary>
    	/// <value><c>True</c> if available; otherwise, <c>False</c>.</value>
    	public bool IsAvailable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityRestrictionView"/> class.
        /// </summary>
        /// <param name="target">A <see cref="IAvailabilityRestriction"/> value.</param>
        /// <param name="week">A week number.</param>
        /// <param name="day">A day number.</param>
        public AvailabilityRestrictionView(IAvailabilityRestriction target, int week, int day)
            : base(target, week, day)
        {
        	IsAvailable = !target.NotAvailable;
        }

    	public void AssignValuesToDomainObject()
    	{
    		ContainedEntity.StartTimeLimitation = StartTimeLimit();
    		ContainedEntity.EndTimeLimitation = EndTimeLimit();
    		ContainedEntity.WorkTimeLimitation = WorkTimeLimit();
    		((IAvailabilityRestriction) ContainedEntity).NotAvailable = !IsAvailable;
    	}
    }
}
