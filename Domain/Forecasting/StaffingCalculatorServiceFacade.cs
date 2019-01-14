using System;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.Calculation;
using Teleopti.Ccc.Secrets.ErlangA;

namespace Teleopti.Ccc.Domain.Forecasting
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_UseErlangAWithInfinitePatience_45845)]
	public class StaffingCalculatorServiceFacade : IStaffingCalculatorServiceFacade
	{
		private readonly IStaffingCalculatorService _secretService;
		private readonly ServiceLevelAchivedOcc _serviceLevelAchivedOcc;

		public StaffingCalculatorServiceFacade()
		{
			_secretService = new StaffingCalculatorService();
			_serviceLevelAchivedOcc = new ServiceLevelAchivedOcc(new TeleoptiCallCapacity(_secretService),  _secretService);
		}

		public virtual AgentsAndOccupancy AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength, double minOcc, double maxOcc,
			int maxParallelTasks, double abandonRate)
		{
			return new AgentsAndOccupancy()
			{
				Agents = _secretService.AgentsUseOccupancy(serviceLevel, serviceTime, tasks / maxParallelTasks, aht, periodLength, minOcc, maxOcc, 1),
				Occupancy = 0
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

		public virtual double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents, 
			int maxParellelTasks, double abandonRate)
		{
			if (calls/maxParellelTasks <= 2)
			{
				return LinearEsl(forecastedAgents, agents, sla);
			}
			return _serviceLevelAchivedOcc.ServiceLevelAchived(forecastedAgents, agents, sla, (int) serviceTime, calls/maxParellelTasks, TimeSpan.FromSeconds(aht), intervalLength);
		}

		public virtual double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParellelTasks, double occupancy)
		{
			return _secretService.Utilization(demandWithoutEfficiency, tasks/maxParellelTasks, aht, periodLength);
		}
	}

	[RemoveMeWithToggle("Move AgentsUseOccupancy to StaffingCalculatorServiceFacadeErlangA", Toggles.ResourcePlanner_UseErlangAWithFinitePatience_47738)]
	public class StaffingCalculatorServiceFacadeErlangAAbandon : StaffingCalculatorServiceFacadeErlangAWithEsl
	{
		private readonly NumberOfAgentsNeededAbandonment _agentsNeededAbandonment;
		public StaffingCalculatorServiceFacadeErlangAAbandon()
		{
			_agentsNeededAbandonment = new NumberOfAgentsNeededAbandonment();
		}

		public override AgentsAndOccupancy AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength,
			double minOcc, double maxOcc, int maxParallelTasks, double abandonRate)
		{
			var result = _agentsNeededAbandonment.Execute(tasks / maxParallelTasks, aht, serviceTime, serviceLevel,
				(int)periodLength.TotalSeconds, minOcc, maxOcc, abandonRate);


			return new AgentsAndOccupancy()
			{
				Agents = result.NumberOfAgentsNeeded,
				Occupancy = result.Occupancy
			};
		}

		public override double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, 
			double forecastedAgents, int maxParellelTasks, double abandonRate)
		{
			if (calls / maxParellelTasks <= 2)
			{
				return LinearEsl(forecastedAgents, agents, sla);
			}

			return EstimatedServiceLevelAbandonment.Execute(agents, calls / maxParellelTasks, aht, (int)serviceTime, (int)intervalLength.TotalSeconds, 
				forecastedAgents, sla, abandonRate);
		}

		public override double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParellelTasks, double occupancy)
		{
			return occupancy;
		}

	}

	[RemoveMeWithToggle("Move ServiceLevelAchievedOcc to StaffingCalculatorServiceFacadeErlangA", Toggles.ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899)]
	public class StaffingCalculatorServiceFacadeErlangAWithEsl : StaffingCalculatorServiceFacadeErlangA
	{
		private readonly EstimatedServiceLevel _estimatedServiceLevel;
		public StaffingCalculatorServiceFacadeErlangAWithEsl()
		{
			_estimatedServiceLevel = new EstimatedServiceLevel();
		}

		public override double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents, 
			int maxParellelTasks, double abandonRate)
		{
			if (calls / maxParellelTasks <= 2)
			{
				return LinearEsl(forecastedAgents, agents, sla);
			}

			return _estimatedServiceLevel.Execute(agents, calls / maxParellelTasks, aht, 10000000, (int) serviceTime, (int) intervalLength.TotalSeconds, forecastedAgents,sla);
		}
	}

	public class StaffingCalculatorServiceFacadeErlangA : StaffingCalculatorServiceFacade
	{
		private readonly NumberOfAgentsNeeded _agentsNeeded;
		public StaffingCalculatorServiceFacadeErlangA()
		{
			_agentsNeeded = new NumberOfAgentsNeeded();
		}

		public override AgentsAndOccupancy AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength,
			double minOcc, double maxOcc, int maxParallelTasks, double abandonRate)
		{
			var result = _agentsNeeded.Execute(tasks / maxParallelTasks, aht, 10000000, serviceTime, serviceLevel,
				(int)periodLength.TotalSeconds, minOcc, maxOcc);

			return new AgentsAndOccupancy()
			{
				Agents = result.NumberOfAgentsNeeded,
				Occupancy = result.Occupancy
			};
		}
	}

	public class AgentsAndOccupancy
	{
		public double Agents { get; set; }
		public double Occupancy { get; set; }
	}
}