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

		public double TeleoptiAgents(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength)
		{
			return _secretService.TeleoptiAgents(sla, serviceTime, calls, averageHandlingTime, periodLength);
		}

		public double AgentsFromUtilization(double obj0, double obj1, double obj2, TimeSpan obj3)
		{
			return _secretService.AgentsFromUtilization(obj0, obj1, obj2, obj3);
		}

		public double AgentsUseOccupancy(double obj0, int obj1, double obj2, double obj3, TimeSpan obj4, double obj5, double obj6,
			int obj7)
		{
			return _secretService.AgentsUseOccupancy(obj0, obj1, obj2, obj3, obj4, obj5, obj6, obj7);
		}

		public double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength, double sla, double forecastedAgents)
		{
			return _serviceLevelAchivedOcc.ServiceLevelAchived(forecastedAgents, agents, sla, (int) serviceTime, calls, TimeSpan.FromSeconds(aht), intervalLength);
		}

		public double ServiceLevelAchieved(double obj0, double obj1, double obj2, double obj3, TimeSpan obj4, int obj5)
		{
			return _secretService.ServiceLevelAchieved(obj0, obj1, obj2, obj3, obj4, obj5);
		}

		public double TeleoptiErgBExtended(double obj0, double obj1)
		{
			throw new NotImplementedException();
		}

		public double TeleoptiErgCExtended(double obj0, double obj1)
		{
			throw new NotImplementedException();
		}

		public double Utilization(double obj0, double obj1, double obj2, TimeSpan obj3)
		{
			return _secretService.Utilization(obj0, obj1, obj2, obj3);
		}
	}
}