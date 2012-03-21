namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileIntegerValueValidator : IForecastsFileValidator
    {
        public bool Validate(string value)
        {
            int result;
            if (int.TryParse(value, out result))
                return true;
            ErrorMessage = string.Format("{0} should be an integer.", value);
            return false;
        }

        public string ErrorMessage { get; set; }
    }
}