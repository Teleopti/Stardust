using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ScenarioPage : BaseUserControl, ISettingPage
	{
		private const short invalidItemIndex = -1;                  // Index of combo when none selected.
		private const short firstItemIndex = 0;                     // Index of the 1st item of the combo.
		private const short itemDiffernce = 1;                      // Represents items different.

		private List<IScenario> _scenarioList; // = new List<IScenario>();
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		public IUnitOfWork UnitOfWork { get; private set; }
		public IScenarioRepository Repository { get; private set; }

		public int LastItemIndex
		{
			get { return comboBoxAdvScenarioCollection.Items.Count - itemDiffernce; }
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
			Repository = ScenarioRepository.DONT_USE_CTOR(UnitOfWork);
		}

		public void Persist()
		{
			SaveChanges();
		}

		public ScenarioPage()
		{
			InitializeComponent();

			// Binds events.
			comboBoxAdvScenarioCollection.SelectedIndexChanging += comboBoxAdvScenarioCollectionSelectedIndexChanging;
			comboBoxAdvScenarioCollection.SelectedIndexChanged += comboBoxAdvScenarioCollectionSelectedIndexChanged;
			textBoxExtDescription.Validating += textBoxDescriptionValidating;
			textBoxExtDescription.Validated += textBoxDescriptionValidated;
			//checkBoxAdvDefaultScenario.Validated += CheckBoxDefaultScenarioValidated;
			//checkBoxAdvAuditTrail.Validated += CheckBoxAuditTrailValidated;
			checkBoxAdvEnableReporting.Validated += checkBoxAdvEnableReportingValidated;
			checkBoxAdvRestricted.Validated += checkBoxAdvRestrictedValidated;
			buttonNew.Click += buttonNewClick;
			buttonDelete.Click += buttonDeleteClick;
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
			autoLabelInfoAboutChanges.Text = changed;
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

		//private void changeDefaultScenario()
		//{
		//    // Default scenario cannot be unchecked.
		//    if (SelectedScenario.DefaultScenario)
		//    {
		//        checkBoxAdvDefaultScenario.Checked = true;
		//    }
		//    else if (checkBoxAdvDefaultScenario.Checked)
		//    {
		//        // Removes default scenario.
		//        IScenario defaultScenario = _scenarioList.Where(s => s.DefaultScenario).SingleOrDefault();
		//        if (defaultScenario != null)
		//        {
		//            defaultScenario.DefaultScenario = false;
		//        }
		//        SelectedScenario.DefaultScenario = true;
		//    }
		//}

		//private void changeAuditTrail()
		//{
		//    SelectedScenario.AuditTrail = checkBoxAdvAuditTrail.Checked;
		//}

		private void changeEnableReporting()
		{
			SelectedScenario.EnableReporting = checkBoxAdvEnableReporting.Checked;
		}

		private void changeRestricted()
		{
			SelectedScenario.Restricted = checkBoxAdvRestricted.Checked;
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
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
			bool failed = string.IsNullOrWhiteSpace(textBoxExtDescription.Text);
			if (failed)
			{
				textBoxExtDescription.SelectedText = SelectedScenario.Description.Name;
			}

			return !failed;
		}

		private void changeScenarioDescription()
		{
			SelectedScenario.ChangeName(textBoxExtDescription.Text);

			loadScenarios();
		}

		private void selectScenario()
		{
			if (SelectedScenario == null) return;
			textBoxExtDescription.Text = SelectedScenario.Description.Name;
			//checkBoxAdvDefaultScenario.Checked = SelectedScenario.DefaultScenario;
			autoLabelDefaultScenario.Visible = SelectedScenario.DefaultScenario;
			//checkBoxAdvAuditTrail.Checked = SelectedScenario.AuditTrail;
			checkBoxAdvEnableReporting.Checked = SelectedScenario.EnableReporting;
			checkBoxAdvRestricted.Checked = SelectedScenario.Restricted;
			setEnableReportingState();
			buttonDelete.Enabled = !SelectedScenario.DefaultScenario;
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
			return index > invalidItemIndex && index < _scenarioList.Count && comboBoxAdvScenarioCollection.DataSource != null;
		}

		/// <summary>
		/// Create a new instance of <see cref="IScenario" /> class with default values.
		/// </summary>
		/// <returns>A new instance.</returns>
		private IScenario createScenario()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_scenarioList, "Description.Name", Resources.NewScenario);

			IScenario newScenario = new Scenario(description.Name);
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
			if (!isWithinRange(selected)) selected = firstItemIndex;

			// Rebinds list to comboBoxAdvScenarioCollection.
			comboBoxAdvScenarioCollection.DataSource = null;
			comboBoxAdvScenarioCollection.DisplayMember = "Description";
			comboBoxAdvScenarioCollection.DataSource = _scenarioList;

			comboBoxAdvScenarioCollection.SelectedIndex = selected;
		}

		//private void CheckBoxAuditTrailValidated(object sender, EventArgs e)
		//{
		//    if (SelectedScenario != null)
		//    {
		//        changeAuditTrail();
		//    }
		//}

		private void checkBoxAdvEnableReportingValidated(object sender, EventArgs e)
		{
			if (SelectedScenario != null)
			{
				changeEnableReporting();
			}
		}

		private void checkBoxAdvRestrictedValidated(object sender, EventArgs e)
		{
			if(SelectedScenario != null)
			{
				changeRestricted();
			}
		}

		//private void CheckBoxDefaultScenarioValidated(object sender, EventArgs e)
		//{
		//    if (SelectedScenario == null) return;
		//    Cursor.Current = Cursors.WaitCursor;
		//    changeDefaultScenario();
		//    Cursor.Current = Cursors.Default;
		//}

		private void comboBoxAdvScenarioCollectionSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedScenario == null) return;
			Cursor.Current = Cursors.WaitCursor;
			selectScenario();
			changedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvScenarioCollectionSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedScenario != null)
			{
				e.Cancel = !validateScenarioDescription();
			}
		}

		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			if (SelectedScenario != null)
			{
				changeScenarioDescription();
			}
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			if (SelectedScenario == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewScenario();
			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteClick(object sender, EventArgs e)
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