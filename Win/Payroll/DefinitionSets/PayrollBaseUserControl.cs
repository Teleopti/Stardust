using System;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    /// <summary>
    /// The base user control for payrol related views.
    /// </summary>
    public class PayrollBaseUserControl : BaseUserControlWithMessageBrokerHandler, ICommonBehavior
    {
        #region Fields - Instance Members

        private readonly IExplorerView _explorerView;

        #endregion

        #region Properties - Instance Member

        /// <summary>
        /// Gets or sets the explorer view.
        /// </summary>
        /// <value>The explorer view.</value>
        public IExplorerView ExplorerView
        {
            get
            {
                return _explorerView;
            }
        }

        #endregion

        #region Methods - Instance Memeber

        #region Methods - Instance Memeber - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollBaseUserControl"/> class.
        /// </summary>
        /// <param name="explorerView">The explorer view.</param>
        public PayrollBaseUserControl(IExplorerView explorerView)
        {
            _explorerView = explorerView;
            InitializeComponent();
        }

        #endregion

        #region Methods - Instance Memeber - Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollBaseUserControl"/> class.
        /// </summary>
        public PayrollBaseUserControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BaseUserControl
            // 
            this.Name = "PayrollBaseUserControl";
            this.Size = new System.Drawing.Size(175, 150);
            this.ResumeLayout(false);

        }

        #endregion

        #region Methods - Instance Memeber - ICommonBehavior Members

        /// <summary>
        /// Adds the new.
        /// </summary>
        public virtual void AddNew()
        {
            
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public virtual void DeleteSelected()
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
        /// Moves up.
        /// </summary>
        public virtual void MoveUp()
        {
            
        }

        /// <summary>
        /// Moves down.
        /// </summary>
        public virtual void MoveDown()
        {
            
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public virtual void Reload()
        {
            
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public virtual void RefreshView()
        {
            
        }

        public virtual string ToolTipDelete
        {
            get { return string.Empty; }
        }

        public virtual string ToolTipAddNew
        {
            get { return string.Empty; }
        }

        #endregion

        #endregion
    }
}
