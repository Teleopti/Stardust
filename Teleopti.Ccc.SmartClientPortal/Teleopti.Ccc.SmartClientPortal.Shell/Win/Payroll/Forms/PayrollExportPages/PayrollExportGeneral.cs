using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages
{
    public partial class PayrollExportGeneral : BaseUserControl, IPropertyPage
    {
        public PayrollExportGeneral()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColor();
        }

        private void SetColor()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;
            textBoxName.Text = payrollExport.Name;
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;
            payrollExport.Name = textBoxName.Text;
            return true;
        }

        public void SetEditMode()
        {
            
        }

        public string PageName
        {
            get { return UserTexts.Resources.PayrollExportGeneralInformation; }
        }
    }
}
