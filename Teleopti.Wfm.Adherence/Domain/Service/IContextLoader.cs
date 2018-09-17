using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IContextLoader
	{
		void ForBatch(BatchInputModel batch);
		void ForActivityChanges();
		void ForClosingSnapshot(DateTime snapshotId, string sourceId);
	}
}