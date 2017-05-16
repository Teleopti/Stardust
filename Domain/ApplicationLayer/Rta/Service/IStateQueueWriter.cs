using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateQueueWriter
	{
		void Enqueue(DateTime time, BatchInputModel model);
	}
}