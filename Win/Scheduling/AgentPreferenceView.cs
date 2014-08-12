using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class AgentPreferenceView : BaseDialogForm, IAgentPreferenceView
	{
		private readonly IAgentPreferenceDayCreator _dayCreator;
		private readonly AgentPreferencePresenter _presenter;
		private bool _isInitialized;
		private readonly ISchedulerStateHolder _schedulerStateHolder;

		public AgentPreferenceView(IScheduleDay scheduleDay, ISchedulerStateHolder schedulerStateHolder)
		{
			InitializeComponent();
			SetTexts();
			toolTip.SetToolTip(checkBoxAdvShiftCategoryNextDayMin, Resources.NextDay);
			toolTip.SetToolTip(checkBoxAdvShiftCategoryNextDayMax, Resources.NextDay);
			_dayCreator = new AgentPreferenceDayCreator();
			_schedulerStateHolder = schedulerStateHolder;
			_presenter = new AgentPreferencePresenter(this, scheduleDay, schedulerStateHolder.SchedulingResultState);
		}

		public void UpdateShiftCategory(IShiftCategory shiftCategory)
		{
			if (shiftCategory == null) return;

			ComboBoxAdvShiftCategory currentCategory = null;

			foreach (var item in comboBoxAdvShiftCategory.Items)
			{
				var comboItem  = item as ComboBoxAdvShiftCategory;
				if (comboItem == null || comboItem.Id != shiftCategory.Id) continue;
				currentCategory = comboItem;
				break;
			}

			if (currentCategory != null)
			{
				comboBoxAdvShiftCategory.SelectedItem = currentCategory;
			}
			
		}

		public void UpdateAbsence(IAbsence absence)
		{
			if (absence == null) return;

			Absence currentAbsence = null;

			foreach (var item in comboBoxAdvAbsence.Items)
			{
				var comboItem = item as Absence;
				if (comboItem == null || comboItem.Id != absence.Id) continue;
				currentAbsence = comboItem;
				break;
			}

			if (currentAbsence != null)
			{
				comboBoxAdvAbsence.SelectedItem = currentAbsence;
			}
		}

		public void UpdateDayOff(IDayOffTemplate dayOffTemplate)
		{
			if (dayOffTemplate == null) return;

			ComboBoxAdvDayOffTemplate currentDayOff = null;

			foreach (var item in comboBoxAdvDayOff.Items)
			{
				var comboItem = item as ComboBoxAdvDayOffTemplate;
				if (comboItem == null ||  comboItem.Id != dayOffTemplate.Id) continue;
				currentDayOff = comboItem;
				break;
			}

			if (currentDayOff != null)
			{
				comboBoxAdvDayOff.SelectedItem = currentDayOff;
			}
		}

		public void UpdateActivity(IActivity activity)
		{
			if (activity == null) return;

			Activity currentActivity = null;

			foreach (var item in comboBoxAdvActivity.Items)
			{
				var comboItem = item as Activity;
				if (comboItem == null ||  comboItem.Id != activity.Id) continue;
				currentActivity = comboItem;
				break;
			}

			if (currentActivity != null)
			{
				comboBoxAdvActivity.SelectedItem = currentActivity;
				if (currentActivity.Id != null)
				{
					EnableActivityTimes(true);
				}
			}
		}

		public void UpdateActivityTimes(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			outlookTimePickerActivityLengthMin.SetTimeValue(minLength);
			outlookTimePickerActivityLengthMax.SetTimeValue(maxLength);
			outlookTimePickerActivityStartMin.SetTimeValue(minStart);
			outlookTimePickerActivityStartMax.SetTimeValue(maxStart);
			outlookTimePickerActivityEndMin.SetTimeValue(minEnd);
			outlookTimePickerActivityEndMax.SetTimeValue(maxEnd);	
		}

		public void UpdateTimesExtended(TimeSpan? minLength, TimeSpan? maxLength, TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd)
		{
			outlookTimePickerContractShiftCategoryMin.SetTimeValue(minLength);
			outlookTimePickerContractShiftCategoryMax.SetTimeValue(maxLength);
			outlookTimePickerShiftCategoryStartMin.SetTimeValue(minStart);
			outlookTimePickerShiftCategoryStartMax.SetTimeValue(maxStart);
			
			if (minEnd.HasValue && minEnd.Value.Days > 0)
			{
				minEnd = minEnd.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvShiftCategoryNextDayMin.Checked = true;
			}
			else
			{
				checkBoxAdvShiftCategoryNextDayMin.Checked = false;
			}
			
			outlookTimePickerShiftCategoryEndMin.SetTimeValue(minEnd);

			if (maxEnd.HasValue && maxEnd.Value.Days > 0)
			{
				maxEnd = maxEnd.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvShiftCategoryNextDayMax.Checked = true;
			}
			else
			{
				checkBoxAdvShiftCategoryNextDayMax.Checked = false;
			}

			outlookTimePickerShiftCategoryEndMax.SetTimeValue(maxEnd);
		}

		public void UpdateMustHave(bool mustHave)
		{
			checkBoxMustHave.Checked = mustHave;	
		}

		public void UpdateMustHaveText(string text)
		{
			checkBoxMustHave.Text = text;	
		}

		private void agentPreferenceViewLoad(object sender, EventArgs e)
		{
			tabControlAgentInfo.SelectedIndex = 2;
			tabControlAgentInfo.SelectedIndex = 1;
			tabControlAgentInfo.SelectedIndex = 0;

			if (!hasExtended())
			{
				tabControlAgentInfo.TabPages[2].Hide();
				tabControlAgentInfo.TabPages[1].Hide();
			}

			_presenter.UpdateView();
			_isInitialized = true;
		}

		private static bool hasExtended()
		{
			var licensedFunctions = (from o in LicenseSchema.GetActiveLicenseSchema(UnitOfWorkFactory.Current.Name).LicenseOptions
									from f in o.EnabledApplicationFunctions
									where (o.Enabled && f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ModifyExtendedPreferences)
									select f).ToList();

			return licensedFunctions.Count != 0;
		}

		public void PopulateShiftCategories()
		{
			var comboCategories = new List<ComboBoxAdvShiftCategory>();
			var shiftCategories = (from sc in _schedulerStateHolder.CommonStateHolder.ShiftCategories where !((IDeleteTag)sc).IsDeleted select sc).ToList();

			foreach (var shiftCategory in shiftCategories)
			{
				comboCategories.Add((new ComboBoxAdvShiftCategory(shiftCategory)));	
			}

			var currentPreferenceRestriction = _presenter.PreferenceRestriction();
			if (currentPreferenceRestriction != null && currentPreferenceRestriction.ShiftCategory != null && !shiftCategories.Contains(currentPreferenceRestriction.ShiftCategory))
			{
				comboCategories.Add(new ComboBoxAdvShiftCategory(currentPreferenceRestriction.ShiftCategory));	
			}

			var sortedCategories = (from c in comboCategories orderby c.Name select c).ToList();
			var noneCategory = new ShiftCategory(Resources.None);
			sortedCategories.Insert(0, new ComboBoxAdvShiftCategory(noneCategory));

			comboBoxAdvShiftCategory.DisplayMember = "Name";
			comboBoxAdvShiftCategory.ValueMember = "Id";
			comboBoxAdvShiftCategory.DataSource = sortedCategories;
		}

		public void PopulateAbsences()
		{
			var absences = (from a in _schedulerStateHolder.CommonStateHolder.Absences where !((IDeleteTag)a).IsDeleted select a).ToList();

			var currentPreferenceRestriction = _presenter.PreferenceRestriction();
			if (currentPreferenceRestriction != null && currentPreferenceRestriction.Absence != null && !absences.Contains(currentPreferenceRestriction.Absence))
			{
				absences.Add(currentPreferenceRestriction.Absence);
			}

			var sortedAbsences = (from a in absences orderby a.Name select a).ToList();
			var noneAbsence = new Absence { Description = new Description(Resources.None) };
			sortedAbsences.Insert(0, noneAbsence);

			comboBoxAdvAbsence.DisplayMember = "Name";
			comboBoxAdvAbsence.ValueMember = "Id";
			comboBoxAdvAbsence.DataSource = sortedAbsences;	
		}

		public void PopulateDayOffs()
		{
			var comboDayOffs = new List<ComboBoxAdvDayOffTemplate>();
			var dayOffs = (from d in _schedulerStateHolder.CommonStateHolder.DayOffs where !((IDeleteTag)d).IsDeleted select d).ToList();

			foreach (var dayOff in dayOffs)
			{
				comboDayOffs.Add((new ComboBoxAdvDayOffTemplate(dayOff)));
			}

			var currentPreferenceRestriction = _presenter.PreferenceRestriction();
			if (currentPreferenceRestriction != null && currentPreferenceRestriction.DayOffTemplate != null && !dayOffs.Contains(currentPreferenceRestriction.DayOffTemplate))
			{
				comboDayOffs.Add(new ComboBoxAdvDayOffTemplate(currentPreferenceRestriction.DayOffTemplate));
			}

			var sortedDayOffs = (from d in comboDayOffs orderby d.Name select d).ToList();
			var noneDayOff = new DayOffTemplate(new Description(Resources.None));
			sortedDayOffs.Insert(0, new ComboBoxAdvDayOffTemplate(noneDayOff));

			comboBoxAdvDayOff.DisplayMember = "Name";
			comboBoxAdvDayOff.ValueMember = "Id";
			comboBoxAdvDayOff.DataSource = sortedDayOffs;
		
		}

		public void PopulateActivities()
		{
			var activities = (from a in _schedulerStateHolder.CommonStateHolder.Activities where !(a).IsDeleted select a).ToList();

			var currentPreferenceRestriction = _presenter.PreferenceRestriction();
			if (currentPreferenceRestriction != null)
			{
				var activityRestriction = currentPreferenceRestriction.ActivityRestrictionCollection.FirstOrDefault();
				if (activityRestriction != null && !activities.Contains(activityRestriction.Activity))
				{
					activities.Add(activityRestriction.Activity);		
				}
			}

			var noneActivity = new Activity(Resources.None);
			activities.Insert(0, noneActivity);

			comboBoxAdvActivity.DisplayMember = "Name";
			comboBoxAdvActivity.ValueMember = "Id";
			comboBoxAdvActivity.DataSource = activities;
		}

		public void ClearShiftCategory()
		{
			comboBoxAdvShiftCategory.SelectedIndex = 0;
		}

		public void ClearShiftCategoryExtended()
		{
			outlookTimePickerContractShiftCategoryMin.SetTimeValue(null);
			outlookTimePickerContractShiftCategoryMax.SetTimeValue(null);
			outlookTimePickerShiftCategoryStartMin.SetTimeValue(null);
			outlookTimePickerShiftCategoryStartMax.SetTimeValue(null);
			outlookTimePickerShiftCategoryEndMin.SetTimeValue(null);
			outlookTimePickerShiftCategoryEndMax.SetTimeValue(null);
			checkBoxAdvShiftCategoryNextDayMax.Checked = false;
			checkBoxAdvShiftCategoryNextDayMin.Checked = false;

			clearTimeErrorMessagesExtended();
		}

		public void ClearAbsence()
		{
			comboBoxAdvAbsence.SelectedIndex = 0;
		}

		public void ClearDayOff()
		{
			comboBoxAdvDayOff.SelectedIndex = 0;
		}

		public void ClearActivity()
		{
			comboBoxAdvActivity.SelectedIndex = 0;
			outlookTimePickerActivityLengthMin.SetTimeValue(null);
			outlookTimePickerActivityLengthMax.SetTimeValue(null);
			outlookTimePickerActivityStartMin.SetTimeValue(null);
			outlookTimePickerActivityStartMax.SetTimeValue(null);
			outlookTimePickerActivityEndMin.SetTimeValue(null);
			outlookTimePickerActivityEndMax.SetTimeValue(null);

			EnableActivityTimes(false);
			clearTimeErrorMessagesActivity();
		}

		public void EnableActivityTimes(bool enable)
		{
			outlookTimePickerActivityLengthMin.Enabled = enable;
			outlookTimePickerActivityLengthMax.Enabled = enable;
			outlookTimePickerActivityStartMin.Enabled = enable;
			outlookTimePickerActivityStartMax.Enabled = enable;
			outlookTimePickerActivityEndMin.Enabled = enable;
			outlookTimePickerActivityEndMax.Enabled = enable;	
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			var data = viewData();
			var result = validateTimes(data);
			var commandToExecute = _presenter.CommandToExecute(data, _dayCreator);

			if (commandToExecute != null)
			{
				_presenter.RunCommand(commandToExecute);
				Hide();
				return;
			}
			
			if (result.ExtendedTimesError)
			{
				tabControlAgentInfo.SelectedIndex = 1;
				return;
			}

			if (result.ActivityTimesError)
			{
				tabControlAgentInfo.SelectedIndex = 2;
				return;
			}

			Hide();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Hide();
		}

		private IAgentPreferenceCanCreateResult validateTimes(IAgentPreferenceData data)
		{

			clearTimeErrorMessagesActivity();
			clearTimeErrorMessagesExtended();

			var result = _dayCreator.CanCreate(data);

			if (!result.Result && !result.EmptyError)
			{
				if (result.StartTimeMinError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMin, Resources.MustSpecifyValidTime);
				if (result.StartTimeMaxError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMax, Resources.MustSpecifyValidTime);
				if (result.LengthMinError) setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMin, Resources.MustSpecifyValidTime);
				if (result.LengthMaxError) setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMax, Resources.MustSpecifyValidTime);
				if (result.EndTimeMinError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMin, Resources.MustSpecifyValidTime);
				if (result.EndTimeMaxError) setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMax, Resources.MustSpecifyValidTime);

				if (result.StartTimeMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityStartMin, Resources.MustSpecifyValidTime);
				if (result.StartTimeMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityStartMax, Resources.MustSpecifyValidTime);
				if (result.LengthMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityLengthMin, Resources.MustSpecifyValidTime);
				if (result.LengthMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityLengthMax, Resources.MustSpecifyValidTime);
				if (result.EndTimeMinErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityEndMin, Resources.MustSpecifyValidTime);
				if (result.EndTimeMaxErrorActivity) setTimeErrorMessageActivity(outlookTimePickerActivityEndMax, Resources.MustSpecifyValidTime);
			}

			return result;
		}

		private void clearTimeErrorMessagesActivity()
		{
			setTimeErrorMessageActivity(outlookTimePickerActivityLengthMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityLengthMax, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityStartMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityStartMax, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityEndMin, null);
			setTimeErrorMessageActivity(outlookTimePickerActivityEndMax, null);
		}

		private void clearTimeErrorMessagesExtended()
		{
			setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMin,null);
			setTimeErrorMessageExtended(outlookTimePickerContractShiftCategoryMax,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMin,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryStartMax,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMin,null);
			setTimeErrorMessageExtended(outlookTimePickerShiftCategoryEndMax, null);
		}

		private bool timesActivityError()
		{
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityLengthMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityLengthMax))) return true;
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityStartMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityStartMax))) return true;
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityEndMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderActivity.GetError(outlookTimePickerActivityEndMax))) return true;

			return false;
		}

		private bool timesExtendedError()
		{
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerContractShiftCategoryMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerContractShiftCategoryMax))) return true;
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryStartMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryStartMax))) return true;
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryEndMin))) return true;
			if (!string.IsNullOrEmpty(errorProviderExtended.GetError(outlookTimePickerShiftCategoryEndMax))) return true;

			return false;
		}

		private void setTimeErrorMessageExtended(Control timeControl, string value)
		{
			errorProviderExtended.SetIconAlignment(timeControl, ErrorIconAlignment.MiddleLeft);
			errorProviderExtended.SetIconPadding(timeControl, 2);
			errorProviderExtended.SetError(timeControl, value);
		}

		private void setTimeErrorMessageActivity(Control timeControl, string value)
		{
			errorProviderActivity.SetIconAlignment(timeControl, ErrorIconAlignment.MiddleLeft);
			errorProviderActivity.SetIconPadding(timeControl, 2);
			errorProviderActivity.SetError(timeControl, value);
		}

		private void comboBoxAdvShiftCategorySelectedIndexChanged(object sender, EventArgs e)
		{
			var comboShiftCategory = comboBoxAdvShiftCategory.SelectedItem as ComboBoxAdvShiftCategory;
			if (comboShiftCategory == null || comboShiftCategory.Id == null) return;
			ClearAbsence();
			ClearDayOff();
		}

		private void comboBoxAdvAbsenceSelectedIndexChanged(object sender, EventArgs e)
		{
			var currentAbsence = comboBoxAdvAbsence.SelectedItem as Absence;
			if (currentAbsence == null || currentAbsence.Id == null) return;
			ClearShiftCategory();
			ClearShiftCategoryExtended();
			ClearDayOff();
			ClearActivity();
		}

		private void comboBoxAdvDayOffSelectedIndexChanged(object sender, EventArgs e)
		{
			var comboDayOff = comboBoxAdvDayOff.SelectedItem as ComboBoxAdvDayOffTemplate;
			if (comboDayOff == null || comboDayOff.Id == null) return;
			ClearShiftCategory();
			ClearShiftCategoryExtended();
			ClearAbsence();
			ClearActivity();
		}

		private void comboBoxAdvActivitySelectedIndexChanged(object sender, EventArgs e)
		{
			var comboActivity = comboBoxAdvActivity.SelectedItem as Activity;
			if (comboActivity != null && comboActivity.Id != null)
			{
				ClearAbsence();
				ClearDayOff();
				EnableActivityTimes(true);
			}
			else
			{
				ClearActivity();
			}
		}

		private void outlookTimePickerContractShiftCategoryMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();	
		}

		private void outlookTimePickerContractShiftCategoryMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerShiftCategoryEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityLengthMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityStartMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMinTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void outlookTimePickerActivityEndMaxTextChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesActivityError()) validateTimes(viewData());
			ClearAbsence();
			ClearDayOff();
		}

		private void checkBoxAdvShiftCategoryNextDayMinCheckStateChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
		}

		private void checkBoxAdvShiftCategoryNextDayMaxCheckStateChanged(object sender, EventArgs e)
		{
			if (!_isInitialized) return;
			if (timesExtendedError()) validateTimes(viewData());
		}

		private AgentPreferenceData viewData()
		{
			var minStart = outlookTimePickerShiftCategoryStartMin.TimeValue();
			var maxStart = outlookTimePickerShiftCategoryStartMax.TimeValue();
			var minEnd = outlookTimePickerShiftCategoryEndMin.TimeValue();
			var maxEnd = outlookTimePickerShiftCategoryEndMax.TimeValue();
			var minLength = outlookTimePickerContractShiftCategoryMin.TimeValue();
			var maxLength = outlookTimePickerContractShiftCategoryMax.TimeValue();

			if (checkBoxAdvShiftCategoryNextDayMin.Checked && minEnd != null)
				minEnd = minEnd.Value.Add(TimeSpan.FromDays(1));

			if (checkBoxAdvShiftCategoryNextDayMax.Checked && maxEnd != null)
				maxEnd = maxEnd.Value.Add(TimeSpan.FromDays(1));


			var minStartActivity = outlookTimePickerActivityStartMin.TimeValue();
			var maxStartActivity = outlookTimePickerActivityStartMax.TimeValue();
			var minEndActivity = outlookTimePickerActivityEndMin.TimeValue();
			var maxEndActivity = outlookTimePickerActivityEndMax.TimeValue();
			var minLengthActivity = outlookTimePickerActivityLengthMin.TimeValue();
			var maxLengthActivity = outlookTimePickerActivityLengthMax.TimeValue();

			var comboShiftCategory = comboBoxAdvShiftCategory.SelectedItem as ComboBoxAdvShiftCategory;
			var currentShiftCategory = comboShiftCategory.ShiftCategory;
			if (currentShiftCategory.Id == null)
			{
				currentShiftCategory = null;
			}

			var currentAbsence = comboBoxAdvAbsence.SelectedItem as IAbsence;
			if (currentAbsence.Id == null)
			{
				currentAbsence = null;
			}

			var comboDayOff = comboBoxAdvDayOff.SelectedItem as ComboBoxAdvDayOffTemplate;
			var currentDayOff = comboDayOff.DayOffTemplate;
			if (currentDayOff.Id == null)
			{
				currentDayOff = null;
			}

			var currentActivity = comboBoxAdvActivity.SelectedItem as IActivity;
			if (currentActivity.Id == null)
			{
				currentActivity = null;
			}

			var mustHave = checkBoxMustHave.Checked;

			var data = new AgentPreferenceData
				{
					ShiftCategory = currentShiftCategory,
					Absence = currentAbsence,
					DayOffTemplate = currentDayOff,
					Activity = currentActivity,
					MinStart = minStart,
					MaxStart = maxStart,
					MinEnd = minEnd,
					MaxEnd = maxEnd,
					MinLength = minLength,
					MaxLength = maxLength,
					MinStartActivity = minStartActivity,
					MaxStartActivity = maxStartActivity,
					MinEndActivity = minEndActivity,
					MaxEndActivity = maxEndActivity,
					MinLengthActivity = minLengthActivity,
					MaxLengthActivity = maxLengthActivity,
					MustHave = mustHave
				};

			return data;
		}
	}

	class ComboBoxAdvShiftCategory
	{
		private readonly IShiftCategory _shiftCategory;

		private ComboBoxAdvShiftCategory() { }

		public ComboBoxAdvShiftCategory(IShiftCategory shiftCategory) : this()
		{
			_shiftCategory = shiftCategory;
		}

		public string Name
		{
			get { return _shiftCategory.Description.Name; }
		}

		public Guid? Id
		{
			get { return _shiftCategory.Id; }
		}

		public IShiftCategory ShiftCategory
		{
			get { return _shiftCategory; }
		}
	}

	class ComboBoxAdvDayOffTemplate
	{
		private readonly IDayOffTemplate _dayOffTemplate;

		private ComboBoxAdvDayOffTemplate() { }

		public ComboBoxAdvDayOffTemplate(IDayOffTemplate dayOffTemplate) : this()
		{
			_dayOffTemplate = dayOffTemplate;
		}

		public string Name
		{
			get { return _dayOffTemplate.Description.Name; }
		}

		public Guid? Id
		{
			get { return _dayOffTemplate.Id; }
		}

		public IDayOffTemplate DayOffTemplate
		{
			get { return _dayOffTemplate; }
		}
	}

}
