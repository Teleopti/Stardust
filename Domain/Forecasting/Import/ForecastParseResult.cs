namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class ForecastParseResult<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Value { get; set; }
    }
}