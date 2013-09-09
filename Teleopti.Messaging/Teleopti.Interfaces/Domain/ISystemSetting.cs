#region Imports

using System;

#endregion

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Defines the functionality of a system setting instance
    /// </summary>
    public interface ISystemSetting : IAggregateRoot
    {
        #region Properties - Instance Member

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-20
        /// </remarks>
        SettingKeys Name { get; set; }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        /// <value>The type of the value.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-20
        /// </remarks>
        SettingValueTypes ValueType{ get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-20
        /// </remarks>
        string Value{ get; set;}

        #endregion

        #region Methods - Instance Member

        #endregion

    }

}
