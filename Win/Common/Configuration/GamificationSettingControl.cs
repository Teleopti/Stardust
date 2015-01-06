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
	public partial class GamificationSettingControl : BaseUserControl, ISettingPage
	{
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
	    private const short itemDiffernce = 1;                      
		private List<IGamificationSetting> _gamificationSettingList;
		private readonly IDictionary<GamificationSettingRuleSet, string> _gamificationSettingRuleSetList = new Dictionary<GamificationSettingRuleSet, string>();
		private readonly IToggleManager _toggleManager;
		private IAgentBadgeTransactionRepository _agentBadgeTransactionRepository;
		private IAgentBadgeWithRankTransactionRepository _agentBadgeWithRankTransactionRepository;

		public IUnitOfWork UnitOfWork { get; private set; }

		public IGamificationSettingRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvGamificationSettings.Items.Count - itemDiffernce; }
		}

		public IGamificationSetting SelectedGamificationSetting
		{
			get { return (IGamificationSetting)comboBoxAdvGamificationSettings.SelectedItem; }
		}

		public GamificationSettingControl(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;

			InitializeComponent();

			comboBoxAdvGamificationSettings.SelectedIndexChanging += comboBoxAdvGamificationSettingSelectedIndexChanging;
			comboBoxAdvGamificationSettings.SelectedIndexChanged += comboBoxAdvGamificationSettingSelectedIndexChanged;
			comboBoxAdvBadgeSettingRuleSets.SelectedIndexChanged += comboBoxAdvGamificationSettingRuleSetSelectedIndexChanged;
			textBoxDescription.Validating += textBoxDescriptionValidating;
			textBoxDescription.Validated += textBoxDescriptionValidated;

			buttonNew.Click += buttonNewClick;
			buttonDeleteGamificationSetting.Click += buttonDeleteGamificationSettingClick;
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
			string changed = _localizer.UpdatedByText(SelectedGamificationSetting, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}
		
		public void Unload()
		{
			// Disposes or flag anything possible.
			_gamificationSettingList = null;
		}

		public void LoadControl()
		{
			setUpTimeSpanBoxes();
			loadGamificationSettingRuleSets();
			loadGamificationSettings();
			
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

		

		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			changeGamificationSettingDescription();
		}

		private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedGamificationSetting != null)
			{
				validateGamificationSettingDescription();
			}
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			//addNewBadgeSetting();
			if (SelectedGamificationSetting == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewGamificationSetting();
			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteGamificationSettingClick(object sender, EventArgs e)
		{
			if (SelectedGamificationSetting == null) return;
			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteGamificationSetting,
				SelectedGamificationSetting.Description
				);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;
			deleteGamificationSetting();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvGamificationSettingSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedGamificationSetting == null) return;
			Cursor.Current = Cursors.WaitCursor;
			selectGamificationSetting();
			changedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvGamificationSettingSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			Repository = new GamificationSettingRepository(UnitOfWork);
			_agentBadgeTransactionRepository = new AgentBadgeTransactionRepository(UnitOfWork);
			_agentBadgeWithRankTransactionRepository = new AgentBadgeWithRankTransactionRepository(UnitOfWork);
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
			toolTip1.SetToolTip(buttonDeleteGamificationSetting, Resources.DeleteAgentBadgeSetting);
			toolTip1.SetToolTip(buttonNew, Resources.NewGamificationSetting);
		}

		private void changeGamificationSettingDescription()
		{
			SelectedGamificationSetting.Description = new Description(textBoxDescription.Text);
			loadGamificationSettings();
		}

		private void validateGamificationSettingDescription()
		{
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedGamificationSetting.Description.Name;
			}
		}

		private void deleteGamificationSetting()
		{
			Repository.Remove(SelectedGamificationSetting);
			_gamificationSettingList.Remove(SelectedGamificationSetting);
			loadGamificationSettings();
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
		
		private void loadGamificationSettingRuleSets()
		{
			if (_gamificationSettingRuleSetList.Count > 0) return;
			_gamificationSettingRuleSetList.Add(GamificationSettingRuleSet.RuleWithRatioConvertor, Resources.RuleWithRatioConvertor);
			if (_toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185))
			{
				_gamificationSettingRuleSetList.Add(GamificationSettingRuleSet.RuleWithDifferentThreshold, Resources.RuleWithDifferentThreshold);
			}
			
			comboBoxAdvBadgeSettingRuleSets.DataSource = new BindingSource(_gamificationSettingRuleSetList, null);
			comboBoxAdvBadgeSettingRuleSets.DisplayMember = "Value";
			comboBoxAdvBadgeSettingRuleSets.ValueMember = "Key";
		}

		private void selectGamificationSetting()
		{
			if (SelectedGamificationSetting == null) return;
			textBoxDescription.Text = SelectedGamificationSetting.Description.ToString();
			comboBoxAdvBadgeSettingRuleSets.SelectedValue =  SelectedGamificationSetting.GamificationSettingRuleSet;

			
		}

		private void addNewGamificationSetting()
		{
			var newBadgeSetting = createGamificationSetting();
			_gamificationSettingList.Add(newBadgeSetting);

			loadGamificationSettings();
			comboBoxAdvGamificationSettings.SelectedIndex = LastItemIndex;
		}

		private IGamificationSetting createGamificationSetting()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_gamificationSettingList, "Description.Name", Resources.NewGamificationSetting);
			IGamificationSetting newGamificationSetting = new GamificationSetting(description.Name) { Description = description };
			Repository.Add(newGamificationSetting);

			return newGamificationSetting;
		}

		private void loadGamificationSettings()
		{
			if (Disposing) return;
			if (_gamificationSettingList == null)
			{
				_gamificationSettingList = new List<IGamificationSetting>();
				//_agentBadgeSettingList.AddRange(Repository.FindAllBadgeSettingsByDescription());
			}

			if (_gamificationSettingList.IsEmpty())
			{
				_gamificationSettingList.Add(createGamificationSetting());
			}

			int selected = comboBoxAdvGamificationSettings.SelectedIndex;
			if (!isWithinRange(selected))
			{
				selected = firstItemIndex;
			}

			// Rebinds list to comboBox.
			comboBoxAdvGamificationSettings.DataSource = null;
			comboBoxAdvGamificationSettings.DataSource = _gamificationSettingList;
			comboBoxAdvGamificationSettings.DisplayMember = "Description";

			comboBoxAdvGamificationSettings.SelectedIndex = selected;
		}

		private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _gamificationSettingList.Count && comboBoxAdvGamificationSettings.DataSource != null;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.Gamification; }
		}

		private void comboBoxAdvGamificationSettingRuleSetSelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdvBadgeSettingRuleSets.SelectedItem == null) return;
			var kvp = ((KeyValuePair<GamificationSettingRuleSet, string>)((ComboBoxAdv)sender).SelectedItem);

			var ruleWithRatioConvertorSelected = kvp.Key == GamificationSettingRuleSet.RuleWithRatioConvertor;

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

		private void reset_Click(object sender, EventArgs e)
		{
			var result = ViewBase.ShowOkCancelMessage(Resources.ResetBadgesConfirm, Resources.ResetBadges);
			if (result != DialogResult.OK) return;
			try
			{
				_agentBadgeTransactionRepository.ResetAgentBadges();
				_agentBadgeWithRankTransactionRepository.ResetAgentBadges();
			}
			catch (Exception)
			{
				ViewBase.ShowErrorMessage(Resources.ResetBadgesFailed, Resources.ResetBadges);
			}
		}
	}
}
