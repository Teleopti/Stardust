#region Imports
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.SdkServiceReference;
using Teleopti.Interfaces.Domain;
#endregion

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    /// <summary>
    /// System settings for applivcation
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 11/5/2008
    /// </remarks>
    public class AgentPortalSettings
    {
        #region Fields - Instance Members

        private readonly ICollection<SettingDto> settingCollection;
        private const string DefaultValue = "15";

        #endregion

        #region Instance Methods - AgentPortalSettings Members - (constructors)

        public AgentPortalSettings(ICollection<SettingDto> settings)
        {
            this.settingCollection = settings;
        }

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified key.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/5/2008
        /// </remarks>
        public string this[SettingKeys key]
        {
            get
            {
                foreach (SettingDto setting in settingCollection)
                {
                    if (key.ToString() == setting.SettingName)
                    {
                        return setting.SettingValue;
                    }
                }

                return DefaultValue;
            }
        }

        #endregion
    }
}
