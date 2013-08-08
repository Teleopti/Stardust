using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillKeyResource
	{
		public string SkillKey { get; set; }
		public PeriodResourceDetail Resource { get; set; }

		public IDictionary<Guid, double> Effiencies { get; set; }
	}
}