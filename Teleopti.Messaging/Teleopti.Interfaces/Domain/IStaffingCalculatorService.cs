using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for StaffingCalculatorService
    /// </summary>
    public interface IStaffingCalculatorService
    {
		/// <summary>
		/// Under construction, do not use.
		/// </summary>
		/// <param name="sla">The sla.</param>
		/// <param name="serviceTime">The service time.</param>
		/// <param name="calls">The calls.</param>
		/// <param name="averageHandlingTime">The average handling time.</param>
		/// <param name="periodLength">Length of the period.</param>
		/// <returns></returns>
    	double TeleoptiAgents(double sla, int serviceTime, double calls, double averageHandlingTime, TimeSpan periodLength);

        /// <summary>
        /// Agentses from utilisation.
        /// </summary>
        /// <param name="theUtilization">The utilization.</param>
        /// <param name="theCallsPerHour">The calls per hour.</param>
        /// <param name="averageHandlingTime">The avg HT.</param>
        /// <param name="periodLength">Length of the period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-10
        /// </remarks>
        double AgentsFromUtilization(double theUtilization, double theCallsPerHour, double averageHandlingTime,
                                     TimeSpan periodLength);

	    /// <summary>
	    /// Agentses the use occupancy.
	    /// </summary>
	    /// <param name="sla">The sla.</param>
	    /// <param name="serviceTime">The service time.</param>
	    /// <param name="calls">The calls.</param>
	    /// <param name="averageHandlingTime">The avg HT.</param>
	    /// <param name="periodLength">Length of the period.</param>
	    /// <param name="minOccupancy">The min occ.</param>
	    /// <param name="maxOccupancy">The max occ.</param>
	    /// <param name="maxParallelTasks">only != 1 on chats</param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2008-01-10
	    /// </remarks>
	    double AgentsUseOccupancy(double sla, int serviceTime, double calls, double averageHandlingTime,
                                TimeSpan periodLength, double minOccupancy, double maxOccupancy,
								int maxParallelTasks);

        /// <summary>
        /// Services the level achived.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="serviceTime">The service time.</param>
        /// <param name="calls">The calls.</param>
        /// <param name="averageHandlingTime">The avg HT.</param>
        /// <param name="periodLength">Length of the period.</param>
        /// <param name="orderedSla">The ordered sla.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-10
        /// </remarks>
        double ServiceLevelAchieved(double agents, double serviceTime, double calls, double averageHandlingTime,
                                    TimeSpan periodLength, int orderedSla);

        /// <summary>
        /// Erlangs the b2.
        /// </summary>
        /// <param name="servers">The servers.</param>
        /// <param name="intensity">The intensity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-10
        /// </remarks>
        double TeleoptiErgBExtended(double servers, double intensity);

        /// <summary>
        /// Erlangs the c2.
        /// </summary>
        /// <param name="servers">The servers.</param>
        /// <param name="intensity">The intensity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-10
        /// </remarks>
        double TeleoptiErgCExtended(double servers, double intensity);

        /// <summary>
        /// Utilisations the specified agents.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="callsPerHour">The calls per hour.</param>
        /// <param name="averageHandlingTime">The avg HT.</param>
        /// <param name="periodLength">An interval lenght.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-10
        /// </remarks>
        double Utilization(double agents, double callsPerHour, double averageHandlingTime, TimeSpan periodLength);
    }
}