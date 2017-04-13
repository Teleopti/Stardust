using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
    [Serializable]
    public class IntradaySettingsPresenter : SettingValue
    {

        private IList<IntradaySetting> _intradaySettings = new List<IntradaySetting>();
        private IntradaySetting _intradaySetting;

        public IList<IntradaySetting> IntradaySettings
        {
            get { return _intradaySettings; }
        }

        public IntradaySetting CurrentIntradaySetting
        {
            get { return _intradaySetting; }
        }

        public void RemoveIntradaySetting(IntradaySetting intradaySetting)
        {
            _intradaySettings.Remove(intradaySetting);
        }

        public void SetIntradaySetting(string intradaySettingName)
        {
            _intradaySetting = GetIntradaySetting(intradaySettingName);
        }

        public IntradaySetting GetIntradaySetting(string intradaySettingName)
        {
            IntradaySetting intradaySetting = _intradaySettings.FirstOrDefault(s => s.Name == intradaySettingName);
            if (intradaySetting == null)
            {
                intradaySetting = new IntradaySetting(intradaySettingName);
                _intradaySettings.Add(intradaySetting);
            }

            return intradaySetting;
        }
    }
}