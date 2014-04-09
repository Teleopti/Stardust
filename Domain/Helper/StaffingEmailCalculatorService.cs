using System;
using Teleopti.Ccc.Domain.Calculation;

namespace Teleopti.Ccc.Domain.Helper
{
    public class StaffingEmailCalculatorService : IStaffingCalculatorService
    {
		public double TeleoptiAgents(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength)
		{
			if (sla > 1d)
			{
				throw new ArgumentOutOfRangeException("sla", "SLA must not be > 1");
			}

			if (serviceTime == 0 || averageHandlingTime == 0) return 0;
			double workPerAgent = periodLength.TotalSeconds/averageHandlingTime;
			return calls/workPerAgent;
		}

    	public double AgentsFromUtilization(double theUtilization, double theCallsPerHour, double averageHandlingTime, TimeSpan periodLength)
        {
            return 0;
        }

        public double AgentsUseOccupancy(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength, double minOccupancy, double maxOccupancy, int maxParallelTasks)
        {
			return TeleoptiAgents(sla, serviceTime, calls, averageHandlingTime, periodLength);
        }

        public double ServiceLevelAchieved(double agents, double serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength, int orderedSla)
        {
            return 1;
        }

        public double TeleoptiErgBExtended(double servers, double intensity)
        {
            return 0;
        }

        public double TeleoptiErgCExtended(double servers, double intensity)
        {
            return 0;
        }

        public double Utilization(double agents, double callsPerHour, double averageHandlingTime, TimeSpan periodLength)
        {
            return 1; //Always 100%
        }
    }
}