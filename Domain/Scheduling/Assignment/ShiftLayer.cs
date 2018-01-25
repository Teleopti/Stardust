using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ShiftLayer : AggregateEntity, ILayer<IActivity>
	{
		private const int notAllowedDbValue = -1;
		
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

		//NOT TO BE USED FOR BUSINESS LOGIC
		protected internal virtual int OrderIndex { get; set; } = notAllowedDbValue;

		public virtual ShiftLayer EntityClone()
		{
			return (ShiftLayer)MemberwiseClone();
		}
	}
}