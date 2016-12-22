﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class EmptyShovelResourceDataForInterval : IShovelResourceDataForInterval
	{
		public void AddResources(double resourcesToAdd)
		{
		}

		public double AbsoluteDifference{ get; }

		public double CalculatedResource { get; }

		public double RelativeDifference { get; }

		public double FStaff { get; }
	}
}