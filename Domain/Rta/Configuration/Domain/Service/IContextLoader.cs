using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IContextLoader
	{
		void ForBatch(BatchInputModel batch);
		void ForActivityChanges();
		void ForClosingSnapshot(DateTime snapshotId, string sourceId);
	}
}