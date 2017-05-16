using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class StateQueue : IStateQueueWriter, IStateQueueReader
	{
		public void Enqueue(DateTime time, BatchInputModel model)
		{
		}

		public BatchInputModel Dequeue()
		{
			return null;
		}
	}
}