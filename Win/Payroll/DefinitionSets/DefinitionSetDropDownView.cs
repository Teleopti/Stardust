using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public partial class DefinitionSetDropDownView : PayrollBaseUserControl
    {
       private IDefinitionSetViewModel SelectedDefinitionSet
        {
            get
            {
                return comboBoxAdvMultiplicatorDefinitionSets.SelectedItem as IDefinitionSetViewModel;
            }
        }

        public DefinitionSetDropDownView(IExplorerView explorerView)
            : base(explorerView)
        {
            InitializeComponent();
            SetTexts();
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
            LoadDefinitionSets();

        }

        /// <summary>
        /// Loads the definition sets.
        /// </summary>
        public void LoadDefinitionSets()
        {
            if (ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection != null &&
                ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection.Count > 0)
            {
                comboBoxAdvMultiplicatorDefinitionSets.DataSource = null;

                comboBoxAdvMultiplicatorDefinitionSets.DataSource =
                    ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection;

                comboBoxAdvMultiplicatorDefinitionSets.ValueMember = "Name";
                comboBoxAdvMultiplicatorDefinitionSets.DisplayMember = "Name";

                if (comboBoxAdvMultiplicatorDefinitionSets.SelectedIndex < 0)
                {
                    comboBoxAdvMultiplicatorDefinitionSets.SelectedIndex = 0;
                    loadControls();
                }
            }
            else
            {
                comboBoxAdvMultiplicatorDefinitionSets.DataSource = null;
                textBoxDefinitionSetName.Text = string.Empty;
                textBoxMultiplicatorType.Text = string.Empty;

                if (ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection != null)
                    ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection.Clear();
                ExplorerView.RefreshSelectedViews();
            }
        }

        /// <summary>
        /// Adds the new.
        /// </summary>
        public override void AddNew()
        {
            using (var editForm = new ManageDefinitionSetForm(ExplorerView.ExplorerPresenter.DefinitionSetPresenter))
            {
                editForm.DefinitionSetAdded += editFormAfterDefinitionSetAdded;
                editForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public override void DeleteSelected()
        {
            if (SelectedDefinitionSet != null)
            {
                string text = string.Format(CurrentCulture,
                                            Resources.AreYouSureYouWantToDeleteDefinitionSet,
                                            SelectedDefinitionSet.Name
                                            );

                if (showMyErrorMessage(text, "Message") == DialogResult.Yes)
                {
                    ExplorerView.ExplorerPresenter.DefinitionSetPresenter.RemoveDefinitionSet(SelectedDefinitionSet.DomainEntity);
                    LoadDefinitionSets();
                }
            }
        }

        /// <summary>
        /// Shows my error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        private static DialogResult showMyErrorMessage(string message, string caption)
        {
            return ViewBase.ShowYesNoMessage(message, caption);
        }


        /// <summary>
        /// Loads the controls.
        /// </summary>
        private void loadControls()
        {
            if(SelectedDefinitionSet != null)
            {
                //Load the controls under the current user control
                textBoxDefinitionSetName.Text = SelectedDefinitionSet.DomainEntity.Name;
                textBoxMultiplicatorType.Text =
                    LanguageResourceHelper.TranslateEnumValue(SelectedDefinitionSet.MultiplicatorType);
                //comboBoxAdvMulitiplicatorType.SelectedItem = SelectedDefinitionSet.DomainEntity.MultiplicatorType;

                
                autoLabelInfoAboutChanges.Text = SelectedDefinitionSet.ChangeInfo;

                //Load the rest of the user controls

                //Previous implementation was to use a set of definition sets at once. 
                //It was changed later to have only one at a time. But still it is better to keep the previous implementation unchanged incase 
                //of future alteration. 
                IList<IMultiplicatorDefinitionSet> filteredSet = new List<IMultiplicatorDefinitionSet> { SelectedDefinitionSet.DomainEntity };

                ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection = filteredSet;
                ExplorerView.RefreshSelectedViews();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the comboBoxAdvMultiplicatorDefinitionSets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void comboBoxAdvMultiplicatorDefinitionSetsSelectedIndexChanged(object sender, EventArgs e)
        {
            loadControls();
        }

        /// <summary>
        /// Handles the Leave event of the textBoxDefinitionSetName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void textBoxDefinitionSetNameLeave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxDefinitionSetName.Text) && SelectedDefinitionSet != null)
            {
                SelectedDefinitionSet.DomainEntity.Name = textBoxDefinitionSetName.Text;
                LoadDefinitionSets();
            }
        }

        /// <summary>
        /// Handles the DefinitionSetAddedEventArgs event of the EditForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void editFormAfterDefinitionSetAdded(object sender, DefinitionSetAddedEventArgs e)
        {
            LoadDefinitionSets();
            comboBoxAdvMultiplicatorDefinitionSets.SelectedIndex =
                (ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection.Count - 1);
        }

        public override string ToolTipDelete
        {
            get { return Resources.DeleteDefinitionSet; }
        }

        public override string ToolTipAddNew
        {
            get { return Resources.AddNewDefinitionSet; }
        }
    }
}
