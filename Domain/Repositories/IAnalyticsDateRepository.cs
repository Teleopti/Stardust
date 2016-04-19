using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsDateRepository
	{
		IList<KeyValuePair<DateOnly, int>> Dates();
		KeyValuePair<DateOnly, int> Date(DateTime date);
	}
}