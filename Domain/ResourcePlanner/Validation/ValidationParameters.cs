using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class ValidationParameters
	{
		public DateOnlyPeriod Period { get; set; }
		public ICollection<IPerson> People { get; set; }
	}
}