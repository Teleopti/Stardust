using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	public class MaxSeatData
	{
		public MaxSeatData(IPerson agent, IPersonAssignment assignment)
		{
			Agent = agent;
			Assignment = assignment;
		}

		public IPerson Agent { get; }
		public IPersonAssignment Assignment { get; }
	}
}