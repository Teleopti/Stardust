#region Imports

using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.WinCode.Common
{

    /// <summary>
    /// Represents a SystemSettingReader
    /// </summary>
    public class SystemSettingReader : ISystemSettingReader
    {
        #region Fields - Instance Member

        private readonly IList<ISetting> _settingsList;

        private readonly ISettingCategory _settingCategory;
        
        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - SystemSettingReader Members

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified key.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-21
        /// </remarks>
        public object this[SettingKeys key]
        {
            get
            {
                object value = null;
                if (_settingsList != null && _settingsList.Count > 0)
                {
                    ISetting setting = _settingsList.FirstOrDefault(k => k.Name == key.ToString());
                    if(setting == null)
                    {
                        setting = _settingCategory.GetOrCreateSetting(key.ToString(), 15);
                        _settingCategory.AddValue(setting);
                    }
                    value = setting.Value;
                }
                return value;
            }
        }

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - SystemSettingReader Members

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemSettingReader"/> class.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-21
        /// </remarks>
        public SystemSettingReader()
        {
            _settingsList = new List<ISetting>();
            Array settingValues = Enum.GetValues(typeof(SettingKeys));
            _settingCategory = CommonStateHolder.Instance.RootSettingCategory.GetOrCreateCategory("RaptorDefaultSegment");

            IList<ISetting> currentSettings = _settingCategory.Values;

            if (currentSettings != null && currentSettings.Count > 0)
            {
                for (int i = 0; i <= (currentSettings.Count - 1); i++)
                {
                    _settingsList.Add(currentSettings[i]);
                }
            }
            else
            {
                for (int i = 0; i <= (settingValues.Length - 1); i++)
                {
                    SettingKeys settingKey = (SettingKeys) settingValues.GetValue(i);
                    ISetting setting = _settingCategory.GetOrCreateSetting(Enum.GetName(typeof (SettingKeys), settingKey), 15);
                    _settingsList.Add(setting);
                }
            }
        }

        #endregion

        #endregion

    }

}
