using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IContextLoader
	{
		void ForBatch(BatchInputModel batch);
		void ForActivityChanges();
		void ForClosingSnapshot(DateTime snapshotId, string sourceId);
	}
}