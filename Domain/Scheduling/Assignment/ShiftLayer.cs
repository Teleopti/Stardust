﻿using System.Linq;
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
				/*
				 * Returns 
				 * >=0: The position in the layer list
				 * -1 : This layer's assignment doesn't have this layer in its layer list
				 * -2 : This layer doesn't have a parent/assignment 
				*/
				var ass = Parent as IPersonAssignment;
				if (ass == null)
					return -2;
				return ass.ShiftLayers.ToList().IndexOf(this);
			}
		}
		

		public virtual IShiftLayer EntityClone()
		{
			return (IShiftLayer)MemberwiseClone();
		}
	}
}