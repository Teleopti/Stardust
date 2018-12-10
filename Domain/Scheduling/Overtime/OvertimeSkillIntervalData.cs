using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeSkillIntervalData
    {
        DateTimePeriod Period { get; }
        double CurrentDemand { get; }
        double ForecastedDemand { get; }
	    void Add(double forecastedDemand, double currentDemand);
    }

    public class OvertimeSkillIntervalData : IOvertimeSkillIntervalData
    {
        private double _forecastedDemand;
	    public DateTimePeriod Period { get; }
	    public double CurrentDemand { get; private set; }

		public OvertimeSkillIntervalData(DateTimePeriod dateTimePeriod, double forecastedDemand, double currentDemand)
        {
            _forecastedDemand = forecastedDemand;
            CurrentDemand = currentDemand;
            Period = dateTimePeriod;
        }

	    public double ForecastedDemand
        {
            get
            {
                if (_forecastedDemand == 0)
                    _forecastedDemand = 0.01;
                return _forecastedDemand;
            }
        }

	    public void Add(double forecastedDemand, double currentDemand)
	    {
		    _forecastedDemand += forecastedDemand;
		    CurrentDemand += currentDemand;
	    }

    }
}