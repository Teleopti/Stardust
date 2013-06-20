﻿using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MainShiftLayer : AggregateEntity, IMainShiftLayer
	{
		public MainShiftLayer(IActivity activity, DateTimePeriod period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
			Payload = activity;
			Period = period;
		}
		protected MainShiftLayer()
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
				return ass.MainLayers.ToList().IndexOf(this);
			}
		}

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual ILayer<IActivity> NoneEntityClone()
		{
			var retObj = (MainShiftLayer)MemberwiseClone();
			retObj.SetId(null);
			return retObj;
		}

		public virtual ILayer<IActivity> EntityClone()
		{
			return (MainShiftLayer)MemberwiseClone();
		}
	}
}