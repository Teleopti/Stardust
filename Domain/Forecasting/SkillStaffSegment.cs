using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// A "segment" of skillstaffs - needed for stuff like emails, where the work
    /// takes more time than a skillstaff
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-09
    /// </remarks>
    public class SkillStaffSegment : ISkillStaffSegment
    {
        private double _forecastedDistributedDemand;

        public SkillStaffSegment(double forecastedDistributedDemand)
        {
            ForecastedDistributedDemand = forecastedDistributedDemand;
        }

        public double ForecastedDistributedDemand
        {
            get { return _forecastedDistributedDemand; }
            private set { _forecastedDistributedDemand = value; }
        }

    }
}
