using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.ErlangA;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class StaffingCalculatorServiceFacade : IStaffingCalculatorServiceFacade
	{
		private readonly NumberOfAgentsNeededAbandonment _agentsNeededAbandonment;

		public StaffingCalculatorServiceFacade()
		{
			_agentsNeededAbandonment = new NumberOfAgentsNeededAbandonment();
		}
		
		public AgentsAndOccupancy AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength,
			double minOcc, double maxOcc, int maxParallelTasks, double abandonRate)
		{
			var result = _agentsNeededAbandonment.Execute(tasks / maxParallelTasks, aht, serviceTime, serviceLevel,
				(int)periodLength.TotalSeconds, minOcc, maxOcc, abandonRate);

			return new AgentsAndOccupancy
			{
				Agents = result.NumberOfAgentsNeeded,
				Occupancy = result.Occupancy
			};
		}

		private static double slope(double x1, double x2, double y1, double y2)
		{
			var dX = x2 - x1;
			var dY = y2 - y1;
			return dY/dX;
		}

		private static double baseFromSlope(double slope, double x2, double y2)
		{
			return y2 - (x2 * slope);
		}

		public double LinearEsl(double forecastedAgents, double agents, double sla)
		{
			if (Math.Abs(forecastedAgents) < 0.001)
				return 0;

			var slope = StaffingCalculatorServiceFacade.slope(forecastedAgents, forecastedAgents/2, sla, 0);
			var baseY = baseFromSlope(slope, forecastedAgents, sla);
			var esl = (agents*slope) + baseY;
			return esl < 0 ? 0 : esl > 1 ? 1 : esl;
		}
		
		public double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, 
			double forecastedAgents, int maxParallelTasks, double abandonRate)
		{
			if (calls / maxParallelTasks <= 2)
			{
				return LinearEsl(forecastedAgents, agents, sla);
			}

			return EstimatedServiceLevelAbandonment.Execute(agents, calls / maxParallelTasks, aht, (int)serviceTime, (int)intervalLength.TotalSeconds, 
				forecastedAgents, sla, abandonRate);
		}
		

		public virtual double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParallelTasks, double occupancy)
		{
			return occupancy;
		}
	}

	public class AgentsAndOccupancy
	{
		public double Agents { get; set; }
		public double Occupancy { get; set; }
	}
}