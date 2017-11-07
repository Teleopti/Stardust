using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class BpoResource
	{
		public BpoResource(double resources, IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			Resources = resources;
			Skills = skills;
			Period = period;
		}
		
		public double Resources { get; }
		public IEnumerable<ISkill> Skills { get; }
		public DateTimePeriod Period { get; }
	}
}