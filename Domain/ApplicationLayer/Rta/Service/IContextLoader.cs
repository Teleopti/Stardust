using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IContextLoader
	{
		void For(StateInputModel input);
		void ForBatch(BatchInputModel batch);
		void ForActivityChanges();
		void ForClosingSnapshot(DateTime snapshotId, string sourceId);
	}
}