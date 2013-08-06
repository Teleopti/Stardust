using System;
using System.Globalization;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Settings
{
    public partial class ChangeCultureControl : BaseUserControl, IDialogControl
    {
        public void InitializeDialogControl()
        {
            InitializeComboBox();

            PersonDto personDto = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            autoLabelUserName.Text = autoLabelUserName.Text + " " + personDto.Name;

            comboBoxAdvCurrentCulture.SelectedItem = CultureInfo.CurrentCulture;
            comboBoxAdvLanguage.SelectedItem = CultureInfo.CurrentUICulture;
        }

        public void LoadControl()
        {
        }

        public string TreeFamily()
        {
            return UserTexts.Resources.AgentSettings;
        }

        public string TreeNode()
        {
            return UserTexts.Resources.CultureSettings;
        }

        public bool SaveChanges()
        {
            PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            var culture = ((CultureInfo) comboBoxAdvCurrentCulture.SelectedItem).LCID;
            var uiCulture = ((CultureInfo) comboBoxAdvLanguage.SelectedItem).LCID;
            if (culture!=person.CultureLanguageId || uiCulture!=person.UICultureLanguageId)
            {
                person.CultureLanguageId = culture;
                person.UICultureLanguageId = uiCulture;
                SdkServiceHelper.OrganizationService.SavePerson(person);
            }
            return true;
        }

        public ChangeCultureControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        private void comboBoxAdvCurrentCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBoxAdvCurrentCulture.SelectedItem.Equals(comboBoxAdvLanguage.SelectedItem))
                comboBoxAdvLanguage.SelectedItem = comboBoxAdvCurrentCulture.SelectedItem;
        }

        private void InitializeComboBox()
        {
            comboBoxAdvCurrentCulture.DataSource = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            comboBoxAdvCurrentCulture.DisplayMember = "DisplayName";
            comboBoxAdvCurrentCulture.ValueMember = "LCID";
            comboBoxAdvLanguage.DataSource = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            comboBoxAdvLanguage.DisplayMember = "DisplayName";
            comboBoxAdvLanguage.ValueMember = "LCID";
        }
    }
}
