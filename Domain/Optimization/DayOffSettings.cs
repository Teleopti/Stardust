using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffSettings : AggregateRoot
	{
		public MinMax<int> DayOffsPerWeek { get; set; }
		public MinMax<int> ConsecutiveWorkdays { get; set; }
		public MinMax<int> ConsecutiveDayOffs { get; set; }
		public bool Default { get; set; }
	}
}