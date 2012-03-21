namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IForecastsFileValidator
    {
        bool Validate(string value);
        string ErrorMessage { get; set; }
    }
}
