﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class ScenarioPage : BaseUserControl, ISettingPage
    {
        private const short InvalidItemIndex = -1;                  // Index of combo when none selected.
        private const short FirstItemIndex = 0;                     // Index of the 1st item of the combo.
        private const short ItemDiffernce = 1;                      // Represents items different.

        private List<IScenario> _scenarioList; // = new List<IScenario>();
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
        public IUnitOfWork UnitOfWork { get; private set; }
        public IScenarioRepository Repository { get; private set; }

        public int LastItemIndex
        {
            get { return comboBoxAdvScenarioCollection.Items.Count - ItemDiffernce; }
        }

        public IScenario SelectedScenario
        {
            get { return (IScenario)comboBoxAdvScenarioCollection.SelectedItem; }
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();

            toolTip1.SetToolTip(buttonDelete, Resources.DeleteScenario);
            toolTip1.SetToolTip(buttonNew, Resources.NewScenario);
            autoLabelMax5Scenarios.Text = Resources.Maximum5Scenarios;
        }

        public void InitializeDialogControl()
        {
            setColors();
            SetTexts();
        }

        public void LoadControl()
        {
            loadScenarios();
        }

        public void SaveChanges()
        {}

        public void Unload()
        {
            // Disposes or flag anything possible.
            _scenarioList = null;
        }

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.Scheduling);
        }

        public string TreeNode()
        {
            return Resources.Scenario;
        }

    	public void OnShow()
    	{
    	}

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;

            // Creates a new repository.
            Repository = new ScenarioRepository(UnitOfWork);
        }

        public void Persist()
        {
            SaveChanges();
        }

        public ScenarioPage()
        {
            InitializeComponent();

            // Binds events.
            comboBoxAdvScenarioCollection.SelectedIndexChanging += ComboBoxAdvScenarioCollectionSelectedIndexChanging;
            comboBoxAdvScenarioCollection.SelectedIndexChanged += ComboBoxAdvScenarioCollectionSelectedIndexChanged;
            textBoxExtDescription.Validating += TextBoxDescriptionValidating;
            textBoxExtDescription.Validated += TextBoxDescriptionValidated;
            checkBoxAdvDefaultScenario.Validated += CheckBoxDefaultScenarioValidated;
            checkBoxAdvAuditTrail.Validated += CheckBoxAuditTrailValidated;
            checkBoxAdvEnableReporting.Validated += CheckBoxAdvEnableReportingValidated;
			checkBoxAdvRestricted.Validated += CheckBoxAdvRestrictedValidated;
            buttonNew.Click += ButtonNewClick;
            buttonDelete.Click += ButtonDeleteClick;
        }

        private void addNewScenario()
        {
            _scenarioList.Add(createScenario());
            loadScenarios();
            comboBoxAdvScenarioCollection.SelectedIndex = LastItemIndex;
        }
        private void changedInfo()
        {
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
            string changed = _localizer.UpdatedByText(SelectedScenario, Resources.UpdatedByColon);
            string created = _localizer.CreatedText(SelectedScenario, Resources.CreatedByColon);
            autoLabelInfoAboutChanges.Text = string.Concat(created, changed);
        }

		
		private void deleteScenario()
        {
            bool deleteable = !(SelectedScenario == null || SelectedScenario.DefaultScenario);
            // Checks and delete if not default.
            if (!deleteable)
            {
                string message = string.Format(
                    CurrentCulture,
                    Resources.CannotDeleteDefaultScenario,
                    SelectedScenario.Description
                    );
				MessageDialogs.ShowError(this, message,labelHeader.Text);
               
            }
            else
            {
                Repository.Remove(SelectedScenario);
                // Removes from list.
                _scenarioList.Remove(SelectedScenario);

                loadScenarios();
            }
        }

        private void changeDefaultScenario()
        {
            // Default scenario cannot be unchecked.
            if (SelectedScenario.DefaultScenario)
            {
                checkBoxAdvDefaultScenario.Checked = true;
            }
            else if (checkBoxAdvDefaultScenario.Checked)
            {
                // Removes default scenario.
                IScenario defaultScenario = _scenarioList.Where(s => s.DefaultScenario).SingleOrDefault();
                if (defaultScenario != null)
                {
                    defaultScenario.DefaultScenario = false;
                }
                SelectedScenario.DefaultScenario = true;
            }
        }

        private void changeAuditTrail()
        {
            SelectedScenario.AuditTrail = checkBoxAdvAuditTrail.Checked;
        }

        private void changeEnableReporting()
        {
            SelectedScenario.EnableReporting = checkBoxAdvEnableReporting.Checked;
        }

		private void ChangeRestricted()
		{
			SelectedScenario.Restricted = checkBoxAdvRestricted.Checked;
		}

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
        }

        private bool validateScenarioDescription()
        {
            bool failed = string.IsNullOrEmpty(textBoxExtDescription.Text);
            if (failed)
            {
                textBoxExtDescription.SelectedText = SelectedScenario.Description.Name;
            }

            return !failed;
        }

        private void changeScenarioDescription()
        {
            SelectedScenario.Description = new Description(textBoxExtDescription.Text);

            loadScenarios();
        }

        private void selectScenario()
        {
        	if (SelectedScenario == null) return;
        	textBoxExtDescription.Text = SelectedScenario.Description.Name;
        	checkBoxAdvDefaultScenario.Checked = SelectedScenario.DefaultScenario;
        	checkBoxAdvAuditTrail.Checked = SelectedScenario.AuditTrail;
        	checkBoxAdvEnableReporting.Checked = SelectedScenario.EnableReporting;
        	checkBoxAdvRestricted.Checked = SelectedScenario.Restricted;
        	setEnableReportingState();
        }

        private void setEnableReportingState()
        {
            if (SelectedScenario.DefaultScenario)
            {
                // Default scenario should always be enabled for reporting
                checkBoxAdvEnableReporting.Enabled = false;
            }
            else
            {
                checkBoxAdvEnableReporting.Enabled = SelectedScenario.EnableReporting || freeSlotExistsForEnableReporting();
            }            
        }

        private bool freeSlotExistsForEnableReporting()
        {
            int counter = 0;
            foreach (IScenario scenario in _scenarioList)
            {
                if (scenario.EnableReporting) counter++;
                if (counter == 5) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specific index is withing a valid range.
        /// </summary>
        /// <param name="index">An item index.</param>
        /// <returns>The response.</returns>
        /// <value><c>True</c> if index is withing a valid range; otherwise, <c>False</c>.</value>
        private bool isWithinRange(int index)
        {
            return index > InvalidItemIndex && index < _scenarioList.Count && comboBoxAdvScenarioCollection.DataSource != null;
        }

        /// <summary>
        /// Create a new instance of <see cref="IScenario" /> class with default values.
        /// </summary>
        /// <returns>A new instance.</returns>
        private IScenario createScenario()
        {
            // Formats the name.
            Description description = PageHelper.CreateNewName(_scenarioList, "Description.Name", Resources.NewScenario);

            IScenario newScenario = new Scenario(description.Name) { Description = description };
            Repository.Add(newScenario);

            return newScenario;
        }

        /// <summary>
        /// Loads scenarios and bings to combo. Creates new if no scenarios found.
        /// </summary>
        private void loadScenarios()
        {
        	if (Disposing) return;
        	if (_scenarioList == null)
        	{
        		_scenarioList = new List<IScenario>();
        		_scenarioList.AddRange(Repository.FindAllSorted());
        	}
        	if (_scenarioList.IsEmpty()) _scenarioList.Add(createScenario());

        	int selected = comboBoxAdvScenarioCollection.SelectedIndex;
        	if (!isWithinRange(selected)) selected = FirstItemIndex;

        	// Rebinds list to comboBoxAdvScenarioCollection.
        	comboBoxAdvScenarioCollection.DataSource = null;
        	comboBoxAdvScenarioCollection.DisplayMember = "Description";
        	comboBoxAdvScenarioCollection.DataSource = _scenarioList;

        	comboBoxAdvScenarioCollection.SelectedIndex = selected;
        }

        private void CheckBoxAuditTrailValidated(object sender, EventArgs e)
        {
            if (SelectedScenario != null)
            {
                changeAuditTrail();
            }
        }

        private void CheckBoxAdvEnableReportingValidated(object sender, EventArgs e)
        {
            if (SelectedScenario != null)
            {
                changeEnableReporting();
            }
        }

		private void CheckBoxAdvRestrictedValidated(object sender, EventArgs e)
		{
			if(SelectedScenario != null)
			{
				ChangeRestricted();
			}
		}

        private void CheckBoxDefaultScenarioValidated(object sender, EventArgs e)
        {
        	if (SelectedScenario == null) return;
        	Cursor.Current = Cursors.WaitCursor;
        	changeDefaultScenario();
        	Cursor.Current = Cursors.Default;
        }

        private void ComboBoxAdvScenarioCollectionSelectedIndexChanged(object sender, EventArgs e)
        {
        	if (SelectedScenario == null) return;
        	Cursor.Current = Cursors.WaitCursor;
        	selectScenario();
        	changedInfo();
        	Cursor.Current = Cursors.Default;
        }

        private void ComboBoxAdvScenarioCollectionSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
        {
            e.Cancel = !isWithinRange(e.NewIndex);
        }

        private void TextBoxDescriptionValidating(object sender, CancelEventArgs e)
        {
            if (SelectedScenario != null)
            {
                e.Cancel = !validateScenarioDescription();
            }
        }

        private void TextBoxDescriptionValidated(object sender, EventArgs e)
        {
            if (SelectedScenario != null)
            {
                changeScenarioDescription();
            }
        }

        private void ButtonNewClick(object sender, EventArgs e)
        {
        	if (SelectedScenario == null) return;
        	Cursor.Current = Cursors.WaitCursor;
        	addNewScenario();
        	Cursor.Current = Cursors.Default;
        }

        private void ButtonDeleteClick(object sender, EventArgs e)
        {
        	if (SelectedScenario == null) return;
        	string text = string.Format(
        		CurrentCulture,
        		Resources.AreYouSureYouWantToDeleteScenario,
        		SelectedScenario.Description
        		);
        	string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

        	DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
        	if (response == DialogResult.Yes)
        	{
        		Cursor.Current = Cursors.WaitCursor;
        		deleteScenario();
        		Cursor.Current = Cursors.Default;
        	}
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }

        public ViewType ViewType
        {
            get { return ViewType.Scenario; }
        }
    }
}