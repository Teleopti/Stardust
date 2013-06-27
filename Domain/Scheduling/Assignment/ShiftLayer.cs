using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract  class ShiftLayer : AggregateEntity
	{
		protected ShiftLayer(IActivity activity, DateTimePeriod period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
			Payload = activity;
			Period = period;
		}

		protected ShiftLayer()
		{
		}

		public virtual IActivity Payload { get; protected set; }
		public virtual DateTimePeriod Period { get; protected set; }


		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual ILayer<IActivity> NoneEntityClone()
		{
			var retObj = (IAggregateEntity)MemberwiseClone();
			retObj.SetId(null);
			retObj.SetParent(null);
			return (ILayer<IActivity>) retObj;
		}

		public virtual ILayer<IActivity> EntityClone()
		{
			return (ILayer<IActivity>)MemberwiseClone();
		}
	}
}