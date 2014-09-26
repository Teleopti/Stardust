﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;
using System.Linq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Optimization
{
	public partial class OptimizationPreferencesDialog : BaseDialogForm, IDataExchange
	{
		private readonly IEventAggregator _eventAggregator;
		public IOptimizationPreferences Preferences { get; private set; }

		private GeneralPreferencesPersonalSettings _defaultGeneralPreferences;
		private DaysOffPreferencesPersonalSettings _defaultDaysOffPreferences;
		private ExtraPreferencesPersonalSettings _defaultExtraPreferences;
		private AdvancedPreferencesPersonalSettings _defaultAdvancedPreferences;
		private ShiftsPreferencesPersonalSettings _defaultshiftsPreferences;

		private IList<IDataExchange> Panels { get; set; }

		private readonly IList<IGroupPageLight> _groupPages;
		private readonly ISchedulerGroupPagesProvider _groupPagesProvider;
		private readonly IEnumerable<IScheduleTag> _scheduleTags;
		private readonly IEnumerable<IActivity> _availableActivity;

		private readonly int _resolution;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IEnumerable<IPerson> _selectedPersons;
		private readonly IToggleManager _toggleManager;
		private readonly IList<IGroupPageLight> _groupPagesForTeamBlockPer;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public OptimizationPreferencesDialog(
			IOptimizationPreferences preferences,
			ISchedulerGroupPagesProvider groupPagesProvider,
			IEnumerable<IScheduleTag> scheduleTags, 
			IEnumerable<IActivity> availableActivity, 
			int resolution,
			IScheduleDictionary scheduleDictionary,
			IEnumerable<IPerson> selectedPersons, 
			IToggleManager toggleManager)
			: this()
		{
			Preferences = preferences;
			_groupPagesProvider = groupPagesProvider;
			_groupPages = _groupPagesProvider.GetGroups(true);
			_groupPagesForTeamBlockPer = _groupPages;
			var singleAgentGroupPage = new GroupPageLight {Key = "SingleAgentTeam", Name = "Single Agent Team"};
			_groupPagesForTeamBlockPer.Add(singleAgentGroupPage);
			_scheduleTags = scheduleTags;
			_availableActivity = availableActivity;
			_resolution = resolution;
			_scheduleDictionary = scheduleDictionary;
			_selectedPersons = selectedPersons;
			_toggleManager = toggleManager;
			_eventAggregator = new EventAggregator();
		}

		private OptimizationPreferencesDialog()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		private void formLoad(object sender, EventArgs e)
		{
			loadPersonalSettings();
			generalPreferencesPanel1.Initialize(Preferences.General, _scheduleTags, _eventAggregator);
			dayOffPreferencesPanel1.Initialize(Preferences.DaysOff);
			extraPreferencesPanel1.Initialize(Preferences.Extra, _groupPagesProvider, _availableActivity, _toggleManager);
			advancedPreferencesPanel1.Initialize(Preferences.Advanced);
			shiftsPreferencesPanel1.Initialize(Preferences.Shifts, _availableActivity, _resolution);
			Panels = new List<IDataExchange> { generalPreferencesPanel1, dayOffPreferencesPanel1, extraPreferencesPanel1, shiftsPreferencesPanel1, advancedPreferencesPanel1 };

			ActiveControl = tabControlTopLevel;
		}


		#region IDataExchange Members

		public bool ValidateData(ExchangeDataOption direction)
		{
			return Panels.All(panel => panel.ValidateData(direction));
		}

		public void ExchangeData(ExchangeDataOption direction)
		{
			Panels.ToList().ForEach(panel => panel.ExchangeData(direction));
		}

		#endregion


		private void loadPersonalSettings()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingRepository = new PersonalSettingDataRepository(uow);
					_defaultGeneralPreferences = settingRepository.FindValueByKey("GeneralPreferencesPersonalSettings", new GeneralPreferencesPersonalSettings());
					_defaultDaysOffPreferences = settingRepository.FindValueByKey("DaysOffPreferencesPersonalSettings", new DaysOffPreferencesPersonalSettings());
					_defaultExtraPreferences = settingRepository.FindValueByKey("ExtraPreferencesPersonalSettings", new ExtraPreferencesPersonalSettings());
					_defaultAdvancedPreferences = settingRepository.FindValueByKey("AdvancedPreferencesPersonalSettings", new AdvancedPreferencesPersonalSettings());
					_defaultshiftsPreferences = settingRepository.FindValueByKey("ShiftsPreferencesPersonalSettings",
																						  new ShiftsPreferencesPersonalSettings());
				}
			}
			catch (DataSourceException)
			{
				// move out silently in case of ex
			}
			if (hasMissedloadingSettings()) return;
			_defaultGeneralPreferences.MapTo(Preferences.General, _scheduleTags);
			_defaultDaysOffPreferences.MapTo(Preferences.DaysOff);
			_defaultExtraPreferences.MapTo(Preferences.Extra, _groupPages, _groupPagesForTeamBlockPer);
			_defaultAdvancedPreferences.MapTo(Preferences.Advanced);
			_defaultshiftsPreferences.MapTo(Preferences.Shifts, _availableActivity);

		}

		private bool hasMissedloadingSettings()
		{
			return _defaultGeneralPreferences == null || _defaultDaysOffPreferences == null || _defaultExtraPreferences == null
				|| _defaultAdvancedPreferences == null || _defaultshiftsPreferences == null;
		}

		private void savePersonalSettings()
		{
			if (hasMissedloadingSettings()) return;
			_defaultGeneralPreferences.MapFrom(Preferences.General);
			_defaultDaysOffPreferences.MapFrom(Preferences.DaysOff);
			_defaultExtraPreferences.MapFrom(Preferences.Extra);
			_defaultAdvancedPreferences.MapFrom(Preferences.Advanced);
			_defaultshiftsPreferences.MapFrom(Preferences.Shifts);
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultGeneralPreferences);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultDaysOffPreferences);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultExtraPreferences);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultAdvancedPreferences);
					uow.PersistAll();
					new PersonalSettingDataRepository(uow).PersistSettingValue(_defaultshiftsPreferences);
					uow.PersistAll();
				}
			}
			catch (DataSourceException)
			{
				// move out silently in case of ex
			}
		}

		private void buttonCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private void buttonOkClick(object sender, EventArgs e)
		{
			if (ValidateData(ExchangeDataOption.ControlsToDataSource))
			{
				if (!extraPreferencesPanel1.ValidateTeamBlockCombination())
				{
					MessageBox.Show(this, UserTexts.Resources.IllegalTeamBlockCombination, UserTexts.Resources.OptimizationOptionMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				if (!validateOptimizationSteps())
				{
					MessageBox.Show(this, UserTexts.Resources.UnsupportedOptimizationSteps, UserTexts.Resources.OptimizationOptionMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				if (!validateTimeBetweenDaysAndSameShift())
				{
					MessageBox.Show(this, UserTexts.Resources.UnSupportedOptimizationBlockAndTimeBetweendays, UserTexts.Resources.OptimizationOptionMessageBox, MessageBoxButtons.OK, MessageBoxIcon.Warning);	
				}

				ExchangeData(ExchangeDataOption.ControlsToDataSource);
				savePersonalSettings();
				if (Preferences.Shifts.KeepShifts || Preferences.DaysOff.UseKeepExistingDaysOff)
				{
					var clonedRanges = new Dictionary<IPerson, IScheduleRange>();
					foreach (var selectedPerson in _selectedPersons)
					{
						var cloneRange = (IScheduleRange)_scheduleDictionary[selectedPerson].Clone();
						clonedRanges.Add(selectedPerson, cloneRange);
					}

					Preferences.Rescheduling.AllSelectedScheduleRangeClones = clonedRanges;
				}

				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private bool validateOptimizationSteps()
		{
			if (extraPreferencesPanel1.IsTeamOrBlockChecked() && generalPreferencesPanel1.IsOptimizationStepsChecked())
				return false;

			return true;
		}

		private bool validateTimeBetweenDaysAndSameShift()
		{
			if (generalPreferencesPanel1.IsTimeBetweenDaysChecked() && extraPreferencesPanel1.IsSameShiftChecked())
				return false;

			return true;
		}

		private void tabControlTopLevelSelectedIndexChanged(object sender, EventArgs e)
		{
			setupHelpContext(tabControlTopLevel.SelectedTab.Controls[0]);
		}

		private void optimizationPreferencesDialogFormClosing(object sender, FormClosingEventArgs e)
		{
			if (generalPreferencesPanel1 != null) generalPreferencesPanel1.UnsubscribeEvents();
		}

		private void setupHelpContext(Control control)
		{
			RemoveControlHelpContext(control);
			AddControlHelpContext(control);
		}
	}
}
