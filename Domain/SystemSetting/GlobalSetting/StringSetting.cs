using System;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    /// <summary>
    /// Class to save siple string values
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-09-22
    /// </remarks>
    [Serializable]
    public class StringSetting : SettingValue
    {
        private string _stringValue = string.Empty;

        public string StringValue
        {
            get { return _stringValue; }
            set { _stringValue = value; }
        }
    }
}
