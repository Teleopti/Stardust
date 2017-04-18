using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// This will have common implementations for drop down grids.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-09-24
    /// </remarks>
    public abstract class GridViewModelBase<T> : EntityContainer<T> where T : IEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance can bold.
        /// </summary>
        /// <value><c>true</c> if this instance can bold; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-24
        /// </remarks>
        [DefaultValue(true)]
        public bool CanBold
        {
            get;
            set;
        }
    }
}