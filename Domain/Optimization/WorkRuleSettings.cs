using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class WorkRuleSettings : AggregateRoot
	{
		public WorkRuleSettings()
		{
			//put default values here until we use real db values. remove this later...
			DayOffsPerWeek = new MinMax<int>(1, 3);
		}
		public MinMax<int> DayOffsPerWeek { get; set; }
	}
}