using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class StateInfo
	{
		public IList<ScheduleLayer> ScheduleLayers { get; set; }
		public IActualAgentState PreviousState { get; set; }
		public IActualAgentState NewState { get; set; }
		public ScheduleLayer CurrentActivity { get; set; }
		public ScheduleLayer NextActivityInShift { get; set; }
		
		public bool SendOverMessageBroker
		{
			get { return PreviousState == null || !NewState.Equals(PreviousState); }
		}
	}
}