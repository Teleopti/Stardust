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

		public virtual double AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength, double minOcc, double maxOcc,
			int maxParallelTasks)
		{
			return _secretService.AgentsUseOccupancy(serviceLevel, serviceTime, tasks/maxParallelTasks, aht, periodLength, minOcc, maxOcc, 1);
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

		public double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents, int maxParellelTasks)
		{
			if (calls/maxParellelTasks <= 2)
			{
				return LinearEsl(forecastedAgents, agents, sla);
			}
			return _serviceLevelAchivedOcc.ServiceLevelAchived(forecastedAgents, agents, sla, (int) serviceTime, calls/maxParellelTasks, TimeSpan.FromSeconds(aht), intervalLength);
		}

		public double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParellelTasks)
		{
			return _secretService.Utilization(demandWithoutEfficiency, tasks/maxParellelTasks, aht, periodLength);
		}
	}

	public class StaffingCalculatorServiceFacadeErlangA : StaffingCalculatorServiceFacade
	{
		private readonly NumberOfAgentsNeeded _agentsNeeded;
		public StaffingCalculatorServiceFacadeErlangA()
		{
			_agentsNeeded = new NumberOfAgentsNeeded();
		}

		public override double AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength,
			double minOcc, double maxOcc, int maxParallelTasks)
		{
			var result = _agentsNeeded.CalculateNumberOfAgentsNeeded(tasks / maxParallelTasks, aht, 100000, serviceTime, serviceLevel,
				(int)periodLength.TotalSeconds, minOcc, maxOcc);
			return result.NumberOfAgentsNeeded;
		}
	}
}