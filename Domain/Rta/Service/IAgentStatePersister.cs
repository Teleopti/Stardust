using System;
using System.Collections.Generic;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DeadLockRetrier
	{
		private readonly int _totalRetries = 3;

		public void RetryOnDeadlock(Action action)
		{
			// make retry and warn logging an aspect?
			var attempt = 1;
			while (true)
			{
				try
				{
					action.Invoke();
					break;
				}
				catch (DeadLockVictimException e)
				{
					attempt++;
					if (attempt > _totalRetries)
						throw;
					LogManager.GetLogger(typeof(DeadLockRetrier))
						.Warn($"Transaction deadlocked, running attempt {attempt} of {_totalRetries}.", e);
				}
			}
		}
	}

	public class DeadLockVictimException : Exception
	{
		public DeadLockVictimException(string message, Exception innerException):base(message, innerException)
		{
		}
	}

	public enum DeadLockVictim
	{
		Yes,
		No
	}

	public interface IAgentStatePersister
	{
		// maintain
		void Delete(Guid personId, DeadLockVictim deadLockVictim);
		void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim);

		// find things to work with
		IEnumerable<PersonForCheck> FindForCheck();
		IEnumerable<Guid> FindForClosingSnapshot(DateTime snapshotId, int snapshotDataSourceId, IEnumerable<Guid> loggedOutStateGroupIds);

		// lock and update
		LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim);
		void Update(AgentState model);
	}

	public class LockedData
	{
		public IEnumerable<AgentState> AgentStates;
		public string MappingVersion;
		public CurrentScheduleReadModelVersion ScheduleVersion;
	}

}