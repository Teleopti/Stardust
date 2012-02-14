using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortal.Settings
{
    /// <summary>
    /// Represnt the control class that proveds functionality to chnage logged users
    /// culture and language
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-08-18
    /// </remarks>
    public partial class ChangeCultureControl : BaseUserControl, IDialogControl
    {
        /// <summary>
        /// Manually initialze control components. Calls when OptionDialog contructor.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void InitializeDialogControl()
        {
            InitializeComboBox();

            PersonDto personDto = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            autoLabelUserName.Text = autoLabelUserName.Text + " " + personDto.Name;

            comboBoxAdvCurrentCulture.SelectedItem = CultureInfo.CurrentCulture;
            comboBoxAdvLanguage.SelectedItem = CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Manually load control details. Calls when OptionDialog loads.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void LoadControl()
        {
        }

        /// <summary>
        /// The name of the Parent if represented in a TreeView
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public string TreeFamily()
        {
            return UserTexts.Resources.AgentSettings;
        }

        /// <summary>
        /// The name of the Node if represented in a TreeView
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public string TreeNode()
        {
            return UserTexts.Resources.CultureSettings;
        }

        /// <summary>
        /// Persist all and save changes by the control. Calls when OkDialog hits.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
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

        /// <summary>
        /// Persist all and delete data by the control. Calls when DeleteDialog hits.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public void Delete()
        {
        }

        public ChangeCultureControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the comboBoxAdvCurrentCulture control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/2/2008
        /// </remarks>
        private void comboBoxAdvCurrentCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBoxAdvCurrentCulture.SelectedItem.Equals(comboBoxAdvLanguage.SelectedItem))
                comboBoxAdvLanguage.SelectedItem = comboBoxAdvCurrentCulture.SelectedItem;
        }

        /// <summary>
        /// Initializes the combo box.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-04-09
        /// </remarks>
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
