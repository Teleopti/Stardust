using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
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
		IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim);
		IEnumerable<AgentState> GetStates();
		void Update(AgentState model, bool updateSchedule);

		// if sync used FindAll this can be removed
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds);

	}

	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<AgentStateFound> Find(this IAgentStatePersister instance, ExternalLogon externalLogon, DeadLockVictim deadLockVictim)
		{
			return instance.Find(new[] {externalLogon}, deadLockVictim);
		}
	}
}