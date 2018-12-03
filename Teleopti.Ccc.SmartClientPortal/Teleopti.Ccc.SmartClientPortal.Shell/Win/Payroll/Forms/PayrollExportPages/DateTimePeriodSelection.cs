using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages
{
    public partial class DateTimePeriodSelection : BaseUserControl, IPropertyPage
    {
        public DateTimePeriodSelection()
        {
            InitializeComponent();
            SetColors();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
				//dateSelectionControlPeriod.TabPanelBackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;
            DateOnlyPeriod dateTimePair = payrollExport.Period;
            dateSelectionControlPeriod.SetInitialDates(dateTimePair);
            
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;
            IList<DateOnlyPeriod> dateTimePairs = dateSelectionControlPeriod.GetCurrentlySelectedDates();
            if (dateTimePairs.Count == 0)
            {
                return false;
            }
            DateOnlyPeriod dateTimePair = dateTimePairs[0];
            payrollExport.Period = dateTimePair;
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return UserTexts.Resources.PeriodSelection; }
        }
    }
}
