using System;
using Teleopti.Ccc.Domain.Calculation;

namespace Teleopti.Interfaces.Domain
{
	public interface IStaffingCalculatorServiceFacade : IStaffingCalculatorService
	{
		double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents);
	}
}