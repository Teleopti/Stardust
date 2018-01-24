using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ShiftLayer : AggregateEntity, ILayer<IActivity>
	{
		protected ShiftLayer(IActivity activity, DateTimePeriod period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
			InParameter.EnsurePeriodHasLength(period);
			Payload = activity;
			Period = period;
		}

		protected ShiftLayer()
		{
		}

		public virtual IActivity Payload { get; protected set; }
		public virtual DateTimePeriod Period { get; protected set; }
		
		//THIS IS NOT TO BE USED IN BUSINESS LOGIC!
		protected internal virtual int OrderIndex { get; set; }

		public virtual ShiftLayer EntityClone()
		{
			return (ShiftLayer)MemberwiseClone();
		}
	}
}