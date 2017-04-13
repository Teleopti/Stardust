#region Imports

using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Views;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    /// <summary>
    /// The base user control for payrol related views.
    /// </summary>
    public class BaseUserControl : SmartClientPortal.Shell.Win.Common.BaseUserControl, ICommonOperation
    {
        private readonly IExplorerPresenter _explorerPresenter;

        /// <summary>
        /// Gets the explorer presenter.
        /// </summary>
        /// <value>The explorer presenter.</value>
        public IExplorerPresenter ExplorerPresenter
        {
            get
            {
                return _explorerPresenter;
            }
        }

        /// <summary>
        /// Gets or sets the explorer view.
        /// </summary>
        /// <value>The explorer view.</value>
        public IExplorerView ExplorerView
        {
            get
            {
                return _explorerPresenter.View;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserControl"/> class.
        /// </summary>
        public BaseUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserControl"/> class.
        /// </summary>
        /// <param name="explorerPresenter">The explorer presenter.</param>
        public BaseUserControl(IExplorerPresenter explorerPresenter)
        {
            InitializeComponent();
            _explorerPresenter = explorerPresenter;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BaseUserControl
            // 
            this.Name = "BaseUserControl";
            this.Size = new System.Drawing.Size(175, 150);
            this.ResumeLayout(false);

        }

        #region ICommonOperation Members

        /// <summary>
        /// Adds the new.
        /// </summary>
        public virtual void Add()
        {
            
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public virtual void Delete()
        {
            
        }

        /// <summary>
        /// Renames this instance.
        /// </summary>
        public virtual void Rename()
        {
            
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        /// <param name="mode"></param>
        public virtual void Sort(SortingMode mode)
        {
            
        }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        public virtual void Cut()
        {
            
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public virtual void Copy()
        {
            
        }

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public virtual void Paste()
        {
            
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public virtual void RefreshView()
        {

        }

        public virtual void Amounts(IList<int> shiftAmount)
        {

        }

        /// <summary>
        /// Clears the view.
        /// </summary>
        public virtual void Clear()
        {
            
        }

        /// <summary>
        /// Pastes the special.
        /// </summary>
        public virtual void PasteSpecial()
        {
            
        }

        #endregion
    }
}
