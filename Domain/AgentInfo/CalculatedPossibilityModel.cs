using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class CalculatedPossibilityModel
	{
		public DateOnly Date { get; set; }

		public IDictionary<DateTime, int> IntervalPossibilies {get;set;}

		public int Resolution { get; set; } = 15;
	}
}