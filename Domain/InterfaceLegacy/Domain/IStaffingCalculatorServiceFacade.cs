using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IStaffingCalculatorServiceFacade
	{
		double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents, int maxParellelTasks);
		
		double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParallelTasks);

		double AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength, double minOcc, double maxOcc,
			int maxParallelTasks, double abandonRate);

		double LinearEsl(double forecastedAgents, double agents, double sla);
	}
}