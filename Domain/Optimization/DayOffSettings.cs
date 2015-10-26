using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffSettings : AggregateRoot
	{
		public DayOffSettings()
		{
			//TODO: put default values here until we use real db values. remove this later...
			DayOffsPerWeek = new MinMax<int>(1, 3);
			ConsecutiveWorkdays = new MinMax<int>(2, 6);
			ConsecutiveDayOffs = new MinMax<int>(1, 3);
			Default = true;
		}
		public MinMax<int> DayOffsPerWeek { get; set; }
		public MinMax<int> ConsecutiveWorkdays { get; set; }
		public MinMax<int> ConsecutiveDayOffs { get; set; }
		public bool Default { get; set; }
	}
}