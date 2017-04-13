using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages
{
    public partial class PayrollWelcomePage : BaseUserControl, IPropertyPage
    {
        public PayrollWelcomePage()
        {
            InitializeComponent();
            SetColors();
            labelWelcome.Text = UserTexts.Resources.CreatePayrollExport;
        }

        private void SetColors()
        {
            this.BackColor = ColorHelper.WizardBackgroundColor();
        }


        public void Populate(IAggregateRoot aggregateRoot)
        {
            //UserTexts.Resources.
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return UserTexts.Resources.Welcome; }
        }
    }
}
