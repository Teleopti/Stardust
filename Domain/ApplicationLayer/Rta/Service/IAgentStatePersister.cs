using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.FeatureFlags;

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
		// maintainer stuff
		void Delete(Guid personId, DeadLockVictim deadLockVictim);
		void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim);

		// rta service stuff
		IEnumerable<ExternalLogonForCheck> FindForCheck();
		IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState);
		IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim);
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim);
		void Update(AgentState model);
	}

	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<AgentStateFound> Find(this IAgentStatePersister instance, ExternalLogon externalLogon, DeadLockVictim deadLockVictim)
		{
			return instance.Find(new[] {externalLogon}, deadLockVictim);
		}
	}
}