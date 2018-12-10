using System;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	public static class RtaExtensions
	{
		public static void Enqueue(this Rta rta, BatchInputModel batch)
		{
			rta.Enqueue(batch, null);
		}

		public static bool QueueIteration(this Rta rta, string tenant)
		{
			return rta.QueueIteration(tenant, null);
		}

		public static void Process(this Rta rta, BatchInputModel batch)
		{
			rta.Process(batch, null);
		}
		
		public static void CheckForActivityChanges(this Rta rta, string tenant, Guid personId)
		{
			rta.CheckForActivityChanges(tenant);
		}

		public static void ProcessState(this Rta rta, StateForTest input)
		{
			rta.Process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.SnapshotId,
				States = new[]
				{
					new BatchStateInputModel
					{
						StateCode = input.StateCode,
						StateDescription = input.StateDescription,
						UserCode = input.UserCode
					}
				}
			}, null);
		}

		public static void CloseSnapshot(this Rta rta, CloseSnapshotForTest input)
		{
			rta.Process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.SnapshotId,
				CloseSnapshot = true
			}, null);
		}
	}
}