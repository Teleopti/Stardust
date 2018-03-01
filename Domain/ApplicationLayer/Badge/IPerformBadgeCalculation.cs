using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IPerformBadgeCalculation
	{
		void Calculate(Guid businessUnitId, DateTime date);
	}
}