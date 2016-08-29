using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStatePrepare
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
	}

	public class ExternalLogon
	{
		public int DataSourceId { get; set; }
		public string UserCode { get; set; }
	}

	public interface IAgentStatePersister
	{
		// cleaner/updater/preparer stuff
		void Delete(Guid personId);
		void Prepare(AgentStatePrepare model);

		// rta service stuff
		IEnumerable<AgentState> Get(int dataSourceId, string userCode);
		AgentState Get(Guid personId);
		IEnumerable<AgentState> Get(IEnumerable<Guid> personIds);
		IEnumerable<AgentState> GetAll();
		IEnumerable<AgentState> GetNotInSnapshot(DateTime snapshotId, string sourceId);
		void Update(AgentState model);
	}
}