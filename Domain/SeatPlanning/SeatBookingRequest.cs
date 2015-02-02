using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequest
	{
		private readonly AgentShift[] _agentShifts;

		public SeatBookingRequest(params AgentShift[] agentShifts)
		{
			_agentShifts = agentShifts;
		}

		public IEnumerable<AgentShift> AgentShifts { get { return _agentShifts; } }

		public int MemberCount { get { return _agentShifts.Length; } }
	}
}
