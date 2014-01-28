using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    [Serializable]
    public class AdherenceReportSetting : SettingValue
    {
	    public const string Key = "AdherenceReportSetting";

	    public AdherenceReportSetting()
	    {
				CalculationMethod = AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime;
	    }
	    public AdherenceReportSettingCalculationMethod CalculationMethod { get; set; }
    }
}