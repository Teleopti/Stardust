using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class FormAgentInfo : BaseRibbonForm
    {
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;
    	private readonly ILifetimeScope _container;
    	private IPerson _selectedPerson;
        private ICollection<DateOnly> _dateOnlyList;
        private ISchedulingResultStateHolder _stateHolder;
        private bool _dateIsSelected;
        private IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
		private readonly IDictionary<EmploymentType, string> _employmentTypeList;
        private IList<IOptionalColumn> _optionalColumns;
    	private readonly IDictionary<SchedulePeriodType, string> _schedulePeriodTypeList;
        private readonly ISchedulerGroupPagesProvider _groupPagesProvider;
        private IList<IGroupPageLight> _groupPages;
        private bool _dataLoaded;
    	private IShiftCategoryFairnessAggregateManager _fairnessManager;

    	public FormAgentInfo()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();

            TopLevel = true;

			_employmentTypeList = LanguageResourceHelper.TranslateEnum<EmploymentType>();
			_schedulePeriodTypeList = LanguageResourceHelper.TranslateEnum<SchedulePeriodType>();
        }

		public FormAgentInfo(IWorkShiftWorkTime workShiftWorkTime, ILifetimeScope container, ISchedulerGroupPagesProvider groupPagesProvider)
			: this()
		{
			_workShiftWorkTime = workShiftWorkTime;
			_container = container;
		    _groupPagesProvider = groupPagesProvider;
		}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void UpdateData(IDictionary<IPerson, IScheduleRange> personDictionary, 
            ICollection<DateOnly> dateOnlyList, ISchedulingResultStateHolder stateHolder,
            IDictionary<IPerson, IPersonAccountCollection> allAccounts)
        {
            if (personDictionary.Values.Count == 0 || dateOnlyList.Count == 0)
            {
                _dateIsSelected = false;
                listViewSchedulePeriod.Items.Clear();
                listViewPersonPeriod.Items.Clear();
                listViewRestrictions.Items.Clear();
                listViewPerson.Items.Clear();
                return;
            }
            _dateIsSelected = true;
            _selectedPerson = personDictionary.Values.ToList()[0].Person;
            _dateOnlyList = dateOnlyList;
            _stateHolder = stateHolder;
            _allAccounts = allAccounts;
        	_optionalColumns = _stateHolder.OptionalColumns;
            update();
        }

        private void update()
        {
	        timerRefresh.Enabled = false;

            if (tabControlAgentInfo.SelectedTab == tabPageAdvSchedulePeriod && _dateIsSelected)
            {
                updateSchedulePeriodData(_selectedPerson, _dateOnlyList.First(), _stateHolder);
                return;
            }

            if (tabControlAgentInfo.SelectedTab == tabPageAdvPersonPeriod && _dateIsSelected)
            {
                updatePersonPeriodData(_selectedPerson, _dateOnlyList.First());
                return;
            }

            if (tabControlAgentInfo.SelectedTab == tabPageAdvRestrictions && _dateIsSelected)
            {
                updateRestrictionData(_selectedPerson, _dateOnlyList.First(), _stateHolder);
                return;
            }
            if (tabControlAgentInfo.SelectedTab == tabPageAdvPerson && _dateIsSelected)
            {
				timerRefresh.Enabled = true;
                updatePersonInfo(_selectedPerson);
                return;
            }
            
            if(tabControlAgentInfo.SelectedTab == tabPageFairness && _dateIsSelected)
            {
                updateFairnessInfo(_selectedPerson, _dateOnlyList.First(), _stateHolder);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void updateFairnessInfo(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder stateHolder)
		{
			agentGroupPageLabel.Text = Resources.GroupPage;

			if (_fairnessManager == null)
				_fairnessManager = _container.Resolve<IShiftCategoryFairnessAggregateManager>();

			ISchedulingOptions schedulingOptions = new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseStudentAvailability = true,
				UseRotations = true
			};
			var helper = new AgentInfoHelper(person, dateOnly, stateHolder, schedulingOptions, _workShiftWorkTime);
			perPersonAndGroupListView.Items.Clear();

			helper.SchedulePeriodData();
			if (!helper.Period.HasValue)
				return;


			var period = person.Period(helper.SelectedDate);
			if (period != null)
			{
				var employmentType = period.PersonContract.Contract.EmploymentType;
				if (employmentType != EmploymentType.HourlyStaff)
				{
					createAndAddItem(perPersonAndGroupListView, Resources.PreferenceFulfillment, helper.PreferenceFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(perPersonAndGroupListView, Resources.MustHaveFulfillment, helper.MustHavesFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(perPersonAndGroupListView, Resources.RotationFulfillment, helper.RotationFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(perPersonAndGroupListView, Resources.AvailabilityFulfillment, helper.AvailabilityFulfillment.ToString(CultureInfo.CurrentCulture), 2);
				}
				else
				{
					createAndAddItem(perPersonAndGroupListView, Resources.StudentAvailabilityFulfillment, helper.StudentAvailabilityFulfillment.ToString(CultureInfo.CurrentCulture), 2);
				}
			}

			var pointPerShiftCategoryOption = !(person.WorkflowControlSet != null && person.WorkflowControlSet.UseShiftCategoryFairness);

			if (pointPerShiftCategoryOption)
			{
				perPersonAndGroupListView.Items.Add("");
				createAndAddItem(perPersonAndGroupListView, Resources.FairnessValue,
								 ((int)
								  stateHolder.Schedules[person].CachedShiftCategoryFairness().FairnessValueResult.
									  FairnessPoints).ToString(CultureInfo.CurrentCulture), 2);
			}

			perPersonAndGroupListView.Items.Add("");
			perPersonAndGroupListView.Items.Add("");
			perPersonAndGroupListView.Items.Add("");

			if (person.WorkflowControlSet != null && person.WorkflowControlSet.UseShiftCategoryFairness)
			{
				var groupPage = comboBoxAgentGrouping.SelectedItem as IGroupPageLight;
				var perPersonAndGroup = _fairnessManager.GetPerPersonAndGroup(person, groupPage, dateOnly);
				var perGroupAndOthers = _fairnessManager.GetPerGroupAndOtherGroup(person, groupPage, dateOnly);
				var shiftCategoryFairness = _stateHolder.Schedules[person].CachedShiftCategoryFairness();
				
				var categories =
					new List<IShiftCategory>(shiftCategoryFairness.ShiftCategoryFairnessDictionary.Keys);
				categories.Sort(new ShiftCategorySorter());

				var fairnessHelper = new ShiftCategoryFairnessValueHelper(perPersonAndGroup.ShiftCategoryFairnessCompareValues, perGroupAndOthers.ShiftCategoryFairnessCompareValues);

				if (perPersonAndGroup.ShiftCategoryFairnessCompareValues != null || perGroupAndOthers.ShiftCategoryFairnessCompareValues != null)
				{
					var displayHeaders = shouldHeadersBeDisplayedOrNot(perPersonAndGroup.ShiftCategoryFairnessCompareValues) || shouldHeadersBeDisplayedOrNot(perGroupAndOthers.ShiftCategoryFairnessCompareValues); 

					if (displayHeaders)
						createAndAddItemInMultipleColumns(perPersonAndGroupListView, ""
														  , Resources.Agent
														  , Resources.Others
														  , Resources.Team
														  , Resources.Others
														  , true);


					foreach (var shiftCategory in fairnessHelper.ShiftCategories())
					{
						var agentShiftCategory = fairnessHelper.AgentValue(shiftCategory);
						var teamShiftCategory = fairnessHelper.TeamValue(shiftCategory);

						if (!(agentShiftCategory.Original == 0.0 && agentShiftCategory.ComparedTo == 0.0) || !(teamShiftCategory.Original == 0.0 && teamShiftCategory.ComparedTo == 0.0))
						{
							var originalFairnessAgent = Math.Round((agentShiftCategory.Original * 100), 2).ToString(CultureInfo.CurrentCulture) + "%";
							var othersFairnessAgent = Math.Round((agentShiftCategory.ComparedTo * 100), 2).ToString(CultureInfo.CurrentCulture) + "%";

							var originalFairnessTeam = Math.Round((teamShiftCategory.Original * 100), 2).ToString(CultureInfo.CurrentCulture) + "%";
							var othersFairnessTeam = Math.Round((teamShiftCategory.ComparedTo * 100), 2).ToString(CultureInfo.CurrentCulture) + "%";

							createAndAddItemInMultipleColumns(perPersonAndGroupListView,
															  shiftCategory.Description.Name,
															  originalFairnessAgent,
															  othersFairnessAgent,
															  originalFairnessTeam,
															  othersFairnessTeam,
															  false);
						}
					}


					if (displayHeaders)
						createAndAddItemInMultipleColumns(perPersonAndGroupListView, Resources.StandardDeviation
														  , Math.Round(perPersonAndGroup.StandardDeviation, 4).ToString(CultureInfo.CurrentCulture)
														  , ""
														  , Math.Round(perGroupAndOthers.StandardDeviation, 4).ToString(CultureInfo.CurrentCulture)
														  , ""
														  , true);
				}
	
			}
		}

        private static bool shouldHeadersBeDisplayedOrNot(IEnumerable<IShiftCategoryFairnessCompareValue> shiftCategoryFairnessCompareValues)
        {
			if (shiftCategoryFairnessCompareValues == null) return false;
	        return shiftCategoryFairnessCompareValues.Any(shiftCategory => !(shiftCategory.Original == 0.0 && shiftCategory.ComparedTo == 0.0));
        }

        private static void createAndAddItemInMultipleColumns(ListView listView, string column1Text, string column2Text, string column3Text, string column4Text, string column5Text, bool isBold )
        {
            var listViewItems = new ListViewItem(column1Text);
            listViewItems.SubItems.Add(column2Text);
            listViewItems.SubItems.Add(column3Text);
			listViewItems.SubItems.Add(column4Text);
			listViewItems.SubItems.Add(column5Text);

            if(isBold)
            {
                listViewItems.Font = listViewItems.Font.ChangeToBold();
            }
            
            listView.Items.Add(listViewItems);
        }

        private void initializeFairnessTab()
        {
            _groupPages = _groupPagesProvider.GetGroups(false);
            comboBoxAgentGrouping.DataSource = _groupPages;
            comboBoxAgentGrouping.DisplayMember = "Name";
            comboBoxAgentGrouping.ValueMember = "Key";
            _dataLoaded = true;
        }

        private void updateRestrictionData(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
        {
            var extractor = new RestrictionExtractor(state);
            extractor.Extract(person, dateOnly);
            listViewRestrictions.Items.Clear();

            var item = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
            item.Font = item.Font.ChangeToBold();
            listViewRestrictions.Items.Add(item);

            createAndAddItem(listViewRestrictions, Resources.Availability, "", 1);
            handleAvailabilities(extractor.AvailabilityList);

            createAndAddItem(listViewRestrictions, Resources.Rotations, "", 1);
            handleRotations(extractor.RotationList);
            createAndAddItem(listViewRestrictions, Resources.StudentAvailability, "", 1);
            handleStudentAvailabilities(extractor.StudentAvailabilityList);

	        if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities))
	        {
		        createAndAddItem(listViewRestrictions, Resources.OvertimeAvailability, "", 1);
		        var scheduleDay = getScheduleDay(person, dateOnly, state);
		        if (scheduleDay != null) handleOvertimeAvailabilities(scheduleDay.OvertimeAvailablityCollection());
	        }

	        createAndAddItem(listViewRestrictions, Resources.Preference, "", 1);
            handlePreferences(extractor.PreferenceList);
            createAndAddItem(listViewRestrictions, Resources.ShiftCategoryLimitations, "", 1);
            handleShiftCategoryLimitations(person, dateOnly);

            listViewRestrictions.Items.Add("");
            IPersonAccountCollection personAbsenceAccounts;
            if (!_allAccounts.TryGetValue(person, out personAbsenceAccounts))
                return;
            foreach (IPersonAbsenceAccount personAbsenceAccount in personAbsenceAccounts)
            {
                createAndAddItem(listViewRestrictions, personAbsenceAccount.Absence.Description.Name, personAbsenceAccount.Absence.Tracker.Description.Name, 1);
                foreach (var account in personAbsenceAccount.AccountCollection())
                {
                    if (account.GetType() == typeof(AccountDay))
                        updateDayAccount(account);
                    else
                    {
                        updateTimeAccount(account);
                    }

                    listViewRestrictions.Items.Add("");
                }
            }
        }

        private void updateTimeAccount(IAccount account)
        {
        	createAndAddItem(listViewRestrictions, Resources.Period, account.Period().DateString, 1);
            createAndAddItem(listViewRestrictions, Resources.BalanceIn,
                             TimeHelper.GetLongHourMinuteSecondTimeString(account.BalanceIn,
                                                                          CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Accrued,
                             TimeHelper.GetLongHourMinuteSecondTimeString(account.Accrued,
                                                                          CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Extra,
                             TimeHelper.GetLongHourMinuteSecondTimeString(account.Extra,
                                                                          CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.BalanceOut,
                             TimeHelper.GetLongHourMinuteSecondTimeString(account.BalanceOut,
                                                                          CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Used,
                             TimeHelper.GetLongHourMinuteSecondTimeString(account.LatestCalculatedBalance,
                                                                          CultureInfo.InvariantCulture), 2);
            ListViewItem item = createAndAddItem(listViewRestrictions, Resources.Remaining,
                                                 TimeHelper.GetLongHourMinuteSecondTimeString(account.Remaining,
                                                                                              CultureInfo.InvariantCulture), 2);
            if (account.IsExceeded)
                item.ForeColor = Color.Red;
        }


        private void updateDayAccount(IAccount account)
        {
        	createAndAddItem(listViewRestrictions, Resources.Period, account.Period().DateString, 1);
            createAndAddItem(listViewRestrictions, Resources.BalanceIn,
                             account.BalanceIn.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Accrued,
                             account.Accrued.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Extra,
                             account.Extra.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.Used,
                             account.LatestCalculatedBalance.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            createAndAddItem(listViewRestrictions, Resources.BalanceOut,
                            account.BalanceOut.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            ListViewItem item = createAndAddItem(listViewRestrictions, Resources.Remaining,
                                                 account.Remaining.TotalDays.ToString(CultureInfo.InvariantCulture), 2);
            if(account.IsExceeded)
                item.ForeColor = Color.Red;
        }

        private void handleShiftCategoryLimitations(IPerson person, DateOnly date)
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions
            {
                UseAvailability = true,
                UsePreferences = true,
                UseStudentAvailability = true,
                UseRotations = true
            };
            var helper = new AgentInfoHelper(person, date, _stateHolder, schedulingOptions, _workShiftWorkTime);

            helper.SchedulePeriodData();

            var shiftCategoryCollection = helper.SchedulePeriod.ShiftCategoryLimitationCollection();
            var undeletedShiftCategories = new List<IShiftCategoryLimitation>();
            foreach (var shiftCategoryLimitation  in shiftCategoryCollection)
            {
                if(!((IDeleteTag) shiftCategoryLimitation.ShiftCategory).IsDeleted )
                    undeletedShiftCategories.Add(shiftCategoryLimitation);
            }
            var perPeriod = undeletedShiftCategories.Where(l => !l.Weekly).ToList();
            var perWeek = undeletedShiftCategories.Where(l => l.Weekly).ToList();
            if (perWeek.Any())
            {
                createAndAddItem(listViewRestrictions, Resources.PerWeek,
                                 string.Empty, 2);

                foreach (IShiftCategoryLimitation shiftCategoryLimitation in perWeek)
                {
                    createAndAddItem(listViewRestrictions, shiftCategoryLimitation.ShiftCategory.Description.Name,
                                     shiftCategoryLimitation.MaxNumberOf.ToString(person.PermissionInformation.Culture()), 3);
                }
            }
            if (perPeriod.Any())
            {
                createAndAddItem(listViewRestrictions, Resources.PerPeriod,
                                 string.Empty, 2); 
                foreach (IShiftCategoryLimitation shiftCategoryLimitation in perPeriod)
                {
                    createAndAddItem(listViewRestrictions, shiftCategoryLimitation.ShiftCategory.Description.Name,
                                     shiftCategoryLimitation.MaxNumberOf.ToString(person.PermissionInformation.Culture()), 3);
                } 
            }
        }

        private void handleAvailabilities(IEnumerable<IAvailabilityRestriction> availList)
        {
            foreach (IAvailabilityRestriction availRestriction in availList)
            {
                if (availRestriction.NotAvailable)
                    createAndAddItem(listViewRestrictions, Resources.Available, (!availRestriction.NotAvailable).ToString(CultureInfo.CurrentCulture), 2);
                if (availRestriction.StartTimeLimitation.HasValue())
                    createAndAddItem(listViewRestrictions, Resources.EarliestStartTime, availRestriction.StartTimeLimitation.StartTimeString, 2);
                if (availRestriction.EndTimeLimitation.HasValue())
                    createAndAddItem(listViewRestrictions, Resources.LatestEndTime, availRestriction.EndTimeLimitation.EndTimeString, 2);
                if (availRestriction.WorkTimeLimitation.HasValue())
                {
                    createAndAddItem(listViewRestrictions, Resources.MinWorkTime, availRestriction.WorkTimeLimitation.StartTimeString, 2);
                    createAndAddItem(listViewRestrictions, Resources.MaxWorkTime, availRestriction.WorkTimeLimitation.EndTimeString, 2); 
                }
            }
        }

        private void handleRotations(IEnumerable<IRotationRestriction> rotList)
        {
            foreach (IRotationRestriction rotRestriction in rotList)
            {
                addTimeRestriction(rotRestriction, 2);
                if (rotRestriction.ShiftCategory != null)
                    createAndAddItem(listViewRestrictions, Resources.ShiftCategory, rotRestriction.ShiftCategory.Description.Name, 2);
                if (rotRestriction.DayOffTemplate != null)
                    createAndAddItem(listViewRestrictions, Resources.DayOff, rotRestriction.DayOffTemplate.Description.Name, 2);
            }
        }

        private void handleStudentAvailabilities(IEnumerable<IStudentAvailabilityDay> studentAvailabilityList)
        {
            foreach (var studentAvailabilityDay in studentAvailabilityList)
            {
                foreach (StudentAvailabilityRestriction availRestriction in studentAvailabilityDay.RestrictionCollection)
                {
                    if (((IStudentAvailabilityDay)availRestriction.Parent).NotAvailable)
                        createAndAddItem(listViewRestrictions, Resources.Available, (!((IStudentAvailabilityDay)availRestriction.Parent).NotAvailable).ToString(CultureInfo.CurrentCulture), 2);
                    addTimeRestriction(availRestriction, 2);
                }
            }

        }

		private IScheduleDay getScheduleDay(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
		{
			IScheduleDay schedulePart = null;
			var scheduleRange = state.Schedules[person];
			if (scheduleRange != null) schedulePart = scheduleRange.ScheduledDay(dateOnly);
			
			return schedulePart;
		}

		private void handleOvertimeAvailabilities(IEnumerable<IOvertimeAvailability> overtimeAvailabilityList)
		{
			foreach (var overtimeAvailability in overtimeAvailabilityList)
			{
				addOvertimeAvailability(overtimeAvailability, 2);
			}
		}


        private void handlePreferences(IEnumerable<IPreferenceRestriction> preferenceList)
        {
           foreach (IPreferenceRestriction restriction in preferenceList)
           {
	           if (restriction.MustHave)
		           createAndAddItem(listViewRestrictions, Resources.MustHave, "", 2);

                addTimeRestriction(restriction, 2);
                if (restriction.ShiftCategory != null)
                    createAndAddItem(listViewRestrictions, Resources.ShiftCategory, restriction.ShiftCategory.Description.Name, 2);

               if (restriction.ActivityRestrictionCollection.Count>0)
               {
                   IActivityRestriction activityRestriction = restriction.ActivityRestrictionCollection[0];
                   createAndAddItem(listViewRestrictions, Resources.Activity,
                                    activityRestriction.Activity.Name, 2);
                   addTimeRestriction(activityRestriction, 3);
               }

			   if (restriction.Absence != null)
			   {
				   createAndAddItem(listViewRestrictions, Resources.Absence, restriction.Absence.Description.Name, 2);
			   }

                if (restriction.DayOffTemplate != null)
                    createAndAddItem(listViewRestrictions, Resources.DayOff, restriction.DayOffTemplate.Description.Name, 2);
            }
        }

        private void addTimeRestriction(IRestrictionBase restrictionBase, int indent)
        {
            if (restrictionBase.StartTimeLimitation.HasValue())
            {
                createAndAddItem(listViewRestrictions, Resources.EarliestStartTime, restrictionBase.StartTimeLimitation.StartTimeString, indent);
                createAndAddItem(listViewRestrictions, Resources.LatestStartTime, restrictionBase.StartTimeLimitation.EndTimeString, indent);
            }
            if (restrictionBase.EndTimeLimitation.HasValue())
            {
                createAndAddItem(listViewRestrictions, Resources.EarliestEndTime, restrictionBase.EndTimeLimitation.StartTimeString, indent);
                createAndAddItem(listViewRestrictions, Resources.LatestEndTime, restrictionBase.EndTimeLimitation.EndTimeString, indent);
            }
            if (restrictionBase.WorkTimeLimitation.HasValue())
            {
                createAndAddItem(listViewRestrictions, Resources.MinWorkTime, restrictionBase.WorkTimeLimitation.StartTimeString, indent);
                createAndAddItem(listViewRestrictions, Resources.MaxWorkTime, restrictionBase.WorkTimeLimitation.EndTimeString, indent);
            }
        }

		private void addOvertimeAvailability(IOvertimeAvailability overtimeAvailability, int indent)
		{
			if (overtimeAvailability.StartTime.HasValue)
			{
				var startTime = TimeHelper.TimeOfDayFromTimeSpan((TimeSpan)overtimeAvailability.StartTime, CultureInfo.CurrentCulture);
				createAndAddItem(listViewRestrictions, Resources.EarliestStartTime, startTime, indent);
			}

			if (overtimeAvailability.EndTime.HasValue)
			{
				var endTime = TimeHelper.TimeOfDayFromTimeSpan((TimeSpan)overtimeAvailability.EndTime, CultureInfo.CurrentCulture);
				createAndAddItem(listViewRestrictions, Resources.LatestEndTime, endTime, indent);
			}

			if (overtimeAvailability.NotAvailable)
			{
				createAndAddItem(listViewRestrictions, Resources.Available, (!overtimeAvailability.NotAvailable).ToString(CultureInfo.CurrentCulture), 2);	
			}
		}

        private void updatePersonPeriodData(IPerson person, DateOnly dateOnly)
        {
            listViewPersonPeriod.Items.Clear();

            IPersonPeriod personPeriod = person.Period(dateOnly);
            if(personPeriod == null)
                return;

            DateOnlyPeriod period = personPeriod.Period;

            var item = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
            item.Font = item.Font.ChangeToBold();
            listViewPersonPeriod.Items.Add(item);

            createAndAddItem(listViewPersonPeriod, Resources.Period, period.DateString, 1);
            createAndAddItem(listViewPersonPeriod, Resources.Team, personPeriod.Team.SiteAndTeam, 2);
            createAndAddItem(listViewPersonPeriod, Resources.PersonSkills, "", 2);
            foreach (IPersonSkill personSkill in personPeriod.PersonSkillCollection)
            {
				if(personSkill.Active && !((IDeleteTag)personSkill.Skill).IsDeleted)
					createAndAddItem(listViewPersonPeriod, personSkill.Skill.Name, personSkill.SkillPercentage.ToString(), 3);
            }

            if (personPeriod.PersonMaxSeatSkillCollection.Count > 0)
            {
				createAndAddItem(listViewPersonPeriod, "", "", 2);

                foreach (IPersonSkill personSkill in personPeriod.PersonMaxSeatSkillCollection)
                {
					createAndAddItem(listViewPersonPeriod, Resources.MaximumSeats, personSkill.Skill.Name, 2);
                }
            }
            

            listViewPersonPeriod.Items.Add("");
            createAndAddItem(listViewPersonPeriod, Resources.Contract, personPeriod.PersonContract.Contract.Description.Name, 2);
            createAndAddItem(listViewPersonPeriod, Resources.ContractScheduleLower, personPeriod.PersonContract.ContractSchedule.Description.Name, 2);
            createAndAddItem(listViewPersonPeriod, Resources.PartTimePercentageLower, personPeriod.PersonContract.PartTimePercentage.Description.Name, 2);
			createAndAddItem(listViewPersonPeriod, Resources.EmploymentType, _employmentTypeList[personPeriod.PersonContract.Contract.EmploymentType], 2);
			if (personPeriod.RuleSetBag != null)
				createAndAddItem(listViewPersonPeriod, Resources.RuleSetBagLower, personPeriod.RuleSetBag.Description.Name, 2);
            
        }

        private void updateSchedulePeriodData(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions
                                                       {
                                                           UseAvailability = true,
                                                           UsePreferences = true,
                                                           UseStudentAvailability = false,
                                                           UseRotations = true
                                                       };
            var helper = new AgentInfoHelper(person, dateOnly, state, schedulingOptions, _workShiftWorkTime);
            
            helper.SchedulePeriodData();
            if (!helper.Period.HasValue)
                return;

            listViewSchedulePeriod.Items.Clear();

            int freeSlots = helper.Period.Value.DayCount() - helper.CurrentOccupiedSlots;
            TimeSpan avgWorkTimePerSlot = TimeSpan.Zero;
            if (freeSlots > 0)
            {
                avgWorkTimePerSlot =
                    TimeSpan.FromTicks(helper.SchedulePeriodTargetTime.Subtract(helper.CurrentContractTime).Ticks/
                                       freeSlots);
            }

            var item = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
            item.Font = item.Font.ChangeToBold();
            listViewSchedulePeriod.Items.Add(item);

            createAndAddItem(listViewSchedulePeriod, Resources.Period, helper.Period.Value.DateString, 1);
            createAndAddItem(listViewSchedulePeriod, Resources.Type, _schedulePeriodTypeList[helper.SchedulePeriod.PeriodType], 2);
            if (person.Period(helper.SelectedDate) == null)
                return;
            EmploymentType employmentType = person.Period(helper.SelectedDate).PersonContract.Contract.EmploymentType;
            if (employmentType != EmploymentType.HourlyStaff)
            {
                if (employmentType == EmploymentType.FixedStaffNormalWorkTime)
                    createAndAddItem(listViewSchedulePeriod, Resources.TargetDaysOff,
                                     helper.SchedulePeriodTargetDaysOff.ToString(CultureInfo.CurrentCulture) + helper.DayOffTolerance, 2);
                else
                {
                    createAndAddItem(listViewSchedulePeriod, Resources.TargetDaysOff,
                                     "(" + helper.SchedulePeriodTargetDaysOff.ToString(CultureInfo.CurrentCulture) + ")",
                                     2);
                }
            }

            string contractTimeText = DateHelper.HourMinutesString(helper.SchedulePeriodTargetTime.TotalMinutes);
            if (helper.SchedulePeriodTargetMinMax.StartTime < helper.SchedulePeriodTargetMinMax.EndTime)
            {
                contractTimeText = contractTimeText + " (" +
                                   DateHelper.HourMinutesString(helper.SchedulePeriodTargetMinMax.StartTime.TotalMinutes) +
                                   " -- " +
                                   DateHelper.HourMinutesString(helper.SchedulePeriodTargetMinMax.EndTime.TotalMinutes) +
                                   ")";
            }
            createAndAddItem(listViewSchedulePeriod, Resources.ContractTime, contractTimeText, 2);

            if (employmentType != EmploymentType.HourlyStaff)
            {
                createAndAddItem(listViewSchedulePeriod, Resources.AverageWorkTimePerDay,
                                 DateHelper.HourMinutesString(
                                     helper.SchedulePeriod.AverageWorkTimePerDay.TotalMinutes), 2);
            }
           

            createAndAddItem(listViewSchedulePeriod, Resources.Current, helper.Period.Value.DateString, 1);
            item = createAndAddItem(listViewSchedulePeriod, Resources.TargetDaysOff,
                                    helper.CurrentDaysOff.ToString(CultureInfo.CurrentCulture), 2);
            if (employmentType == EmploymentType.FixedStaffNormalWorkTime)
            {
                if (helper.CurrentDaysOff != helper.SchedulePeriodTargetDaysOff)
                    item.ForeColor = Color.Red;
            }

            item = createAndAddItem(listViewSchedulePeriod, Resources.ContractTime,
                                    DateHelper.HourMinutesString(helper.CurrentContractTime.TotalMinutes), 2);
            if (!helper.SchedulePeriodTargetMinMax.Contains(helper.CurrentContractTime) &&
                employmentType != EmploymentType.HourlyStaff &&
                helper.SchedulePeriodTargetMinMax.EndTime != helper.CurrentContractTime)
                item.ForeColor = Color.Red;

            createAndAddItem(listViewSchedulePeriod, Resources.WorkTime,
                             DateHelper.HourMinutesString(helper.CurrentWorkTime.TotalMinutes), 2);
            createAndAddItem(listViewSchedulePeriod, Resources.PaidTime,
                             DateHelper.HourMinutesString(helper.CurrentPaidTime.TotalMinutes), 2);
            foreach (var pair in helper.TimePerDefinitionSet.OrderBy(k => k.Key))
            {
                TimeSpan time = pair.Value;
                createAndAddItem(listViewSchedulePeriod, pair.Key, DateHelper.HourMinutesString(time.TotalMinutes), 2);
            }

            listViewSchedulePeriod.Items.Add("");
            createAndAddItem(listViewSchedulePeriod, Resources.FreeSlots, freeSlots.ToString(CultureInfo.CurrentCulture),
                             2);
            createAndAddItem(listViewSchedulePeriod, Resources.AverageWorkTimePerSlot,
                             DateHelper.HourMinutesString(avgWorkTimePerSlot.TotalMinutes), 2);

            listViewSchedulePeriod.Items.Add("");

            if (employmentType != EmploymentType.HourlyStaff)
            {
                listViewSchedulePeriod.Items.Add("");
                createAndAddItem(listViewSchedulePeriod, Resources.PeriodInLegalState,
                                 helper.PeriodInLegalState.ToString(CultureInfo.CurrentCulture), 2);
                createAndAddItem(listViewSchedulePeriod, Resources.WeekInLegalState,
                                 helper.WeekInLegalState.ToString(CultureInfo.CurrentCulture), 2);
            }
        }

        private static ListViewItem createAndAddItem(ListView listView, string itemText, string subItemText, int indent)
        {
            var item = new ListViewItem(itemText);
            
            if (indent == 1)
                item.Font = item.Font.ChangeToBold();
            item.IndentCount = indent;
 
            var subItem = new ListViewItem.ListViewSubItem {Text = subItemText};
        	item.SubItems.Add(subItem);
            listView.Items.Add(item);

            return item;
        }

       

        private void tabControlAgentInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            update();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
	            var local = e.KeyCode == Keys.F1 && e.Modifiers == Keys.Shift;
	            var control = new Control();

                if (tabControlAgentInfo.SelectedTab == tabPageAdvSchedulePeriod)
                    control = tabPageAdvSchedulePeriod;
                else if (tabControlAgentInfo.SelectedTab == tabPageAdvPersonPeriod)
                    control = tabPageAdvPersonPeriod;
                else if (tabControlAgentInfo.SelectedTab == tabPageAdvRestrictions)
                    control = tabPageAdvRestrictions;

                HelpHelper.Current.GetHelp(this, new ControlHelpContext(control), local);
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        private void updatePersonInfo(IPerson person)
        {
            listViewPerson.Items.Clear();
            if (person == null) return;
            var item = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
            item.Font = item.Font.ChangeToBold();
	        var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
            listViewPerson.Items.Add(item);
            createAndAddItem(listViewPerson, Resources.Email, person.Email ?? "", 2);
            createAndAddItem(listViewPerson, Resources.EmployeeNo, person.EmploymentNumber ?? "", 2);
            createAndAddItem(listViewPerson, Resources.Note, person.Note ?? "", 2);
            createAndAddItem(listViewPerson, Resources.WorkflowControlSet,
                             person.WorkflowControlSet != null ? person.WorkflowControlSet.Name : "", 2);
			createAndAddItem(listViewPerson, Resources.TimeZone, timeZoneInfo.DisplayName, 2);
			createAndAddItem(listViewPerson, Resources.LocalTime, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo).ToShortTimeString(), 2);
            createAndAddItem(listViewPerson, Resources.WorkWeekStartsAt, DayOfWeekDisplay.ListOfDayOfWeek.SingleOrDefault(day => day.DayOfWeek == person.FirstDayOfWeek).DisplayName, 2);
            createAndAddItem(listViewPerson, Resources.TerminalDate,
                             person.TerminalDate != null ? person.TerminalDate.Value.ToShortDateString(CultureInfo.CurrentCulture) : "", 2);
            createAndAddItem(listViewPerson, Resources.OptionalColumn, "", 1);

            try
            {
                foreach (var column in _optionalColumns)
                {
                    createAndAddItem(listViewPerson, column.Name,
									 person.GetColumnValue(column) != null
                                         ? person.GetColumnValue(column).Description
                                         : "", 2);
                }
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.AgentInfo,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
            }
            
        }

        private void formAgentInfoResizeEnd(object sender, EventArgs e)
        {
            if (Width - 270 > 0)
                listViewPerson.Columns[1].Width = Width - 270;
        }

        private void comboBoxAgentGrouping_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dataLoaded && _selectedPerson != null)
            {
                updateFairnessInfo(_selectedPerson, _dateOnlyList.First(), _stateHolder);
            }
        }

        private void agentInfo_FromLoad(object sender, EventArgs e)
        {
            initializeFairnessTab();
	        timerRefresh.Interval = 2000;
        }

		private void timerRefreshTick(object sender, EventArgs e)
		{
			if (tabControlAgentInfo.SelectedTab == tabPageAdvPerson && _dateIsSelected)
			{
				updatePersonInfo(_selectedPerson);
			}
		}
    }
}
