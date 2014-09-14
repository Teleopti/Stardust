using System;
using Teleopti.Ccc.Domain.Calculation;

namespace Teleopti.Interfaces.Domain
{
	public interface IStaffingCalculatorServiceFacade : IStaffingCalculatorService
	{
		double ServiceLevelAchievedOcc(double obj0, double obj1, double obj2, double obj3, TimeSpan obj4, int obj5, double forecastedAgents);
	}
}