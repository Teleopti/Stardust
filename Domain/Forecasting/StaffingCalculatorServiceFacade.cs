using System;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Calculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class StaffingCalculatorServiceFacade : IStaffingCalculatorServiceFacade
	{
		private readonly IStaffingCalculatorService _secretService;
		private readonly ServiceLevelAchivedOcc _serviceLevelAchivedOcc;

		public StaffingCalculatorServiceFacade()
		{
			_secretService = new StaffingCalculatorService();
			_serviceLevelAchivedOcc = new ServiceLevelAchivedOcc(new TeleoptiCallCapacity(_secretService),  _secretService);
		}

		public double AgentsUseOccupancy(double serviceLevel, int serviceTime, double tasks, double aht, TimeSpan periodLength, double minOcc, double maxOcc,
			int maxParallelTasks)
		{
			return _secretService.AgentsUseOccupancy(serviceLevel, serviceTime, tasks/maxParallelTasks, aht, periodLength, minOcc, maxOcc, 1);
		}

		public double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents, int maxParellelTasks)
		{
			return _serviceLevelAchivedOcc.ServiceLevelAchived(forecastedAgents, agents, sla, (int) serviceTime, calls, TimeSpan.FromSeconds(aht), intervalLength);
		}

		public double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParellelTasks)
		{
			return _secretService.Utilization(demandWithoutEfficiency, tasks/maxParellelTasks, aht, periodLength);
		}
	}
}