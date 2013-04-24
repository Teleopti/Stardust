using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class AgentDayDateSetter : IAgentDayDateSetter
	{
		public static readonly DateOnly RestoreDate = new DateOnly(1800, 1, 1);
		private readonly IEnumerable<IPersonAssignmentConverter> _agentDayDatePartSetters;
		private readonly SqlConnectionStringBuilder _connectionStringBuilder;

		public AgentDayDateSetter(IEnumerable<IPersonAssignmentConverter> agentDayDatePartSetters, SqlConnectionStringBuilder connectionStringBuilder)
		{
			_agentDayDatePartSetters = agentDayDatePartSetters;
			_connectionStringBuilder = connectionStringBuilder;
		}

		public void Execute()
		{
			foreach (var converter in _agentDayDatePartSetters)
			{
				converter.Execute(_connectionStringBuilder);
			}
		}
	}
}