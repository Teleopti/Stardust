using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroup : NonversionedAggregateRootWithBusinessUnit
	{
		public AgentGroup()
		{
			Name = string.Empty;
		}

		public virtual string Name { get; set; }
	}
}