using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IForecastsAnalyzeCommandResult
    {
        string ErrorMessage { get; set; }
        bool Succeeded { get; set; }
        string SkillName { get; set; }
        long IntervalLengthTicks { get; set; }
        DateOnlyPeriod Period { get; set; }
        IForecastFileContainer ForecastFileContainer { get; set; }
        IWorkloadDayOpenHoursContainer WorkloadDayOpenHours { get; set; }
    }

    public class ForecastsAnalyzeCommandResult : IForecastsAnalyzeCommandResult
    {
        public string ErrorMessage { get; set; }
        public bool Succeeded { get { return string.IsNullOrEmpty(ErrorMessage); } set{} }
        public string SkillName { get; set; }
        public long IntervalLengthTicks { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IForecastFileContainer ForecastFileContainer { get; set; }
        public IWorkloadDayOpenHoursContainer WorkloadDayOpenHours { get; set; }
    }
}