using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat.TestData
{
	public class MaxSeatData
	{
		public MaxSeatData(Person agent, IPersonAssignment assignment)
		{
			Agent = agent;
			Assignment = assignment;
		}

		public Person Agent { get; }
		public IPersonAssignment Assignment { get; }
	}
}