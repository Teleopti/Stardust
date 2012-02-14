#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Domain.Repositories
{

    /// <summary>
    /// Defines the functionality of the system setting repository
    /// </summary>
    public interface ISystemSettingRepository
    {

        #region Properties - Instance Member

        #endregion

        #region Methods - Instance Member

        /// <summary>
        /// Finds the by setting key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-20
        /// </remarks>
        ISystemSetting FindBySettingKey(SettingKeys key);

        /// <summary>
        /// Finds the by business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-20
        /// </remarks>
        IList<ISystemSetting> FindByBusinessUnit(IBusinessUnit businessUnit);

        #endregion

    }

}
