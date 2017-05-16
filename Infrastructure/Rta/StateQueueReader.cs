using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class StateQueueWriter : IStateQueueWriter
	{
		public void Enqueue(DateTime time, BatchInputModel model)
		{
		}
	}

	public class StateQueueReader : IStateQueueReader
	{
		public BatchInputModel Dequeue()
		{
			return null;
		}
	}
}