using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class CalculatedPossibilityModel
	{
		public DateOnly Date { get; set; }

		public IDictionary<DateTime, int> IntervalPossibilies { get; set; } = new Dictionary<DateTime, int>();

		public int Resolution { get; set; } = 15;
	}

	public class CalculatedPossibilityModelResult
	{
		public IScheduleDictionary ScheduleDictionary { get; }
		public IList<CalculatedPossibilityModel> Models { get; }

		public CalculatedPossibilityModelResult(IScheduleDictionary scheduleDictionary, IList<CalculatedPossibilityModel> models)
		{
			ScheduleDictionary = scheduleDictionary;
			Models = models;
		}
	}
}