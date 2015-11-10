﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public interface IFilter
	{
		bool IsValidFor(IPerson person, DateOnly dateOnly);
	}
}