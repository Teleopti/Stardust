using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ForecastScheduleValuePair : IForecastScheduleValuePair
	{
		private double _forecastValue;
		private double _scheduleValue;

		public double ForecastValue
		{
			get { return _forecastValue; }
			set { _forecastValue = value; }
		}

		public double ScheduleValue
		{
			get { return _scheduleValue; }
			set { _scheduleValue = value; }
		}
	}
}
