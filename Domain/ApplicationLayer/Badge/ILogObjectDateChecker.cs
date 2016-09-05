using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface ILogObjectDateChecker
	{
		bool HistoricalDataIsEarlierThan(DateOnly date);
	}
}