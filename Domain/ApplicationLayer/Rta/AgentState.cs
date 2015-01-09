using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AgentState
	{
		public DateTime? BatchId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string SourceId { get; set; }

		public Guid PersonId { get; set; }
		public DateTime ReceivedTime { get; set; }
		public string StateCode { get; set; }
		public Guid StateGroupId { get; set; }
		public Guid ActivityId { get; set; }

		public Guid NextActivityId { get; set; }
		public DateTime NextActivityStartTime { get; set; }

		public Guid AlarmTypeId { get; set; }
		public DateTime AlarmTypeStartTime { get; set; }

		public double StaffingEffect { get; set; }


		
		private Func<IActualAgentState> _assemble;

		public void UseAssembleMethod(Func<IActualAgentState> helpMeFunction)
		{
			_assemble = helpMeFunction;
		}

		public IActualAgentState MakeActualAgentState()
		{
			return _assemble.Invoke();
		}

	}
}