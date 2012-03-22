using System.Globalization;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDoubleValueValidator : IForecastsFileValidator
    {
        private readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        public bool Validate(string value)
        {
            double result;
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, _cultureInfo, out result))
                return true;
            ErrorMessage = string.Format("{0} should be a double value.", value);
            return false;
        }

        public string ErrorMessage { get; set; }
    }
}