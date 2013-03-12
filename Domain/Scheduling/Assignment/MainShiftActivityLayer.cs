using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// MainshiftActivitylayer class
    /// </summary>
    public class MainShiftActivityLayer : PersistedActivityLayer, IMainShiftActivityLayer
    {
		//private IEntity _parent;
		//private Guid? _id;

	    public MainShiftActivityLayer(IActivity activity, DateTimePeriod period)
            : base(activity, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainShiftActivityLayer"/> class.
        /// </summary>
        protected MainShiftActivityLayer()
        {
        }

		public override bool Equals(IEntity other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;
			if (!other.Id.HasValue || !Id.HasValue)
				return false;

			return (Id.Value == other.Id.Value);
		}
    }
}