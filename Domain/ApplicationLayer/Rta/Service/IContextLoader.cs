using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IContextLoader
	{
		void For(StateInputModel input, Action<Context> action);
		void ForBatch(BatchInputModel batch, Action<Context> action);
		void ForActivityChanges(Action<Context> action);
		void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action);
		void ForSynchronize(Action<Context> action);
	}
}