using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
		private readonly List<GamificationSettingView> _gamificationSettingList = new List<GamificationSettingView>();
		private readonly List<GamificationSettingView> _gamificationSettingListToBeDeleted;
		private readonly IDictionary<GamificationSettingRuleSet, string> _gamificationSettingRuleSetList = new Dictionary<GamificationSettingRuleSet, string>();
		private readonly IToggleManager _toggleManager;
		private readonly Lazy<GamificationSettingRuleWithDifferentThresholdControl> gamificationSettingRuleWithDifferentThresholdControl;
		private readonly Lazy<GamificationSettingRuleWithRatioConvertorControl> gamificationSettingRuleWithRatioConvertorControl;

		public IUnitOfWork UnitOfWork { get; private set; }

		public IGamificationSettingRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvGamificationSettings.Items.Count - itemDiffernce; }
		}

		public GamificationSettingView SelectedGamificationSetting
		{
			get { return (GamificationSettingView)comboBoxAdvGamificationSettings.SelectedItem; }
		}

		public GamificationSettingRuleSet SelectedGamificationSettingRuleSet
		{
			get { return (GamificationSettingRuleSet) comboBoxAdvBadgeSettingRuleSets.SelectedValue; }
		}

		public GamificationSettingControl(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
			_gamificationSettingListToBeDeleted = new List<GamificationSettingView>();

			InitializeComponent();

			gamificationSettingRuleWithDifferentThresholdControl = new Lazy<GamificationSettingRuleWithDifferentThresholdControl>
				(
				() =>
				{
					var ctrl = new GamificationSettingRuleWithDifferentThresholdControl();
					ctrl.Dock = DockStyle.Fill;
					ctrl.Validated += gamificationSettingRuleWithDifferentThreshold_Validated;
					return ctrl;
				});
			gamificationSettingRuleWithRatioConvertorControl = new Lazy<GamificationSettingRuleWithRatioConvertorControl>(
				() =>
				{
					var ctrl = new GamificationSettingRuleWithRatioConvertorControl();
					ctrl.Dock = DockStyle.Fill;
					ctrl.Validated += gamificationSettingRuleWithRatioConvertor_Validated;
					return ctrl;
				});
		}

		private void gamificationSettingRuleWithRatioConvertor_Validated(object sender, EventArgs e)
		{
			var ruleSettingWithRatioConvertor = gamificationSettingRuleWithRatioConvertorControl.Value.CurrentSetting;

			SelectedGamificationSetting.AHTBadgeEnabled = ruleSettingWithRatioConvertor.AHTBadgeEnabled;
			SelectedGamificationSetting.AHTThreshold = ruleSettingWithRatioConvertor.AHTThreshold;

			SelectedGamificationSetting.AdherenceBadgeEnabled = ruleSettingWithRatioConvertor.AdherenceBadgeEnabled;
			SelectedGamificationSetting.AdherenceThreshold = ruleSettingWithRatioConvertor.AdherenceThreshold;

			SelectedGamificationSetting.AnsweredCallsBadgeEnabled = ruleSettingWithRatioConvertor.AnsweredCallsBadgeEnabled;
			SelectedGamificationSetting.AnsweredCallsThreshold = ruleSettingWithRatioConvertor.AnsweredCallsThreshold;

			SelectedGamificationSetting.GoldToSilverBadgeRate = ruleSettingWithRatioConvertor.GoldToSilverBadgeRate;
			SelectedGamificationSetting.SilverToBronzeBadgeRate = ruleSettingWithRatioConvertor.SilverToBronzeBadgeRate;
		}

		private void gamificationSettingRuleWithDifferentThreshold_Validated(object sender, EventArgs e)
		{
			var ruleSettingWithDifferentThreshold = gamificationSettingRuleWithDifferentThresholdControl.Value.CurrentSetting;
			SelectedGamificationSetting.AHTBadgeEnabled = ruleSettingWithDifferentThreshold.AHTBadgeEnabled;
			SelectedGamificationSetting.AHTBronzeThreshold = ruleSettingWithDifferentThreshold.AHTBronzeThreshold;
			SelectedGamificationSetting.AHTSilverThreshold = ruleSettingWithDifferentThreshold.AHTSilverThreshold;
			SelectedGamificationSetting.AHTGoldThreshold = ruleSettingWithDifferentThreshold.AHTGoldThreshold;

			SelectedGamificationSetting.AdherenceBadgeEnabled = ruleSettingWithDifferentThreshold.AdherenceBadgeEnabled;
			SelectedGamificationSetting.AdherenceBronzeThreshold = ruleSettingWithDifferentThreshold.AdherenceBronzeThreshold;
			SelectedGamificationSetting.AdherenceGoldThreshold = ruleSettingWithDifferentThreshold.AdherenceGoldThreshold;
			SelectedGamificationSetting.AdherenceSilverThreshold = ruleSettingWithDifferentThreshold.AdherenceSilverThreshold;

			SelectedGamificationSetting.AnsweredCallsBadgeEnabled = ruleSettingWithDifferentThreshold.AnsweredCallsBadgeEnabled;
			SelectedGamificationSetting.AnsweredCallsBronzeThreshold = ruleSettingWithDifferentThreshold.AnsweredCallsBronzeThreshold;
			SelectedGamificationSetting.AnsweredCallsGoldThreshold = ruleSettingWithDifferentThreshold.AnsweredCallsGoldThreshold;
			SelectedGamificationSetting.AnsweredCallsSilverThreshold = ruleSettingWithDifferentThreshold.AnsweredCallsSilverThreshold;
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
			string changed = _localizer.UpdatedByText(SelectedGamificationSetting.ContainedEntity, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}
		
		public void Unload()
		{
			// Disposes or flag anything possible.
			_gamificationSettingList.Clear();
		}

		public void LoadControl()
		{
			loadGamificationSettingRuleSets();
			loadGamificationSettings();
		}

		public void SaveChanges()
		{
			Persist();
		}

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
		{}

		public void Persist()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repo = new GamificationSettingRepository(uow);

				foreach (var settingView in _gamificationSettingListToBeDeleted)
				{
					repo.Remove(settingView.ContainedOriginalEntity);
				}
				foreach (var settingView in _gamificationSettingList)
				{
					if (!settingView.Id.HasValue)
					{
						repo.Add(settingView.ContainedEntity);
						settingView.UpdateAfterMerge(settingView.ContainedEntity);
					}
					else
					{
						var updatedSetting = uow.Merge(settingView.ContainedEntity);
						LazyLoadingManager.Initialize(updatedSetting.UpdatedBy);
						settingView.UpdateAfterMerge(updatedSetting);
					}

					
				}
				uow.PersistAll();
			}
			_gamificationSettingListToBeDeleted.Clear();
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
		{}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDeleteGamificationSetting, Resources.DeleteAgentBadgeSetting);
			toolTip1.SetToolTip(buttonNew, Resources.NewGamificationSetting);
		}

		private void changeGamificationSettingDescription()
		{
			if (SelectedGamificationSetting == null) return;
			_gamificationSettingList[comboBoxAdvGamificationSettings.SelectedIndex].Description = new Description(textBoxDescription.Text);
			SelectedGamificationSetting.Description = new Description(textBoxDescription.Text);
			bindSettingListToComboBox();
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
			if (SelectedGamificationSetting.Id.HasValue)
			{
				_gamificationSettingListToBeDeleted.Add(new GamificationSettingView(SelectedGamificationSetting.ContainedEntity));
			}
			_gamificationSettingList.Remove(SelectedGamificationSetting);
			bindSettingListToComboBox();
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
			if (SelectedGamificationSetting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithRatioConvertor)
			{
				var selectedSetting = new RuleSettingWithRatioConvertor()
				{
					AnsweredCallsBadgeEnabled = SelectedGamificationSetting.AnsweredCallsBadgeEnabled,
					AHTBadgeEnabled = SelectedGamificationSetting.AHTBadgeEnabled,
					AdherenceBadgeEnabled = SelectedGamificationSetting.AdherenceBadgeEnabled,

					AnsweredCallsThreshold = SelectedGamificationSetting.AnsweredCallsThreshold,

					AHTThreshold = SelectedGamificationSetting.AHTThreshold,

					AdherenceThreshold = SelectedGamificationSetting.AdherenceThreshold,

					GoldToSilverBadgeRate = SelectedGamificationSetting.GoldToSilverBadgeRate,
					SilverToBronzeBadgeRate = SelectedGamificationSetting.SilverToBronzeBadgeRate
				};
				gamificationSettingRuleWithRatioConvertorControl.Value.CurrentSetting = selectedSetting;
			}
			else
			{
				var selectedSetting = new RuleSettingWithDifferentThreshold()
				{
					AnsweredCallsBadgeEnabled = SelectedGamificationSetting.AnsweredCallsBadgeEnabled,
					AHTBadgeEnabled = SelectedGamificationSetting.AHTBadgeEnabled,
					AdherenceBadgeEnabled = SelectedGamificationSetting.AdherenceBadgeEnabled,

					AnsweredCallsBronzeThreshold = SelectedGamificationSetting.AnsweredCallsBronzeThreshold,
					AnsweredCallsSilverThreshold = SelectedGamificationSetting.AnsweredCallsSilverThreshold,
					AnsweredCallsGoldThreshold = SelectedGamificationSetting.AnsweredCallsGoldThreshold,

					AHTBronzeThreshold = SelectedGamificationSetting.AHTBronzeThreshold,
					AHTSilverThreshold = SelectedGamificationSetting.AHTSilverThreshold,
					AHTGoldThreshold = SelectedGamificationSetting.AHTGoldThreshold,

					AdherenceBronzeThreshold = SelectedGamificationSetting.AdherenceBronzeThreshold,
					AdherenceSilverThreshold = SelectedGamificationSetting.AdherenceSilverThreshold,
					AdherenceGoldThreshold = SelectedGamificationSetting.AdherenceGoldThreshold
				};
				gamificationSettingRuleWithDifferentThresholdControl.Value.CurrentSetting = selectedSetting;
			}
		}

		private void addNewGamificationSetting()
		{
			var newBadgeSetting = createGamificationSetting();
			_gamificationSettingList.Add(newBadgeSetting);
			//loadGamificationSettings();
			bindSettingListToComboBox();
			comboBoxAdvGamificationSettings.SelectedIndex = LastItemIndex;
		}

		private GamificationSettingView createGamificationSetting()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_gamificationSettingList, "Description.Name", Resources.NewGamificationSetting);
			IGamificationSetting newGamificationSetting = new GamificationSetting(description.Name) { Description = description };

			return new GamificationSettingView(newGamificationSetting);
		}

		private void loadGamificationSettings()
		{
			if (Disposing) return;
			using (var myUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var gamificationSettingRepo = new GamificationSettingRepository(myUow);
				var gamificationSettings = gamificationSettingRepo.FindAllGamificationSettingsSortedByDescription().ToList();

				foreach (var setting in gamificationSettings)
				{
					LazyLoadingManager.Initialize(setting.UpdatedBy);
					_gamificationSettingList.Add(new GamificationSettingView(setting));
				}
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
			bindSettingListToComboBox();

			comboBoxAdvGamificationSettings.SelectedIndex = selected;
		}

		private void bindSettingListToComboBox()
		{
			comboBoxAdvGamificationSettings.DataSource = null;
			comboBoxAdvGamificationSettings.DataSource = _gamificationSettingList;
			comboBoxAdvGamificationSettings.DisplayMember = "Description";
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

			tableLayoutPanel6.Controls.Remove(tableLayoutPanel6.GetControlFromPosition(0,2));
			if (ruleWithRatioConvertorSelected)
			{
				tableLayoutPanel6.Controls.Add(gamificationSettingRuleWithRatioConvertorControl.Value, 0, 2);
			}
			else
			{
				tableLayoutPanel6.Controls.Add(gamificationSettingRuleWithDifferentThresholdControl.Value, 0, 2);
			}
			tableLayoutPanel6.SetColumnSpan(tableLayoutPanel6.GetControlFromPosition(0, 2), 2);

			if(SelectedGamificationSetting == null) return;
			SelectedGamificationSetting.GamificationSettingRuleSet = SelectedGamificationSettingRuleSet;
		}

		private void reset_Click(object sender, EventArgs e)
		{
			var result = ViewBase.ShowOkCancelMessage(Resources.ResetBadgesConfirm, Resources.ResetBadges);
			if (result != DialogResult.OK) return;
			try
			{
				using (var myUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var agentBadgeTransactionRepository = new AgentBadgeTransactionRepository(myUow);
					var agentBadgeWithRankTransactionRepository = new AgentBadgeWithRankTransactionRepository(myUow);
					agentBadgeTransactionRepository.ResetAgentBadges();
					agentBadgeWithRankTransactionRepository.ResetAgentBadges();
					myUow.PersistAll();
				}
			}
			catch (Exception)
			{
				ViewBase.ShowErrorMessage(Resources.ResetBadgesFailed, Resources.ResetBadges);
			}
		}
	}
}
