using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IForecastsFileValidator
    {
        bool Validate(string value);
        string ErrorMessage { get; set; }
    }
    
    public class ForecastsFileSkillNameValidator : IForecastsFileValidator
    {
        private const int maxSkillNameLength = 50;

        public bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ErrorMessage = "Skill name should not be empty";
                return false;
            }
            if (value.Length > maxSkillNameLength)
            {
                ErrorMessage = string.Format("Skill name is longer than {0} characters.",
                                             maxSkillNameLength);
                return false;
            }
            return true;
        }

        public string ErrorMessage { get; set; }
    }

    public class ForecastsFileDateTimeValidator : IForecastsFileValidator
    {
        public bool Validate(string value)
        {
            try
            {
                DateTime result;
                if (DateTime.TryParseExact(value, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out result))
                    return true;
                ErrorMessage = string.Format("Datetime format of {0} is wrong", value);
                return false;
            }
            catch (ArgumentException)
            {
                ErrorMessage = string.Format("Datetime format of {0} is wrong", value);
                return false;
            }
        }

        public string ErrorMessage { get; set; }
    }

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
