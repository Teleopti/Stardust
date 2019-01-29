using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class AgentInfoControl : BaseUserControl
    {
    	private IPerson _selectedPerson;
        private ICollection<DateOnly> _dateOnlyList;
        private readonly ISchedulerStateHolder _stateHolder;
		private readonly IEnumerable<IOptionalColumn> _optionalColumns;
		private bool _dateIsSelected;
        private IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
		private readonly IDictionary<EmploymentType, string> _employmentTypeList;
    	private readonly IDictionary<SchedulePeriodType, string> _schedulePeriodTypeList;
        private readonly SchedulerGroupPagesProvider _groupPagesProvider;
	    private readonly ILifetimeScope _container;
	    private readonly DateOnlyPeriod _dateOnlyPeriod;
	    private readonly DateOnlyPeriod _requestedPeriod;
	    private readonly IRestrictionExtractor _restrictionExtractor;
	    private IList<GroupPageLight> _groupPages;
	    private IGroupPagePerDate _groupPagePerDate;
	    private GroupPageLight _lastSelectedGroupPage;

        public AgentInfoControl()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();

            _employmentTypeList = LanguageResourceHelper.TranslateEnum<EmploymentType>();
			_schedulePeriodTypeList = LanguageResourceHelper.TranslateEnum<SchedulePeriodType>();
        }

	    public AgentInfoControl(SchedulerGroupPagesProvider groupPagesProvider,
	                            ILifetimeScope container, DateOnlyPeriod dateOnlyPeriod, DateOnlyPeriod requestedPeriod, 
															 ISchedulerStateHolder stateHolder,
			IEnumerable<IOptionalColumn> optionalColumns)
		    : this()
	    {
		    _groupPagesProvider = groupPagesProvider;
		    _container = container;
		    _dateOnlyPeriod = dateOnlyPeriod;
		    _requestedPeriod = requestedPeriod;
		    _stateHolder = stateHolder;
			_optionalColumns = optionalColumns;
			_restrictionExtractor = _container.Resolve<IRestrictionExtractor>();
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void UpdateData(IDictionary<IPerson, IScheduleRange> personDictionary, 
            ICollection<DateOnly> dateOnlyList,
            IDictionary<IPerson, IPersonAccountCollection> allAccounts)
        {
            if (personDictionary.Values.Count == 0 || dateOnlyList.Count == 0)
            {
                _dateIsSelected = false;
                listViewSchedulePeriod.Items.Clear();
                listViewPersonPeriod.Items.Clear();
                listViewRestrictions.Items.Clear();
                listViewPerson.Items.Clear();
				perPersonAndGroupListView.Items.Clear();
                return;
            }
            _dateIsSelected = true;
            _selectedPerson = personDictionary.Values.ToList()[0].Person;
            _dateOnlyList = dateOnlyList;
            _allAccounts = allAccounts;

            update();
        }

        private void update()
        {
	        timerRefresh.Enabled = false;

            if (tabControlAgentInfo.SelectedTab == tabPageAdvSchedulePeriod && _dateIsSelected)
            {
                updateSchedulePeriodData(_selectedPerson, _dateOnlyList.First(), _stateHolder.SchedulingResultState);
                return;
            }

            if (tabControlAgentInfo.SelectedTab == tabPageAdvPersonPeriod && _dateIsSelected)
            {
                updatePersonPeriodData(_selectedPerson, _dateOnlyList.First());
                return;
            }

            if (tabControlAgentInfo.SelectedTab == tabPageAdvRestrictions && _dateIsSelected)
            {
                updateRestrictionData(_selectedPerson, _dateOnlyList.First(), _stateHolder.SchedulingResultState);
                return;
            }
            if (tabControlAgentInfo.SelectedTab == tabPageAdvPerson && _dateIsSelected)
            {
				timerRefresh.Enabled = true;
                updatePersonInfo(_selectedPerson);
            }
	        if (tabControlAgentInfo.SelectedTab == tabPageFairness && _dateIsSelected)
	        {

				if (_selectedPerson.WorkflowControlSet == null)
				{
					tableLayoutPanelNoWorkflowControlSet.Visible = true;
					tableLayoutPanelRanking.Visible = false;
					tableLayoutPanelFairness.Visible = false;
					return;
				}

				var fairnessType = _selectedPerson.WorkflowControlSet.GetFairnessType();

				tableLayoutPanelNoWorkflowControlSet.Visible = false;

		        switch (fairnessType)
		        {
					case FairnessType.EqualNumberOfShiftCategory:
						tableLayoutPanelRanking.Visible = false;
						tableLayoutPanelFairness.Visible = true;
						updateFairnessData(_selectedPerson, _dateOnlyList.First(), _stateHolder.SchedulingResultState);
				        break;
					case FairnessType.Seniority :
						tableLayoutPanelRanking.Visible = true;
						tableLayoutPanelFairness.Visible = false;
						updateRankingData(_stateHolder.Schedules[_selectedPerson], _stateHolder.CommonStateHolder.ShiftCategories,
					                  _stateHolder.FilteredCombinedAgentsDictionary.Values.ToList());
				        break;
		        }
	        }
        }

        private void updateRankingData(IScheduleRange scheduleRange, IEnumerable<IShiftCategory> shiftCategories, IList<IPerson> filteredPersons)
        {
            labelDayOffPointValue.Text = "";
            labelSeniorityPointsValue.Text = "";
            labelShiftCategoryPointValue.Text = "";
	        labelRanking.Text = "";

			//bug32528, hide this for now...
			labelDayOffPointValue.Visible = false;
			labelShiftCategoryPointValue.Visible = false;
			labelSeniorityPointsValue.Visible = false;

	        labelDayOffPoints.Visible = false;
	        labelShiftCategoryPoint.Visible = false;
	        labelSeniorityPoints.Visible = false;
	       
			var rankedPersonBasedOnStartDate = _container.Resolve<IRankedPersonBasedOnStartDate>();
            var personDayOffPointsCalculator = _container.Resolve<IPersonDayOffPointsCalculator>();
            var personShiftCategoryPointCalculator = _container.Resolve<IPersonShiftCategoryPointCalculator>();
            var dayOffPoints =  personDayOffPointsCalculator.CalculateDaysOffSeniorityValue(scheduleRange, _requestedPeriod, _stateHolder.SchedulingResultState.SeniorityWorkDayRanks);
            var shiftCategoryPoints = personShiftCategoryPointCalculator.CalculateShiftCategorySeniorityValue(scheduleRange, _requestedPeriod,shiftCategories.ToList());
            var ranking = rankedPersonBasedOnStartDate.GetRankForPerson(filteredPersons, _selectedPerson);
            if (ranking.HasValue)
            {
                labelRanking.Text = rankedPersonBasedOnStartDate.GetRankForPerson(filteredPersons, _selectedPerson).Value.ToString(CultureInfo.CurrentCulture);    
            }

			labelDayOffPointValue.Text = dayOffPoints.ToString(CultureInfo.CurrentCulture);
            labelShiftCategoryPointValue.Text = shiftCategoryPoints.ToString(CultureInfo.CurrentCulture);
            labelSeniorityPointsValue.Text = (dayOffPoints + shiftCategoryPoints).ToString(CultureInfo.CurrentCulture);
        }

        private void initializeFairnessTab()
        {
            _groupPages = _groupPagesProvider.GetGroups(false);
            comboBoxAgentGrouping.DataSource = _groupPages;
            comboBoxAgentGrouping.DisplayMember = "DisplayName";
            comboBoxAgentGrouping.ValueMember = "Key";
        }

		private void resetGroupPageData()
		{
			_groupPagePerDate = _container.Resolve<IGroupPageCreator>()
				.CreateGroupPagePerDate(_stateHolder.ChoosenAgents, _stateHolder.Schedules, _dateOnlyPeriod.DayCollection(), _container.Resolve<IGroupScheduleGroupPageDataProvider>(),
					(GroupPageLight) comboBoxAgentGrouping.SelectedItem);
		}

	    private void updateFairnessData(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
		{
			perPersonAndGroupListView.Items.Clear();

			if (!_requestedPeriod.Contains(dateOnly))
				return;

			var service = _container.Resolve<IDistributionReportService>();
			var report = service.CreateReport(person, _groupPagePerDate.GetGroupPageByDate(dateOnly),
				state.LoadedAgents.ToList(), state.Schedules);
			foreach (var shiftCategory in report.DistributionDictionary.Keys)
			{
				var item = new ListViewItem(shiftCategory.Description.Name);
				item.Font.ChangeToBold();
				item.SubItems.Add(new Percent(report.DistributionDictionary[shiftCategory].Agent).ToString());
				item.SubItems.Add(new Percent(report.DistributionDictionary[shiftCategory].Team).ToString());
				item.SubItems.Add(new Percent(report.DistributionDictionary[shiftCategory].All).ToString());

				perPersonAndGroupListView.Items.Add(item);
			}
		}

        private void updateRestrictionData(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
        {
            var result = _restrictionExtractor.Extract(state.Schedules[person].ScheduledDay(dateOnly));
            listViewRestrictions.Items.Clear();

            var item = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
            item.Font = item.Font.ChangeToBold();
            listViewRestrictions.Items.Add(item);
			createAndAddItem(listViewRestrictions, "", "", 0);

			SchedulingOptions schedulingOptions = new SchedulingOptions
                                                       {
                                                           UseAvailability = true,
                                                           UsePreferences = true,
                                                           UseStudentAvailability = false,
                                                           UseRotations = true
                                                       };
            var helper = new AgentInfoHelper(person, dateOnly, state, schedulingOptions, _container.Resolve<MatrixListFactory>(), _container.Resolve<IWorkShiftMinMaxCalculator>());
			if(helper.HasMatrix)
				helper.SetRestrictionFullfillment();
			var period = person.Period(helper.SelectedDate);
			if (period != null)
			{
				var employmentType = period.PersonContract.Contract.EmploymentType;
				if (employmentType != EmploymentType.HourlyStaff)
				{
					createAndAddItem(listViewRestrictions, Resources.PreferenceFulfillment, helper.PreferenceFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(listViewRestrictions, Resources.MustHaveFulfillment, helper.MustHavesFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(listViewRestrictions, Resources.RotationFulfillment, helper.RotationFulfillment.ToString(CultureInfo.CurrentCulture), 2);
					createAndAddItem(listViewRestrictions, Resources.AvailabilityFulfillment, helper.AvailabilityFulfillment.ToString(CultureInfo.CurrentCulture), 2);
				}
				else
				{
					createAndAddItem(listViewRestrictions, Resources.StudentAvailabilityFulfillment, helper.StudentAvailabilityFulfillment.ToString(CultureInfo.CurrentCulture), 2);
				}
			}

			createAndAddItem(listViewRestrictions, "", "", 0);
			createAndAddItem(listViewRestrictions, "", "", 0);

            createAndAddItem(listViewRestrictions, Resources.Availability, "", 1);
            handleAvailabilities(result.AvailabilityList);

            createAndAddItem(listViewRestrictions, Resources.Rotations, "", 1);
            handleRotations(result.RotationList);
            createAndAddItem(listViewRestrictions, Resources.StudentAvailability, "", 1);
            handleStudentAvailability(result.StudentAvailabilityList.FirstOrDefault());

	        if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities))
	        {
		        createAndAddItem(listViewRestrictions, Resources.OvertimeAvailability, "", 1);
		        var scheduleDay = getScheduleDay(person, dateOnly, state);
		        if (scheduleDay != null) handleOvertimeAvailabilities(scheduleDay.OvertimeAvailablityCollection());
	        }

	        createAndAddItem(listViewRestrictions, Resources.Preference, "", 1);
            handlePreferences(result.PreferenceList);
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
						 if (account.StartDate > person.TerminalDate) continue;
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
            SchedulingOptions schedulingOptions = new SchedulingOptions
            {
                UseAvailability = true,
                UsePreferences = true,
                UseStudentAvailability = true,
                UseRotations = true
            };
			var helper = new AgentInfoHelper(person, date, _stateHolder.SchedulingResultState, schedulingOptions, _container.Resolve<MatrixListFactory>(), _container.Resolve<IWorkShiftMinMaxCalculator>());

            helper.SchedulePeriodData(false);

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

	    private void handleStudentAvailability(IStudentAvailabilityDay studentAvailabilityDay)
	    {
		    if (studentAvailabilityDay == null) return;
		    foreach (StudentAvailabilityRestriction availRestriction in studentAvailabilityDay.RestrictionCollection)
		    {
			    if (((IStudentAvailabilityDay) availRestriction.Parent).NotAvailable)
				    createAndAddItem(listViewRestrictions, Resources.Available, (!((IStudentAvailabilityDay) availRestriction.Parent).NotAvailable).ToString(CultureInfo.CurrentCulture), 2);
			    addTimeRestriction(availRestriction, 2);
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

			var personNameItem = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
			personNameItem.Font = personNameItem.Font.ChangeToBold();
			listViewPersonPeriod.Items.Add(personNameItem);

			IPersonPeriod personPeriod = person.Period(dateOnly);
			if (personPeriod == null)
			{
				var noPeriodPresentItem = new ListViewItem(Resources.NoPeriodPresent);
				listViewPersonPeriod.Items.Add(noPeriodPresentItem);
				return;
			}

			DateOnlyPeriod period = personPeriod.Period;

			createAndAddItem(listViewPersonPeriod, Resources.Period, period.DateString, 1);
			createAndAddItem(listViewPersonPeriod, Resources.Team, personPeriod.Team.SiteAndTeam, 2);
			createAndAddItem(listViewPersonPeriod, Resources.PersonSkills, "", 1);


			var sortedDic = new SortedDictionary<int, IList<IPersonSkill>>();

			var livingSkills =
				personPeriod.PersonSkillCollection.Where(
					personSkill => personSkill.Active && !((IDeleteTag) personSkill.Skill).IsDeleted).ToList();
			var primaryLevel = 1;
			if (livingSkills.Any())
			{
				var livingCascadingSkills = livingSkills.Where(personSkill => personSkill.Skill.IsCascading()).ToList();

				if (livingCascadingSkills.Any())
					primaryLevel = livingCascadingSkills.Min(personSkill => personSkill.Skill.CascadingIndex.Value);
			}

			foreach (var personSkill in livingSkills)
			{
				var skillPrioIndex = personSkill.Skill.CascadingIndex.GetValueOrDefault(primaryLevel);
				IList<IPersonSkill> skillList;
				if (!sortedDic.TryGetValue(skillPrioIndex, out skillList))
				{
					sortedDic.Add(skillPrioIndex, new List<IPersonSkill> {personSkill});
				}
				else
				{
					skillList.Add(personSkill);
				}
			}

			var personalIndex = 0;
			var levelString = Resources.Level;
			foreach (var keyValuePair in sortedDic)
			{
				personalIndex++;
				string level = levelString + " " + personalIndex;
				if (personalIndex == 1)
					level = Resources.Primary;

				createAndAddItem(listViewPersonPeriod, level, "", 2);
				foreach (IPersonSkill personSkill in keyValuePair.Value.OrderBy(ps => ps.Skill.Name))
				{
					createAndAddItem(listViewPersonPeriod, personSkill.Skill.Name, personSkill.SkillPercentage.ToString(), 3);
				}
			}

			if (personPeriod.MaxSeatSkill != null)
			{
				createAndAddItem(listViewPersonPeriod, Resources.MaxSeats, "", 1);
				createAndAddItem(listViewPersonPeriod, personPeriod.MaxSeatSkill.Name, "", 2);
			}


			listViewPersonPeriod.Items.Add("");

			string name = string.Empty;
			if (!((IDeleteTag) personPeriod.PersonContract.Contract).IsDeleted)
				name = personPeriod.PersonContract.Contract.Description.Name;

			createAndAddItem(listViewPersonPeriod, Resources.Contract, name, 2);

			name = string.Empty;
			if (!((IDeleteTag) personPeriod.PersonContract.ContractSchedule).IsDeleted)
				name = personPeriod.PersonContract.ContractSchedule.Description.Name;

			createAndAddItem(listViewPersonPeriod, Resources.ContractScheduleLower, name, 2);

			name = string.Empty;
			if (!((IDeleteTag) personPeriod.PersonContract.PartTimePercentage).IsDeleted)
				name = personPeriod.PersonContract.PartTimePercentage.Description.Name;

			createAndAddItem(listViewPersonPeriod, Resources.PartTimePercentageLower, name, 2);

			createAndAddItem(listViewPersonPeriod, Resources.EmploymentType,
				_employmentTypeList[personPeriod.PersonContract.Contract.EmploymentType], 2);

			name = string.Empty;
			if (personPeriod.RuleSetBag != null && !((IDeleteTag) personPeriod.RuleSetBag).IsDeleted)
				name = personPeriod.RuleSetBag.Description.Name;

			createAndAddItem(listViewPersonPeriod, Resources.RuleSetBag, name, 2);

			name = string.Empty;
			if (personPeriod.BudgetGroup != null && !((IDeleteTag)personPeriod.BudgetGroup).IsDeleted)
				name = personPeriod.BudgetGroup.Name;

			createAndAddItem(listViewPersonPeriod, Resources.BudgetGroup, name, 2);

			listViewPersonPeriod.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			listViewPersonPeriod.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void updateSchedulePeriodData(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder state)
        {
            listViewSchedulePeriod.Items.Clear();

			var personNameItem = new ListViewItem(person.Name.ToString(NameOrderOption.FirstNameLastName));
			personNameItem.Font = personNameItem.Font.ChangeToBold();
			listViewSchedulePeriod.Items.Add(personNameItem);

			SchedulingOptions schedulingOptions = new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseStudentAvailability = false,
				UseRotations = true
			};

			var helper = new AgentInfoHelper(person, dateOnly, state, schedulingOptions, _container.Resolve<MatrixListFactory>(), _container.Resolve<IWorkShiftMinMaxCalculator>());
			helper.SchedulePeriodData(true);
			if (nullOrZeroPeriod(helper.Period))
			{
				var noPeriodPresentItem = new ListViewItem(Resources.NoPeriodPresent);
				listViewSchedulePeriod.Items.Add(noPeriodPresentItem);
				return;
			}
				
            int freeSlots = helper.Period.Value.DayCount() - helper.CurrentOccupiedSlots;
            TimeSpan avgWorkTimePerSlot = TimeSpan.Zero;
            if (freeSlots > 0)
            {
                avgWorkTimePerSlot =
                    TimeSpan.FromTicks(helper.SchedulePeriodTargetTime.Subtract(helper.CurrentContractTime).Ticks/
                                       freeSlots);
            }

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
            var item = createAndAddItem(listViewSchedulePeriod, Resources.DaysOff,
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

	    private bool nullOrZeroPeriod(DateOnlyPeriod? period)
	    {
		    if (!period.HasValue)
			    return true;
		    var periodValue = period.Value;
		    return (periodValue.StartDate == DateOnly.MinValue && periodValue.EndDate == DateOnly.MinValue);
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
                bool local = true;
                if (e.KeyCode == Keys.F1 && e.Modifiers == Keys.Shift)
                    local = false;
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
            listViewPerson.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewPerson.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

	    private void updateLocalTime(IPerson person)
	    {
		    var item = listViewPerson.Items[6];
		    var subItem = item.SubItems[1];
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
		    subItem.Text = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo).ToShortTimeString();

	    }

	    private void AgentInfo_FromLoad(object sender, EventArgs e)
        {
            initializeFairnessTab();
	        timerRefresh.Interval = 2000;
        }

		private void timerRefreshTick(object sender, EventArgs e)
		{
			if (tabControlAgentInfo.SelectedTab == tabPageAdvPerson && _dateIsSelected)
			{
				updateLocalTime(_selectedPerson);
			}
		}

		private void comboBoxAgentGrouping_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selected = (GroupPageLight) comboBoxAgentGrouping.SelectedItem;
			if (selected.Key == _lastSelectedGroupPage.Key)
				return;

			_lastSelectedGroupPage = selected;
			resetGroupPageData();
			update();
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			updateSchedulePeriodData(_selectedPerson, _dateOnlyList.First(), _stateHolder.SchedulingResultState);
		}
    }
}
