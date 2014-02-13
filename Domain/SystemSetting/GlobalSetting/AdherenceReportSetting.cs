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

	    public int AdherenceIdForReport()
	    {
					switch (CalculationMethod)
					{
					    case AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime:
					        return 1;
					
					    case AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime:
					        return 2;
					
					    case AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime:
					        return 3;
					
					}
		    throw new NotSupportedException("Illegal adherence report setting");
	    }
    }
}