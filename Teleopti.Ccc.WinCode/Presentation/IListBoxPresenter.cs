using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCode.Presentation
{
    /// <summary>
    /// Interface for the general description for entities.
    /// </summary>
    /// <remarks>
    /// Many cases the gui controls (mostly list box type controls and treeview based controls)
    /// needs a description for the item. With implementing this interface the domain object 
    /// will carry this information.
    /// </remarks>
    public interface IListBoxPresenter : IEntityPresenter
    {
        /// <summary>
        /// Gets the Name value. Usually this is the short name.
        /// </summary>
        /// <value>The key field.</value>
        /// <remarks>
        /// Usually this value goes to the name field of a control.
        /// </remarks>
        string DataBindText { get; }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        string DataBindDescriptionText { get; }
    }

}
