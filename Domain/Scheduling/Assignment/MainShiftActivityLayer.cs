using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// MainshiftActivitylayer class
    /// </summary>
    public class MainShiftActivityLayer : PersistedActivityLayer, IMainShiftActivityLayer
    {
	    private readonly IMainShiftActivityLayerNew _thisShouldGoAway;


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


			//hackeri hackera
			public MainShiftActivityLayer(IMainShiftActivityLayerNew thisShouldGoAway)
				:base(thisShouldGoAway.Payload, thisShouldGoAway.Period)
			{
				_thisShouldGoAway = thisShouldGoAway;
				SetId(thisShouldGoAway.Id);
			}
			public override bool Equals(object other)
			{
				if (_thisShouldGoAway != null)
				{
					var that = other as MainShiftActivityLayer;
					if (that == null)
						return false;
					return _thisShouldGoAway.Equals(that._thisShouldGoAway);					
				}
				return Equals((ILayer)other);
			}
			/// 


				protected override int findOrderIndex()
				{
					//fix later
					return (((IMainShift)Parent).LayerCollection).IndexOf(this);
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

		public override int GetHashCode()
		{
			throw new NotImplementedException("this class should disappear anyhow");
		}
    }
}