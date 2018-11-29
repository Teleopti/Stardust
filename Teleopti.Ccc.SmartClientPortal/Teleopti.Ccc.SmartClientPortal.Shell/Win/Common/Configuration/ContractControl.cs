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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ContractControl : BaseUserControl, ISettingPage
	{
		private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly List<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
		private const int maxHoursPerDay = 24;
	    private const short itemDiffernce = 1;                      
		private List<IContract> _contractList;
		private readonly IDictionary<EmploymentType, string> _employmentTypeList = new Dictionary<EmploymentType, string>();
		private readonly IMessageBrokerComposite _messageBroker = MessageBrokerInStateHolder.Instance;

		public IUnitOfWork UnitOfWork { get; private set; }

		public IContractRepository Repository { get; private set; }

		private int LastItemIndex
		{
			get { return comboBoxAdvContracts.Items.Count - itemDiffernce; }
		}

		public IContract SelectedContract
		{
			get { return (IContract)comboBoxAdvContracts.SelectedItem; }
		}

		private EmploymentType SelectedEmploymentType
		{
			get
			{
				return (EmploymentType)comboBoxAdvEmpTypes.SelectedValue;
			}
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void changedInfo()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			string changed = _localizer.UpdatedByText(SelectedContract, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}
		
		public void Unload()
		{
			// Disposes or flag anything possible.
			_messageBroker.UnregisterSubscription(refreshMultiplicatorDefinitionSet);
			timeSpanTextBoxPlanningMin.Validated -= timeSpanTextBoxPlanningMinValidated;
			timeSpanTextBoxPlanningMax.Validated -= timeSpanTextBoxPlanningMaxValidated;
			_contractList = null;
		}

		public void LoadControl()
		{
			setUpTimeSpanBoxes();
			loadEmploymentTypes();
			loadContracts();
			loadMultiplicatorDefinitionSets();

			initializeMessageBroker();
		}
		
		private void setUpTimeSpanBoxes()
		{
			textBoxExtAvgWorkTimePerDay.SetSize(54, 23);
			textBoxExtMaxTimePerWeek.SetSize(54, 23);
			textBoxExtMinTimePerWeek.SetSize(54, 23);
			textBoxExtNightlyRestTime.SetSize(54, 23);
			textBoxExtWeeklyRestTime.SetSize(54, 23);
			timeSpanTextBoxNegativeTolerance.SetSize(54, 23);
			timeSpanTextBoxPositiveTolerance.SetSize(54, 23);
			textBoxExMinTimeSchedulePeriod.SetSize(54, 23);
			timeSpanTextBoxPlanningMin.SetSize(54,23);
			timeSpanTextBoxPlanningMax.SetSize(54,23);
			
			textBoxExtAvgWorkTimePerDay.AllowNegativeValues = false;
			textBoxExtMaxTimePerWeek.AllowNegativeValues = false;
			textBoxExtMinTimePerWeek.AllowNegativeValues = false;
			textBoxExtNightlyRestTime.AllowNegativeValues = false;
			textBoxExtWeeklyRestTime.AllowNegativeValues = false;
			timeSpanTextBoxNegativeTolerance.AllowNegativeValues = false;
			timeSpanTextBoxPositiveTolerance.AllowNegativeValues = false;
			textBoxExMinTimeSchedulePeriod.AllowNegativeValues = false;
		}

		private void initializeMessageBroker()
		{
			_messageBroker.RegisterEventSubscription(refreshMultiplicatorDefinitionSet,typeof(IMultiplicatorDefinitionSet));
		}

		private void refreshMultiplicatorDefinitionSet(object sender, EventMessageArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventMessageArgs>(refreshMultiplicatorDefinitionSet), sender, e);
			}
			else
			{
				if (e.Message.DomainUpdateType == DomainUpdateType.Insert)
				{
					IMultiplicatorDefinitionSet newMultiplicatorDefinitionSet = _multiplicatorDefinitionSetRepository.Get(e.Message.DomainObjectId);
					if (newMultiplicatorDefinitionSet!=null)
					{
						_multiplicatorDefinitionSets.Add(newMultiplicatorDefinitionSet);
						checkedListBoxMultiplicatorDefenitionSets.Items.Add(newMultiplicatorDefinitionSet, false);
					}
				}
				if (e.Message.DomainUpdateType == DomainUpdateType.Delete)
				{
					IMultiplicatorDefinitionSet multiplicatorDefinitionSetToRemove =
						_multiplicatorDefinitionSets.FirstOrDefault(mds => mds.Id.Value == e.Message.DomainObjectId);
					if (multiplicatorDefinitionSetToRemove!=null)
					{
						checkedListBoxMultiplicatorDefenitionSets.Items.Remove(multiplicatorDefinitionSetToRemove);
						_multiplicatorDefinitionSets.Remove(multiplicatorDefinitionSetToRemove);
					}
				}
			}
		}

		public void  SaveChanges()
		{}

		public ContractControl()
		{
			InitializeComponent();

			comboBoxAdvContracts.SelectedIndexChanging += comboBoxAdvContractsSelectedIndexChanging;
			comboBoxAdvContracts.SelectedIndexChanged += comboBoxAdvContractsSelectedIndexChanged;
			comboBoxAdvEmpTypes.SelectedIndexChanged += comboBoxAdvEmpTypesSelectedIndexChanged;
			textBoxDescription.Validating += textBoxDescriptionValidating;
			textBoxDescription.Validated += textBoxDescriptionValidated;
			textBoxExtAvgWorkTimePerDay.Validating += textBoxExtAvgWorkTimePerDayValidating;
			textBoxExtAvgWorkTimePerDay.Validated += textBoxExtAvgWorkTimePerDayValidated;
			textBoxExtMaxTimePerWeek.Validating += textBoxExtMinMaxTimePerWeekValidating;
			textBoxExtMinTimePerWeek.Validating += textBoxExtMinMaxTimePerWeekValidating;
			textBoxExtMaxTimePerWeek.Validated += textBoxExtMinMaxTimePerWeekValidated;
			textBoxExtMinTimePerWeek.Validated += textBoxExtMinMaxTimePerWeekValidated;
			textBoxExtNightlyRestTime.Validating += textBoxExtNightlyRestTimeValidating;
			textBoxExtNightlyRestTime.Validated += textBoxExtNightlyRestTimeValidated;
			textBoxExtWeeklyRestTime.Validating += textBoxExtWeeklyRestTimeValidating;
			textBoxExtWeeklyRestTime.Validated += textBoxExtWeeklyRestTimeValidated;
			timeSpanTextBoxNegativeTolerance.Validated += timeSpanTextBoxNegativeToleranceValidated;
			timeSpanTextBoxPositiveTolerance.Validated += timeSpanTextBoxPositiveToleranceValidated;
			buttonNew.Click += buttonNewClick;
			buttonDeleteContract.Click += buttonDeleteContractClick;
			textBoxExMinTimeSchedulePeriod.Validated += textBoxExMinTimeSchedulePeriodValidated;
			textBoxExMinTimeSchedulePeriod.Validating += textBoxExMinTimeSchedulePeriodValidating;

			timeSpanTextBoxPlanningMin.Validated += timeSpanTextBoxPlanningMinValidated;
			timeSpanTextBoxPlanningMax.Validated += timeSpanTextBoxPlanningMaxValidated;
		}

		void timeSpanTextBoxPlanningMaxValidated(object sender, EventArgs e)
		{
			SelectedContract.PlanningTimeBankMax = timeSpanTextBoxPlanningMax.Value;
		}

		void timeSpanTextBoxPlanningMinValidated(object sender, EventArgs e)
		{
			if (timeSpanTextBoxPlanningMin.Value <= TimeSpan.FromHours(-100))
				timeSpanTextBoxPlanningMin.SetInitialResolution(TimeSpan.FromHours(-100).Add(TimeSpan.FromMinutes(1)));
			SelectedContract.PlanningTimeBankMin = timeSpanTextBoxPlanningMin.Value;
		}

		void timeSpanTextBoxPositiveToleranceValidated(object sender, EventArgs e)
		{
			SelectedContract.PositivePeriodWorkTimeTolerance = timeSpanTextBoxPositiveTolerance.Value;
		}

		void timeSpanTextBoxNegativeToleranceValidated(object sender, EventArgs e)
		{
			SelectedContract.NegativePeriodWorkTimeTolerance = timeSpanTextBoxNegativeTolerance.Value;
		}

		private void textBoxExtWeeklyRestTimeValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

		private void textBoxExMinTimeSchedulePeriodValidated(object sender, EventArgs e)
		{
			SelectedContract.MinTimeSchedulePeriod = textBoxExMinTimeSchedulePeriod.Value;
		}

		private void textBoxExtNightlyRestTimeValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

		private void textBoxExtMinMaxTimePerWeekValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

		private void textBoxExtAvgWorkTimePerDayValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTime = new WorkTime(textBoxExtAvgWorkTimePerDay.Value);
		}

		private void textBoxDescriptionValidated(object sender, EventArgs e)
		{
			changeContractDescription();
		}

		private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				validateContractDescription();
			}
		}

		private void textBoxExMinTimeSchedulePeriodValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract == null) return;
			if (SelectedContract.EmploymentType != EmploymentType.HourlyStaff)
				textBoxExMinTimeSchedulePeriod.SetInitialResolution(TimeSpan.Zero);
		}

		private void textBoxExtMinMaxTimePerWeekValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateHoursPerWeek();
			}
		}

		private bool validateHoursPerWeek()
		{
			var cancel = false;

			var minHoursPerWeek = textBoxExtMinTimePerWeek.Value;
			var maxHoursPerWeek = textBoxExtMaxTimePerWeek.Value;
			if (!validateHoursSetting(maxHoursPerWeek, minHoursPerWeek, MaxHoursPerWeek))
			{
				ViewBase.ShowErrorMessage(Resources.HoursPerWeekIsInvalid, Resources.TimeError);
				cancel = true;
			}

			return cancel;
		}

		private void textBoxExtNightlyRestTimeValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateNightlyRestTime();
			}
		}

		private void textBoxExtWeeklyRestTimeValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateWeeklyRestTime();
			}
		}

		private void textBoxExtAvgWorkTimePerDayValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateAvgWorkTimePerDay();
			}
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			//addNewContract();
			if (SelectedContract == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewContract();
			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteContractClick(object sender, EventArgs e)
		{
			if (SelectedContract == null) return;
			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteContract,
				SelectedContract.Description
				);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;
			deleteContract();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvContractsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedContract == null) return;
			Cursor.Current = Cursors.WaitCursor;
			selectContract();
			changedInfo();
			Cursor.Current = Cursors.Default;
		}

		private void comboBoxAdvContractsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			Repository = new ContractRepository(UnitOfWork);
			_multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(UnitOfWork);
		}

		public void Persist()
		{
			SaveChanges();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Contract);
		}

		public string TreeNode()
		{
			return Resources.Contracts;
		}

		public void OnShow()
		{
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDeleteContract, Resources.DeleteContract);
			toolTip1.SetToolTip(buttonNew, Resources.NewContract);
		}

		private bool validateAvgWorkTimePerDay()
		{
			bool cancel = false;

			if (!validateMaxHours(textBoxExtAvgWorkTimePerDay.Value, maxHoursPerDay))
			{
				ViewBase.ShowErrorMessage(Resources.AverageWorkTimeCannotBeMoreThan24Hours,
									   Resources.TimeError);
				cancel = true;
			}

			return cancel;
		}

		private bool validateNightlyRestTime()
		{
			bool cancel = false;

			TimeSpan nightlyRestTime = textBoxExtNightlyRestTime.Value;
			if (!validateMaxHours(nightlyRestTime, maxHoursPerDay))
			{
				ViewBase.ShowErrorMessage(Resources.NightlyRestTimeCannotBeMoreThan24Hours,
								   Resources.TimeError);
				cancel = true;
			}

			return cancel;
		}

		private bool validateWeeklyRestTime()
		{
			bool cancel = false;

			TimeSpan weeklyRestTime = textBoxExtWeeklyRestTime.Value;

			if (!validateMaxHours(weeklyRestTime, MaxHoursPerWeek))
			{
				ViewBase.ShowErrorMessage(Resources.WeeklyRestTimeCannotBeMoreThan168Hours,
								   Resources.TimeError);
				cancel = true;
			}
			return cancel;
		}

		private void changeContractDescription()
		{
			SelectedContract.Description = new Description(textBoxDescription.Text);

			loadContracts();
		}

		private void validateContractDescription()
		{
			if (string.IsNullOrWhiteSpace(textBoxDescription.Text))
				textBoxDescription.SelectedText = SelectedContract.Description.Name;
		}

		private void deleteContract()
		{
			Repository.Remove(SelectedContract);
			_contractList.Remove(SelectedContract);

			loadContracts();
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

			tableLayoutPanelSubHeader3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader3.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader3.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}
		
		private void loadEmploymentTypes()
		{
			if (_employmentTypeList.Count > 0) return;
			_employmentTypeList.Add(EmploymentType.FixedStaffNormalWorkTime, Resources.EmploymentTypeFixedStaffNormalWorkTime);
			_employmentTypeList.Add(EmploymentType.FixedStaffDayWorkTime, Resources.EmploymentTypeFixedStaffDayWorkTime);
			_employmentTypeList.Add(EmploymentType.HourlyStaff, Resources.EmploymentTypeHourlyStaff);

			// Get the data to Employment type. No need to sort.
			comboBoxAdvEmpTypes.DataSource = new BindingSource(_employmentTypeList, null);
			comboBoxAdvEmpTypes.DisplayMember = "Value";
			comboBoxAdvEmpTypes.ValueMember = "Key";
		}

		private void selectContract()
		{
			if (SelectedContract == null) return;
			textBoxDescription.Text = SelectedContract.Description.ToString();
			comboBoxAdvEmpTypes.SelectedValue =  SelectedContract.EmploymentType;
			textBoxExtMaxTimePerWeek.SetInitialResolution(SelectedContract.WorkTimeDirective.MaxTimePerWeek);
			textBoxExtMinTimePerWeek.SetInitialResolution(SelectedContract.WorkTimeDirective.MinTimePerWeek);
			textBoxExtNightlyRestTime.SetInitialResolution(SelectedContract.WorkTimeDirective.NightlyRest);
			textBoxExtWeeklyRestTime.SetInitialResolution(SelectedContract.WorkTimeDirective.WeeklyRest);
			textBoxExtAvgWorkTimePerDay.SetInitialResolution(SelectedContract.WorkTime.AvgWorkTimePerDay);
			timeSpanTextBoxNegativeTolerance.SetInitialResolution(SelectedContract.NegativePeriodWorkTimeTolerance);
			timeSpanTextBoxPositiveTolerance.SetInitialResolution(SelectedContract.PositivePeriodWorkTimeTolerance);
			numericUpDownNegativeDayOff.Value = SelectedContract.NegativeDayOffTolerance;
			numericUpDownPositiveDayOff.Value = SelectedContract.PositiveDayOffTolerance;
			textBoxExMinTimeSchedulePeriod.SetInitialResolution(SelectedContract.MinTimeSchedulePeriod);
			timeSpanTextBoxPlanningMin.SetInitialResolution(SelectedContract.PlanningTimeBankMin);
			timeSpanTextBoxPlanningMax.SetInitialResolution(SelectedContract.PlanningTimeBankMax);
			checkBoxAdjustTimeBankWithSeasonality.Checked = SelectedContract.AdjustTimeBankWithSeasonality;
			checkBoxAdjustTimeBankWithPartTimePercentage.Checked = SelectedContract.AdjustTimeBankWithPartTimePercentage;
			radioButtonFromContract.Checked = SelectedContract.WorkTimeSource == WorkTimeSource.FromContract;
			radioButtonFromSchedule.Checked = SelectedContract.WorkTimeSource == WorkTimeSource.FromSchedulePeriod;
			loadMultiplicatorCheckbox();
		}

		private WorkTimeDirective getWorkTimeDirective()
		{
			TimeSpan maxTime = textBoxExtMaxTimePerWeek.Value;
			TimeSpan minTime = textBoxExtMinTimePerWeek.Value;
			TimeSpan nightRest = textBoxExtNightlyRestTime.Value;
			TimeSpan weeklyRest = textBoxExtWeeklyRestTime.Value;
			return new WorkTimeDirective(minTime, maxTime, nightRest, weeklyRest);
		}

		private static bool validateMaxHours(TimeSpan timeSpan, int maxHours)
		{
			bool isValid = true;

			switch (maxHours)
			{
				case maxHoursPerDay:
					if (timeSpan.Days > 0 || timeSpan.Hours > maxHours)
					{
						isValid = false;
					}
					break;
				case MaxHoursPerWeek:
					if (timeSpan.Days * 24 + timeSpan.Hours > maxHours)
					{
						isValid = false;
					}
					break;
				default:
					isValid = false;
					break;
			}

			return isValid;
		}

		private static bool validateHoursSetting(TimeSpan currentMaxHoursSetting, TimeSpan currentMinHoursSetting, int maxHours)
		{
			bool isValid = true;
			if (currentMaxHoursSetting < currentMinHoursSetting)
			{
				isValid = false;
			}
			else
			{
				switch (maxHours)
				{
					case maxHoursPerDay:
						if (currentMaxHoursSetting.Days > 0 || currentMaxHoursSetting.Hours > maxHours|| 
							currentMinHoursSetting.Days > 0 ||currentMinHoursSetting.Hours > maxHours)
						{
							isValid = false;
						}
						break;
					case MaxHoursPerWeek:
						if ((currentMaxHoursSetting.Days*24 + currentMaxHoursSetting.Hours > maxHours) ||
						    (currentMinHoursSetting.Days*24 + currentMinHoursSetting.Hours > maxHours))
						{
							isValid = false;
						}
						break;
					default:
						isValid = false;
						break;
				}
			}
			return isValid;
		}

		private void addNewContract()
		{
			var newContract = createContract();
			newContract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
			_contractList.Add(newContract);

			loadContracts();
			comboBoxAdvContracts.SelectedIndex = LastItemIndex;
		}

		private IContract createContract()
		{
			// Formats the name.
			Description description = PageHelper.CreateNewName(_contractList, "Description.Name", Resources.NewContract);
			IContract newContract = new Contract(description.Name) { Description = description };
			Repository.Add(newContract);

			return newContract;
		}

		private void loadContracts()
		{
			if (Disposing) return;
			if (_contractList == null)
			{
				_contractList = new List<IContract>();
				_contractList.AddRange(Repository.FindAllContractByDescription());
			}

			if (_contractList.IsEmpty())
			{
				_contractList.Add(createContract());
			}

			int selected = comboBoxAdvContracts.SelectedIndex;
			if (!isWithinRange(selected))
			{
				selected = firstItemIndex;
			}

			// Rebinds list to comboBoxContract.
			comboBoxAdvContracts.DataSource = null;
			comboBoxAdvContracts.DataSource = _contractList;
			comboBoxAdvContracts.DisplayMember = "Description";

			comboBoxAdvContracts.SelectedIndex = selected;
		}

		private void loadMultiplicatorDefinitionSets()
		{
			checkedListBoxMultiplicatorDefenitionSets.DisplayMember = "Name";
			_multiplicatorDefinitionSets.Clear();
			_multiplicatorDefinitionSets.AddRange(_multiplicatorDefinitionSetRepository.LoadAll());

			loadMultiplicatorCheckbox();
		}

		private void loadMultiplicatorCheckbox()
		{
			checkedListBoxMultiplicatorDefenitionSets.Items.Clear();
			var sortedList = new List<IMultiplicatorDefinitionSet>();
			foreach (IMultiplicatorDefinitionSet definitionSet in SelectedContract.MultiplicatorDefinitionSetCollection.OrderBy(s => s.Name).ToList())
			{
				if (((IDeleteTag) definitionSet).IsDeleted) continue;
				sortedList.Add(definitionSet);
				checkedListBoxMultiplicatorDefenitionSets.Items.Add(definitionSet, true);
			}
			foreach (IMultiplicatorDefinitionSet definitionSet in _multiplicatorDefinitionSets.OrderBy(s => s.Name).ToList())
			{
				if (((IDeleteTag) definitionSet).IsDeleted) continue;
				if (!sortedList.Contains(definitionSet))
				{
					checkedListBoxMultiplicatorDefenitionSets.Items.Add(definitionSet, false);
				}
			}
		}

		private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _contractList.Count && comboBoxAdvContracts.DataSource != null;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.Contract; }
		}

		private void checkedListBoxMultiplicatorDefenitionSetsItemCheck(object sender, ItemCheckEventArgs e)
		{
			var definitionSet = ((CheckedListBox) sender).SelectedItem as IMultiplicatorDefinitionSet;
			if (definitionSet == null) return;
			if (e.NewValue == CheckState.Checked)
				SelectedContract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			else
				SelectedContract.RemoveMultiplicatorDefinitionSetCollection(definitionSet);
		}

		private void comboBoxAdvEmpTypesSelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdvEmpTypes.SelectedItem == null) return;
			var kvp = ((KeyValuePair<EmploymentType, string>)((ComboBoxAdv)sender).SelectedItem);

			var hourly = kvp.Key == EmploymentType.HourlyStaff;
			
			autoLabel7.Enabled = hourly;
			textBoxExMinTimeSchedulePeriod.Enabled = hourly;
			labelTimeBankMin.Enabled = !hourly;
			labelTimeBankMax.Enabled = !hourly;
			timeSpanTextBoxPlanningMin.Enabled = !hourly;
			timeSpanTextBoxPlanningMax.Enabled = !hourly;
			numericUpDownPositiveDayOff.Enabled = !hourly;
			numericUpDownNegativeDayOff.Enabled = !hourly;
			autoLabel13.Enabled = !hourly;
			textBoxExtMinTimePerWeek.Enabled = !hourly;
			
			if (SelectedContract == null) return;
			SelectedContract.EmploymentType = SelectedEmploymentType;

			if (SelectedEmploymentType != EmploymentType.HourlyStaff)
			{
				SelectedContract.MinTimeSchedulePeriod = TimeSpan.Zero;
			}
			else
			{
				SelectedContract.NegativeDayOffTolerance = 0;
				SelectedContract.PositiveDayOffTolerance = 0;
			}
		}

		private void checkBoxAdjustTimeBankWithSeasonalityCheckStateChanged(object sender, EventArgs e)
		{
			SelectedContract.AdjustTimeBankWithSeasonality = checkBoxAdjustTimeBankWithSeasonality.Checked;
		}

		private void checkBoxAdjustTimeBankWithPartTimePercentageCheckStateChanged(object sender, EventArgs e)
		{
			SelectedContract.AdjustTimeBankWithPartTimePercentage = checkBoxAdjustTimeBankWithPartTimePercentage.Checked;
		}

		private void numericUpDownPositiveDayOffValueChanged(object sender, EventArgs e)
		{
			SelectedContract.PositiveDayOffTolerance = (int) numericUpDownPositiveDayOff.Value;
		}

		private void numericUpDownNegativeDayOffValueChanged(object sender, EventArgs e)
		{
			SelectedContract.NegativeDayOffTolerance = (int)numericUpDownNegativeDayOff.Value;
		}

		private void radioButtonFromContractCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonFromContract.Checked)
			{
				SelectedContract.WorkTimeSource  = WorkTimeSource.FromContract;
			}
		}

		private void radioButtonFromScheduleCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonFromSchedule.Checked)
			{
				SelectedContract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
			}
		}
	}
}
