namespace Teleopti.Ccc.Win.Common.Configuration
{
	/// <summary>
	/// Interface for controls which can be added to OptionDialog.
	/// </summary>
	/// <remarks>
	/// Created By: kosalanp
	/// Created Date: 02-04-2008
	/// </remarks>
    public interface ISettingPage : ISelfDataHandling, IExternalModuleLoader
    {
        /// <summary>
        /// Manually initialze control components. Calls when OptionDialog contructor.
        /// </summary>
        void InitializeDialogControl();
        /// <summary>
        /// Manually load control details. Calls when OptionDialog loads.
        /// </summary>
        void LoadControl();
        /// <summary>
        /// Save unsaved changes. If the Page has it's own UnitOfWork it should call Persist on that.
        /// If the Page uses the common UnitOfWork PersistAll will be called on that later.
        /// Is called when Ok or Apply is clicked.
        /// </summary>
        void SaveChanges();
        /// <summary>
        /// Called when the dialog is closed.
        /// </summary>
        void Unload();
        /// <summary>
        /// The name of the Parent if represented in a TreeView
        /// </summary>
        TreeFamily TreeFamily();
        /// <summary>
        /// The name of the Node if represented in a TreeView
        /// </summary>
         string TreeNode();

		 /// <summary>
		 /// Called when the Page is Shown.
		 /// </summary>
    	void OnShow();
    }
}
