using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ForecastsAnalyzeCommandResult
    {
        public string ErrorMessage { get; set; }
        public bool Succeeded { get { return string.IsNullOrEmpty(ErrorMessage); } }
        public string SkillName { get; set; }
        public long IntervalLengthTicks { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IForecastFileDictionary ForecastFileDictionary { get; set; }
        public IWorkloadDayOpenHoursDictionary WorkloadDayOpenHours { get; set; }
    }
}