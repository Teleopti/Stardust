﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class ContractControl : BaseUserControl, ISettingPage
	{
        private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
        private readonly List<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
		private const short InvalidItemIndex = -1;
		private const short FirstItemIndex = 0;
		private const int MaxHoursPerDay = 24;
		private const int MaxHoursPerWeek = 168;
		private const short ItemDiffernce = 1;                      
		private List<IContract> _contractList;
        private readonly IDictionary<EmploymentType, string> _employmentTypeList = new Dictionary<EmploymentType, string>();
        private readonly IMessageBroker _messageBroker = StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging;

		public IUnitOfWork UnitOfWork { get; private set; }

        public IContractRepository Repository { get; private set; }

        private int LastItemIndex
        {
            get { return comboBoxAdvContracts.Items.Count - ItemDiffernce; }
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
            _messageBroker.UnregisterEventSubscription(refreshMultiplicatorDefinitionSet);
            timeSpanTextBoxPlanningMin.Validated -= TimeSpanTextBoxPlanningMinValidated;
            timeSpanTextBoxPlanningMax.Validated -= TimeSpanTextBoxPlanningMaxValidated;
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
            textBoxExtNightlyRestTime.SetSize(54, 23);
            textBoxExtWeeklyRestTime.SetSize(54, 23);
            timeSpanTextBoxNegativeTolerance.SetSize(54, 23);
            timeSpanTextBoxPositiveTolerance.SetSize(54, 23);
            textBoxExMinTimeSchedulePeriod.SetSize(54, 23);
            timeSpanTextBoxPlanningMin.SetSize(54,23);
            timeSpanTextBoxPlanningMax.SetSize(54,23);
            
            textBoxExtAvgWorkTimePerDay.AllowNegativeValues = false;
            textBoxExtMaxTimePerWeek.AllowNegativeValues = false;
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
            comboBoxAdvEmpTypes.SelectedIndexChanged += ComboBoxAdvEmpTypesSelectedIndexChanged;
			textBoxDescription.Validating += TextBoxDescriptionValidating;
			textBoxDescription.Validated += TextBoxDescriptionValidated;
			textBoxExtAvgWorkTimePerDay.Validating += TextBoxExtAvgWorkTimePerDayValidating;
			textBoxExtAvgWorkTimePerDay.Validated += TextBoxExtAvgWorkTimePerDayValidated;
			textBoxExtMaxTimePerWeek.Validating += TextBoxExtMaxTimePerWeekValidating;
			textBoxExtMaxTimePerWeek.Validated += TextBoxExtMaxTimePerWeekValidated;
			textBoxExtNightlyRestTime.Validating += TextBoxExtNightlyRestTimeValidating;
			textBoxExtNightlyRestTime.Validated += TextBoxExtNightlyRestTimeValidated;
			textBoxExtWeeklyRestTime.Validating += TextBoxExtWeeklyRestTimeValidating;
			textBoxExtWeeklyRestTime.Validated += TextBoxExtWeeklyRestTimeValidated;
            timeSpanTextBoxNegativeTolerance.Validated += TimeSpanTextBoxNegativeToleranceValidated;
            timeSpanTextBoxPositiveTolerance.Validated += TimeSpanTextBoxPositiveToleranceValidated;
			buttonNew.Click += ButtonNewClick;
			buttonDeleteContract.Click += ButtonDeleteContractClick;
            textBoxExMinTimeSchedulePeriod.Validated += TextBoxExMinTimeSchedulePeriodValidated;
            textBoxExMinTimeSchedulePeriod.Validating += TextBoxExMinTimeSchedulePeriodValidating;

            timeSpanTextBoxPlanningMin.Validated += TimeSpanTextBoxPlanningMinValidated;
            timeSpanTextBoxPlanningMax.Validated += TimeSpanTextBoxPlanningMaxValidated;
        }

        void TimeSpanTextBoxPlanningMaxValidated(object sender, EventArgs e)
        {
            SelectedContract.PlanningTimeBankMax = timeSpanTextBoxPlanningMax.Value;
        }

        void TimeSpanTextBoxPlanningMinValidated(object sender, EventArgs e)
        {
            if (timeSpanTextBoxPlanningMin.Value <= TimeSpan.FromHours(-100))
                timeSpanTextBoxPlanningMin.SetInitialResolution(TimeSpan.FromHours(-100).Add(TimeSpan.FromMinutes(1)));
            SelectedContract.PlanningTimeBankMin = timeSpanTextBoxPlanningMin.Value;
        }

        void TimeSpanTextBoxPositiveToleranceValidated(object sender, EventArgs e)
        {
            SelectedContract.PositivePeriodWorkTimeTolerance = timeSpanTextBoxPositiveTolerance.Value;
        }

        void TimeSpanTextBoxNegativeToleranceValidated(object sender, EventArgs e)
        {
            SelectedContract.NegativePeriodWorkTimeTolerance = timeSpanTextBoxNegativeTolerance.Value;
        }

		private void TextBoxExtWeeklyRestTimeValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

        private void TextBoxExMinTimeSchedulePeriodValidated(object sender, EventArgs e)
        {
            SelectedContract.MinTimeSchedulePeriod = textBoxExMinTimeSchedulePeriod.Value;
        }

		private void TextBoxExtNightlyRestTimeValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

		private void TextBoxExtMaxTimePerWeekValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTimeDirective = getWorkTimeDirective();
		}

		private void TextBoxExtAvgWorkTimePerDayValidated(object sender, EventArgs e)
		{
			SelectedContract.WorkTime = new WorkTime(textBoxExtAvgWorkTimePerDay.Value);
		}

		private void TextBoxDescriptionValidated(object sender, EventArgs e)
		{
			changeContractDescription();
		}

		private void TextBoxDescriptionValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				validateContractDescription();
			}
		}

        private void TextBoxExMinTimeSchedulePeriodValidating(object sender, CancelEventArgs e)
        {
        	if (SelectedContract == null) return;
        	if (SelectedContract.EmploymentType != EmploymentType.HourlyStaff)
        		textBoxExMinTimeSchedulePeriod.SetInitialResolution(TimeSpan.Zero);
        }

		private void TextBoxExtMaxTimePerWeekValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
                e.Cancel = validateMaxHoursPerWeek();
			}
		}

        private bool validateMaxHoursPerWeek()
        {
            bool cancel = false;

            TimeSpan hoursPerWeek = textBoxExtMaxTimePerWeek.Value;
            if (!validateMaxHours(hoursPerWeek, MaxHoursPerWeek))
            {
                ViewBase.ShowErrorMessage(Resources.MaximumHoursPerWeekCannotBeMoreThan168Hours,
                                   Resources.TimeError);
                cancel = true;
            }

            return cancel;
        }

		private void TextBoxExtNightlyRestTimeValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateNightlyRestTime();
			}
		}

		private void TextBoxExtWeeklyRestTimeValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateWeeklyRestTime();
			}
		}

		private void TextBoxExtAvgWorkTimePerDayValidating(object sender, CancelEventArgs e)
		{
			if (SelectedContract != null)
			{
				e.Cancel = validateAvgWorkTimePerDay();
			}
		}

		private void ButtonNewClick(object sender, EventArgs e)
		{
			//addNewContract();
			if (SelectedContract == null) return;
			Cursor.Current = Cursors.WaitCursor;
			addNewContract();
			Cursor.Current = Cursors.Default;
		}

		private void ButtonDeleteContractClick(object sender, EventArgs e)
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

            if (!validateMaxHours(textBoxExtAvgWorkTimePerDay.Value, MaxHoursPerDay))
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
			if (!validateMaxHours(nightlyRestTime, MaxHoursPerDay))
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
			bool failed = string.IsNullOrEmpty(textBoxDescription.Text);
			if (failed)
			{
				textBoxDescription.SelectedText = SelectedContract.Description.Name;
			}
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

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
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
		    TimeSpan nightRest = textBoxExtNightlyRestTime.Value;
		    TimeSpan weeklyRest = textBoxExtWeeklyRestTime.Value;
			return new WorkTimeDirective(maxTime, nightRest, weeklyRest);
		}

		private static bool validateMaxHours(TimeSpan timeSpan, int maxHours)
		{
			bool isValid = true;

			switch (maxHours)
			{
				case MaxHoursPerDay:
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
				selected = FirstItemIndex;
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
            _multiplicatorDefinitionSets.AddRange(_multiplicatorDefinitionSetRepository.LoadAllSortByName());

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
			return index > InvalidItemIndex && index < _contractList.Count && comboBoxAdvContracts.DataSource != null;
		}

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.Contract; }
        }

        private void CheckedListBoxMultiplicatorDefenitionSetsItemCheck(object sender, ItemCheckEventArgs e)
        {
            var definitionSet = ((CheckedListBox) sender).SelectedItem as IMultiplicatorDefinitionSet;
        	if (definitionSet == null) return;
        	if (e.NewValue == CheckState.Checked)
        		SelectedContract.AddMultiplicatorDefinitionSetCollection(definitionSet);
        	else
        		SelectedContract.RemoveMultiplicatorDefinitionSetCollection(definitionSet);
        }

        private void ComboBoxAdvEmpTypesSelectedIndexChanged(object sender, EventArgs e)
        {
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

        private void checkBoxAdjustTimeBankWithSeasonality_CheckStateChanged(object sender, EventArgs e)
        {
            SelectedContract.AdjustTimeBankWithSeasonality = checkBoxAdjustTimeBankWithSeasonality.Checked;
        }

        private void checkBoxAdjustTimeBankWithPartTimePercentage_CheckStateChanged(object sender, EventArgs e)
        {
            SelectedContract.AdjustTimeBankWithPartTimePercentage = checkBoxAdjustTimeBankWithPartTimePercentage.Checked;
        }

        private void numericUpDownPositiveDayOff_ValueChanged(object sender, EventArgs e)
        {
            SelectedContract.PositiveDayOffTolerance = (int) numericUpDownPositiveDayOff.Value;
        }

        private void numericUpDownNegativeDayOff_ValueChanged(object sender, EventArgs e)
        {
            SelectedContract.NegativeDayOffTolerance = (int)numericUpDownNegativeDayOff.Value;
        }

        private void radioButtonFromContract_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonFromContract.Checked)
            {
                SelectedContract.WorkTimeSource  = WorkTimeSource.FromContract;
            }
        }

        private void radioButtonFromSchedule_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonFromSchedule.Checked)
            {
                SelectedContract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            }
        }
    }
}
