using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
	public class StaffingEmailCalculatorService : IStaffingCalculatorServiceFacade
    {
		private static double teleoptiAgents(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength)
		{
			if (sla > 1d)
			{
				throw new ArgumentOutOfRangeException("sla", "SLA must not be > 1");
			}

			if (serviceTime == 0 || averageHandlingTime == 0) return 0;
			double workPerAgent = periodLength.TotalSeconds/averageHandlingTime;
			return calls/workPerAgent;
		}

        public AgentsAndOccupancy AgentsUseOccupancy(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength, double minOccupancy, double maxOccupancy, int maxParallelTasks, double abandonRate)
        {
			return new AgentsAndOccupancy()
			{
				Agents = teleoptiAgents(sla, serviceTime, calls, averageHandlingTime, periodLength),
				Occupancy = 0
			};
		}

		public double LinearEsl(double forecastedAgents, double agents, double sla)
		{
			throw new NotImplementedException();
		}


		public double ServiceLevelAchievedOcc(double agents, double serviceTime, double calls, double aht, TimeSpan intervalLength,
			double sla, double forecastedAgents, int maxParellelTasks, double abandonRate)
		{
			return 0;
			//return ServiceLevelAchieved(agents, serviceTime, calls, aht, intervalLength, (int)sla*100);
		}

		public double Utilization(double demandWithoutEfficiency, double tasks, double aht, TimeSpan periodLength, int maxParallelTasks, double occupancy)
		{
			return 1; //Always 100%
		}
    }
}