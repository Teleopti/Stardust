using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    [Serializable]
    public class AdherenceReportSetting : SettingValue
    {
        
        private AdherenceReportSettingCalculationMethod _calculationMethod =
            AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime;

        public AdherenceReportSettingCalculationMethod CalculationMethod
        {
            get { return _calculationMethod; }
            set { _calculationMethod = value; }
        }

        public static int MapToMatrix(AdherenceReportSettingCalculationMethod calculationMethod)
        {
            switch (calculationMethod)
            {
                case AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime:
                    return 1;

                case AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime:
                    return 2;

                case AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime:
                    return 3;

            }

            return 0;
        }
    }
}