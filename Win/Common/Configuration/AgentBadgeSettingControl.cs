using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class AgentBadgeSettingControl : BaseUserControl, ISettingPage
	{
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
	    private const short itemDiffernce = 1;                      
		private List<IDifferentialAgentBadgeSettings> _agentBadgeSettingList;
		private readonly IDictionary<AgentBadgeSettingRuleSet, string> _agentBadgeSettingRuleSetList = new Dictionary<AgentBadgeSettingRuleSet, string>();
		private readonly IToggleManager _toggleManager;

		public IUnitOfWork UnitOfWork { get; private set; }

		public IDifferentialAgentBadgeSettingRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvAgentBadgeSettings.Items.Count - itemDiffernce; }
		}

		public IDifferentialAgentBadgeSettings SelectedBadgeSetting
		{
			get { return (IDifferentialAgentBadgeSettings)comboBoxAdvAgentBadgeSettings.SelectedItem; }
		}

		public void InitializeDialogControl()
		{
			if (!_toggleManager.IsEnabled(Toggles.Portal_ResetBadges_30544))
			{
				reset.Hide();
			}
			
			setColors();
			SetTexts();
		}

		private void changedInfo()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			string changed = _localizer.UpdatedByText(SelectedBadgeSetting, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}
		
		public void Unload()
		{
			// Disposes or flag anything possible.
			_agentBadgeSettingList = null;
		}

		public void LoadControl()
		{
			setUpTimeSpanBoxes();
			loadBadgeSettingRuleSets();
			loadBadgeSettings();
			
		}
		
		private void setUpTimeSpanBoxes()
		{
			timeSpanTextBoxThresholdForAHT.SetSize(115, 33);
			timeSpanTextBoxBronzeThresholdForAHT.SetSize(115, 33);
			timeSpanTextBoxSilverThresholdForAHT.SetSize(115, 33);
			timeSpanTextBoxGoldThresholdForAHT.SetSize(115, 33);
		}

		public void  SaveChanges()
		{}

		public AgentBadgeSettingControl(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
			
			InitializeComponent();

			comboBoxAdvAgentBadgeSettings.SelectedIndexChanging += comboBoxAdvBadgeSettingsSelectedIndexChanging;
			comboBoxAdvAgentBadgeSettings.SelectedIndexChanged += comboBoxAdvBadgeSettingsSelectedIndexChanged;
			comboBoxAdvBadgeSettingRuleSets.SelectedIndexChanged += comboBoxAdvAgentBadgeSettingRuleSetSelectedIndexChanged;
			textBoxDescription.Validating += textBoxDescriptionValidating;
			textBoxDescription.Validated += textBoxDescriptionValidated;

			buttonNew.Click += buttonNewClick;
			buttonDeleteBadgeSetting.Click += buttonDeleteBadgeSettingClick;

		}


		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			changeBadgeSettingDescription();
		}

		private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedBadgeSetting != null)
			{
				validateBadgeSettingDescription();
			}
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			//addNewBadgeSetting();
			if (SelectedBadgeSetting == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewBadgeSetting();
			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteBadgeSettingClick(object sender, EventArgs e)
		{
			if (SelectedBadgeSetting == null) return;
			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteBadgeSetting,
				SelectedBadgeSetting.Description
				);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;
			deleteBadgeSetting();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvBadgeSettingsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedBadgeSetting == null) return;
			Cursor.Current = Cursors.WaitCursor;
			selectBadgeSetting();
			changedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvBadgeSettingsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			Repository = new DifferentialAgentBadgeSettingsRepository(UnitOfWork);
		}

		public void Persist()
		{
			SaveChanges();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Gamification);
		}

		public string TreeNode()
		{
			return Resources.Settings;
		}

		public void OnShow()
		{
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDeleteBadgeSetting, Resources.DeleteAgentBadgeSetting);
			toolTip1.SetToolTip(buttonNew, Resources.NewAgentBadgeSetting);
		}

		private void changeBadgeSettingDescription()
		{
			SelectedBadgeSetting.Description = new Description(textBoxDescription.Text);
			loadBadgeSettings();
		}

		private void validateBadgeSettingDescription()
		{
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedBadgeSetting.Description.Name;
			}
		}

		private void deleteBadgeSetting()
		{
			Repository.Remove(SelectedBadgeSetting);
			_agentBadgeSettingList.Remove(SelectedBadgeSetting);
			loadBadgeSettings();
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
		
		private void loadBadgeSettingRuleSets()
		{
			if (_agentBadgeSettingRuleSetList.Count > 0) return;
			_agentBadgeSettingRuleSetList.Add(AgentBadgeSettingRuleSet.RuleWithRatioConvertor, Resources.RuleWithRatioConvertor);
			if (_toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185))
			{
				_agentBadgeSettingRuleSetList.Add(AgentBadgeSettingRuleSet.RuleWithDifferentThreshold, Resources.RuleWithDifferentThreshold);
			}
			
			comboBoxAdvBadgeSettingRuleSets.DataSource = new BindingSource(_agentBadgeSettingRuleSetList, null);
			comboBoxAdvBadgeSettingRuleSets.DisplayMember = "Value";
			comboBoxAdvBadgeSettingRuleSets.ValueMember = "Key";
		}

		private void selectBadgeSetting()
		{
			if (SelectedBadgeSetting == null) return;
			textBoxDescription.Text = SelectedBadgeSetting.Description.ToString();
			comboBoxAdvBadgeSettingRuleSets.SelectedValue =  SelectedBadgeSetting.BadgeSettingRuleSet;

			
		}

		private void addNewBadgeSetting()
		{
			var newBadgeSetting = createBadgeSetting();
			_agentBadgeSettingList.Add(newBadgeSetting);

			loadBadgeSettings();
			comboBoxAdvAgentBadgeSettings.SelectedIndex = LastItemIndex;
		}

		private IDifferentialAgentBadgeSettings createBadgeSetting()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_agentBadgeSettingList, "Description.Name", Resources.NewBadgeSetting);
			IDifferentialAgentBadgeSettings newBadgeSetting = new DifferentialAgentBadgeSettings(description.Name) { Description = description };
			Repository.Add(newBadgeSetting);

			return newBadgeSetting;
		}

		private void loadBadgeSettings()
		{
			if (Disposing) return;
			if (_agentBadgeSettingList == null)
			{
				_agentBadgeSettingList = new List<IDifferentialAgentBadgeSettings>();
				//_agentBadgeSettingList.AddRange(Repository.FindAllBadgeSettingsByDescription());
			}

			if (_agentBadgeSettingList.IsEmpty())
			{
				_agentBadgeSettingList.Add(createBadgeSetting());
			}

			int selected = comboBoxAdvAgentBadgeSettings.SelectedIndex;
			if (!isWithinRange(selected))
			{
				selected = firstItemIndex;
			}

			// Rebinds list to comboBox.
			comboBoxAdvAgentBadgeSettings.DataSource = null;
			comboBoxAdvAgentBadgeSettings.DataSource = _agentBadgeSettingList;
			comboBoxAdvAgentBadgeSettings.DisplayMember = "Description";

			comboBoxAdvAgentBadgeSettings.SelectedIndex = selected;
		}

		private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _agentBadgeSettingList.Count && comboBoxAdvAgentBadgeSettings.DataSource != null;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.Gamification; }
		}

		private void comboBoxAdvAgentBadgeSettingRuleSetSelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdvBadgeSettingRuleSets.SelectedItem == null) return;
			var kvp = ((KeyValuePair<AgentBadgeSettingRuleSet, string>)((ComboBoxAdv)sender).SelectedItem);

			var ruleWithRatioConvertorSelected = kvp.Key == AgentBadgeSettingRuleSet.RuleWithRatioConvertor;

			labelSetThresholdForAnsweredCalls.Visible = ruleWithRatioConvertorSelected;
			numericUpDownThresholdForAnsweredCalls.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(numericUpDownThresholdForAnsweredCalls, ruleWithRatioConvertorSelected);

			labelSetBadgeThresholdForAHT.Visible = ruleWithRatioConvertorSelected;
			timeSpanTextBoxThresholdForAHT.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(timeSpanTextBoxThresholdForAHT, ruleWithRatioConvertorSelected);

			labelSetBadgeThresholdForAdherence.Visible = ruleWithRatioConvertorSelected;
			doubleTextBoxThresholdForAdherence.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(doubleTextBoxThresholdForAdherence, ruleWithRatioConvertorSelected);

			labelRatioSplitter.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(labelRatioSplitter, ruleWithRatioConvertorSelected, 10);
			labelOneSilverBadgeEqualsBronzeBadgeCount.Visible = ruleWithRatioConvertorSelected;
			numericUpDownSilverToBronzeBadgeRate.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(numericUpDownSilverToBronzeBadgeRate, ruleWithRatioConvertorSelected);
			labelOneGoldBadgeEqualsSilverBadgeCount.Visible = ruleWithRatioConvertorSelected;
			numericUpDownGoldToSilverBadgeRate.Visible = ruleWithRatioConvertorSelected;
			toggleRowDisplay(numericUpDownGoldToSilverBadgeRate, ruleWithRatioConvertorSelected);

			labelSetBronzeThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			labelSetSilverThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			labelSetGoldThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			numericUpDownBronzeThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			numericUpDownSilverThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			numericUpDownGoldThresholdForAnsweredCalls.Visible = !ruleWithRatioConvertorSelected;
			toggleRowDisplay(numericUpDownBronzeThresholdForAnsweredCalls, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(numericUpDownSilverThresholdForAnsweredCalls, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(numericUpDownGoldThresholdForAnsweredCalls, !ruleWithRatioConvertorSelected);

			labelSetBronzeBadgeThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			labelSetSilverBadgeThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			labelSetGoldBadgeThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			timeSpanTextBoxBronzeThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			timeSpanTextBoxSilverThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			timeSpanTextBoxGoldThresholdForAHT.Visible = !ruleWithRatioConvertorSelected;
			toggleRowDisplay(timeSpanTextBoxBronzeThresholdForAHT, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(timeSpanTextBoxSilverThresholdForAHT, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(timeSpanTextBoxGoldThresholdForAHT, !ruleWithRatioConvertorSelected);

			labelSetBadgeBronzeThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			labelSetBadgeSilverThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			labelSetBadgeGoldThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			doubleTextBoxBronzeThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			doubleTextBoxSilverThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			doubleTextBoxGoldThresholdForAdherence.Visible = !ruleWithRatioConvertorSelected;
			toggleRowDisplay(doubleTextBoxBronzeThresholdForAdherence, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(doubleTextBoxSilverThresholdForAdherence, !ruleWithRatioConvertorSelected);
			toggleRowDisplay(doubleTextBoxGoldThresholdForAdherence, !ruleWithRatioConvertorSelected);

		}

		private void toggleRowDisplay(Control control, bool display, int height = 35)
		{
			var rowIndex = tableLayoutPanel6.GetRow(control);
			if (rowIndex > 0)
			{
				tableLayoutPanel6.RowStyles[rowIndex].Height = display ? height : 0;
			}
		}

		private void checkBoxUseBadgeForAnsweredCalls_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownBronzeThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownSilverThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			numericUpDownGoldThresholdForAnsweredCalls.Enabled = ((CheckBox)sender).Checked;
			updateRatioSettingsState();
		}

		private void checkBoxUseBadgeForAHT_CheckedChanged(object sender, EventArgs e)
		{
			timeSpanTextBoxThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxBronzeThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxSilverThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			timeSpanTextBoxGoldThresholdForAHT.Enabled = ((CheckBox)sender).Checked;
			updateRatioSettingsState();
		}

		private void checkBoxUseBadgeForAdherence_CheckedChanged(object sender, EventArgs e)
		{
			doubleTextBoxThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxBronzeThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxSilverThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			doubleTextBoxGoldThresholdForAdherence.Enabled = ((CheckBox)sender).Checked;
			updateRatioSettingsState();
		}

		private void updateRatioSettingsState()
		{
			var isAnyTypeEnabled = (doubleTextBoxThresholdForAdherence.Enabled || timeSpanTextBoxThresholdForAHT.Enabled ||
			                    numericUpDownThresholdForAnsweredCalls.Enabled);
			numericUpDownGoldToSilverBadgeRate.Enabled = isAnyTypeEnabled;
			numericUpDownSilverToBronzeBadgeRate.Enabled = isAnyTypeEnabled;
		}
	}
}
