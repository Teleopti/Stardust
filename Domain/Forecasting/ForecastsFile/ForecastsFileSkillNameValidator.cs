namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
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
}