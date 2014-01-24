using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    [Serializable]
    public class AdherenceReportSetting : SettingValue
    {
	    public const string Key = "AdherenceReportSetting";

	    public AdherenceReportSetting()
	    {
		    CalculationMethod = AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime;
	    }
	    public AdherenceReportSettingCalculationMethod CalculationMethod { get; set; }
    }
}