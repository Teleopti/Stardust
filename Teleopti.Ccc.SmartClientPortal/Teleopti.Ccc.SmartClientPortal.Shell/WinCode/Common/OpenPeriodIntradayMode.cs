using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class OpenPeriodIntradayMode : IOpenPeriodMode
    {
        private readonly ISpecification<DateOnlyPeriod> _specification;
        private const string SETTING_NAME = "Intraday";
        private const int MAX_NUMBER_OF_DAYS = 14;

        public OpenPeriodIntradayMode()
        {
            _specification = new OpenPeriodSpecification(MAX_NUMBER_OF_DAYS);
        }

        public ISpecification<DateOnlyPeriod> Specification
        {
            get { return _specification; }
        }

        public string SettingName
        {
            get { return SETTING_NAME; }
        }

        public bool ConsiderRestrictedScenarios
        {
            get
            {
                return true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])")]
        public string AliasOfMaxNumberOfDays
        {
            get { return string.Format(CultureInfo.CurrentUICulture, "2 " + UserTexts.Resources.Weeks).ToLower(CultureInfo.CurrentUICulture); }
        }

        public bool ForecasterStyle
        {
            get { return true; }
        }
    }
}