using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DeadLockRetrier
	{
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
					if (attempt > 3)
						throw;
					LogManager.GetLogger(typeof(DeadLockRetrier))
						.Warn($"Transaction deadlocked, running attempt {attempt}.", e);
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
		void InvalidateSchedules(Guid personId, DeadLockVictim deadLockVictim);

		// rta service stuff
		[RemoveMeWithToggle(Toggles.RTA_FasterActivityCheck_41380)]
		IEnumerable<ExternalLogon> FindAll();
		IEnumerable<ExternalLogonForCheck> FindForCheck();
		IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState);
		IEnumerable<AgentStateFound> FindForSynchronize();
		IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim);
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim);
		void Update(AgentState model, bool updateSchedule);
	}

	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<AgentStateFound> Find(this IAgentStatePersister instance, ExternalLogon externalLogon, DeadLockVictim deadLockVictim)
		{
			return instance.Find(new[] {externalLogon}, deadLockVictim);
		}
	}
}