using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public class PayrollBaseUserControl : BaseUserControlWithMessageBrokerHandler, ICommonBehavior
    {
        private readonly IExplorerView _explorerView;

        public IExplorerView ExplorerView
        {
            get
            {
                return _explorerView;
            }
        }
        public PayrollBaseUserControl(IExplorerView explorerView)
        {
            _explorerView = explorerView;
            initializeComponent();
        }

        public PayrollBaseUserControl()
        {
            initializeComponent();
        }

        private void initializeComponent()
        {
            SuspendLayout();
            // 
            // PayrollBaseUserControl
            // 
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Name = "PayrollBaseUserControl";
            Size = new System.Drawing.Size(175, 150);
            ResumeLayout(false);

        }

        public virtual void AddNew()
        {
            
        }

        public virtual void DeleteSelected()
        {
            
        }

        public virtual void Rename()
        {
            
        }

        public virtual void Sort(SortingMode mode)
        {
            
        }

        public virtual void Cut()
        {
            
        }

        public virtual void Copy()
        {
            
        }

        public virtual void Paste()
        {
            
        }

        public virtual void MoveUp()
        {
            
        }

        public virtual void MoveDown()
        {
            
        }

        public virtual void Reload()
        {
            
        }

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
    }
}
