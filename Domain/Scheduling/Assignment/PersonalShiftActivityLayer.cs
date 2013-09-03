using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// PersonalShiftActivityLayer class
    /// </summary>
    public class PersonalShiftActivityLayer : PersistedActivityLayer, IPersonalShiftActivityLayer
    {
		//private IEntity _parent;
		//private Guid? _id;

        public PersonalShiftActivityLayer(IActivity activity, DateTimePeriod period) : base(activity, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalShiftActivityLayer"/> class.
        /// </summary>
        protected PersonalShiftActivityLayer()
        {
        }
    }
}