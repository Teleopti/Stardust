using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
    public partial class WizardWelcome : BaseUserControl, IPropertyPage
    {
        protected WizardWelcome()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();
        }
        private void SetColors()
        {
            this.BackColor = ColorHelper.WizardBackgroundColor();
            this.panelWelcome.BackColor = ColorHelper.WizardBackgroundColor();
            }

        public WizardWelcome(string welcomeHeading, 
            string welcomeDescription, 
            bool showUseWizardCheckBox)
            : this()
        {
            checkBoxUseWizard.Visible = showUseWizardCheckBox;
            labelWelcome.Text = welcomeHeading;
            labelDescription.Text = welcomeDescription;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.Welcome; }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }
    }
}