using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileSkillNameValidator : IForecastsFileValidator<string>
    {
        private const int maxSkillNameLength = 50;

        public bool TryParse(string value, out ForecastParseResult<string> result)
        {
            result = new ForecastParseResult<string>();
            if (string.IsNullOrEmpty(value))
            {
                result.ErrorMessage = "Skill name should not be empty";
                return false;
            }
            if (value.Length > maxSkillNameLength)
            {
                result.ErrorMessage = string.Format("Skill name is longer than {0} characters.",
                                             maxSkillNameLength);
                return false;
            }
            result.Value = value;
            result.Success = true;
            return true;
        }
    }
}