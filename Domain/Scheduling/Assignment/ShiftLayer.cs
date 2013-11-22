using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public abstract class ShiftLayer : AggregateEntity, IShiftLayer
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
		public virtual int OrderIndex
		{
			get
			{
				var ass = Parent as IPersonAssignment;
				if (ass == null)
					return -1;
				return ass.ShiftLayers.ToList().IndexOf(this);
			}
		}


		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual IShiftLayer NoneEntityClone()
		{
			var retObj = (IAggregateEntity)MemberwiseClone();
			retObj.SetId(null);
			retObj.SetParent(null);
			return (IShiftLayer)retObj;
		}

		public virtual IShiftLayer EntityClone()
		{
			return (IShiftLayer)MemberwiseClone();
		}
	}
}