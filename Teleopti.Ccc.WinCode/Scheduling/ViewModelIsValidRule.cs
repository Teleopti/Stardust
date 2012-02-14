using System.Globalization;
using System.Windows.Controls;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class ViewModelIsValidRule:ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            IViewModelIsValid model = value as IViewModelIsValid;
            if (model != null) return new ValidationResult(model.IsValid,model.InvalidMessage);
            return ValidationResult.ValidResult;
        }
    }
}
