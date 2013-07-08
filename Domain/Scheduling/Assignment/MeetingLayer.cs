using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MeetingLayer : ILayer<IActivity>
	{
		public MeetingLayer(IActivity activity, DateTimePeriod period)
		{
			Payload = activity;
			Period = period;
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}

		public ILayer<IActivity> NoneEntityClone()
		{
			throw new NotImplementedException();
		}

		public ILayer<IActivity> EntityClone()
		{
			throw new NotImplementedException();
		}

		public DateTimePeriod Period { get; private set; }
		public IActivity Payload { get; private set; }
		public int OrderIndex { get; private set; }
	}
}