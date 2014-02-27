using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeSkillIntervalData
    {
        DateTimePeriod Period { get; }
        double CurrentDemand { get; }
        double ForecastedDemand { get; }
        double RelativeDifference();
    }

    public class OvertimeSkillIntervalData : IOvertimeSkillIntervalData
    {
        private double _forecastedDemand;
        private readonly double _currentDemand;
        public DateTimePeriod Period { get; private set; }

        public OvertimeSkillIntervalData(DateTimePeriod dateTimePeriod, double forecastedDemand, double currentDemand)
        {
            _forecastedDemand = forecastedDemand;
            _currentDemand = currentDemand;
            Period = dateTimePeriod;
        }

        public double CurrentDemand { get { return _currentDemand; } }

        public double ForecastedDemand
        {
            get
            {
                if (_forecastedDemand == 0)
                    _forecastedDemand = 0.01;
                return _forecastedDemand;
            }
        }

        public double RelativeDifference()
        {
            return (CurrentDemand / ForecastedDemand) * -1;

        }

    }
}