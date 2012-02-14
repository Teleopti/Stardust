using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a SettingDto object.
    /// </summary>
    [DataContract]
    public class SettingDto : Dto
    {
        private string _settingName;
        private string _settingValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDto"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public SettingDto(ISetting setting)
        {
            Id = setting.Id;
            SettingName = setting.Name;
            SettingValue = setting.Value.ToString();
        }

        /// <summary>
        /// Gets or sets the name of the setting.
        /// </summary>
        /// <value>The name of the setting.</value>
        [DataMember]
        public string SettingName 
        {
            get { return _settingName; }
            set { _settingName = value; }
        }

        /// <summary>
        /// Gets or sets the setting value.
        /// </summary>
        /// <value>The setting value.</value>
        [DataMember]
        public string SettingValue 
        {
            get { return _settingValue; }
            set { _settingValue = value; }
        }
    }
}