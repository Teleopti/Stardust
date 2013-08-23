#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortal.Common
{

    /// <summary>
    /// Defines the functionality of a Option Dialog Control.
    /// </summary>
    public interface IDialogControl
    {

        #region Methods - Instance Member

        /// <summary>
        /// Manually initialze control components. Calls when OptionDialog contructor.
        /// </summary>
        void InitializeDialogControl();
        /// <summary>
        /// Manually load control details. Calls when OptionDialog loads.
        /// </summary>
        void LoadControl();
        /// <summary>
        /// The name of the Parent if represented in a TreeView
        /// </summary>
        string TreeFamily();
        /// <summary>
        /// The name of the Node if represented in a TreeView
        /// </summary>
        string TreeNode();
        /// <summary>
        /// Persist all and save changes by the control. Calls when OkDialog hits.
        /// </summary>
        bool SaveChanges();

        #endregion

    }

}
