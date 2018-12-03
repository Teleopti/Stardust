using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastsAnalyzeQueryResult
    {
        string ErrorMessage { get; set; }
        bool Succeeded { get; set; }
        string SkillName { get; set; }
        TimeSpan IntervalLength { get; set; }
        DateOnlyPeriod Period { get; set; }
        IForecastFileContainer ForecastFileContainer { get; set; }
        IWorkloadDayOpenHoursContainer WorkloadDayOpenHours { get; set; }
    }

    public class ForecastsAnalyzeQueryResult : IForecastsAnalyzeQueryResult
    {
        public string ErrorMessage { get; set; }
        public bool Succeeded { get { return string.IsNullOrEmpty(ErrorMessage); } set{} }
        public string SkillName { get; set; }
        public TimeSpan IntervalLength { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IForecastFileContainer ForecastFileContainer { get; set; }
        public IWorkloadDayOpenHoursContainer WorkloadDayOpenHours { get; set; }
    }
}