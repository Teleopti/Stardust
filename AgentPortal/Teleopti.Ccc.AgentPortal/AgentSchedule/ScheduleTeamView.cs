using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Reports.Grid;
using Teleopti.Ccc.AgentPortal.Schedules;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    /// <summary>
    /// Display Schedules of Team 
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-04-02
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class ScheduleTeamView : BaseUserControl
    {
        private VisualProjectionGridPresenter _presenter;
        private readonly AgentScheduleTeamView _scheduleTeamView;
        private DateTime _startDateForTeamView;
        private readonly List<VisualProjection> _schedules = new List<VisualProjection>();
        private PersonDto _lastRightClickedPerson;
        private readonly ApplicationFunctionDto _applicationFunctionDto = new ApplicationFunctionDto();
        private const string _functionPath = "Raptor/Global/ViewSchedules";
        private TeamDto _loggedOnPersonTeam;
        private readonly Guid _teamAll = Guid.NewGuid();
    	private bool _isAscending;
    	private IList<GroupPageDto> _allGroupPages;
        private bool _filterPeopleForShiftTrade;

        internal static readonly Guid PageMain = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");

        public ScheduleTeamView()
        {
            InitializeComponent();
            navigationMonthCalendarTeamView.SetCurrentPersonCulture();
            _applicationFunctionDto.FunctionPath = _functionPath;
            _scheduleTeamView = new AgentScheduleTeamView(AgentScheduleStateHolder.Instance());

			navigationMonthCalendarTeamView.SelectedDates.SelectionsChanged +=
				NavigationMonthCalendarTeamViewDateValueChanged;
        }

        public DateTime StartDateForTeamView
        {
            get { return _startDateForTeamView; }
        }

        public PersonDto LastRightClickedPerson
        {
            get { return _lastRightClickedPerson; }
        }

        public bool FilterPeopleForShiftTrade
        {
            get { return _filterPeopleForShiftTrade; }
            set
            {
                if (_filterPeopleForShiftTrade == value) return;
                _filterPeopleForShiftTrade = value;
                Reload(_filterPeopleForShiftTrade);
            }
        }

        /// <summary>
        /// Sets the date selection.
        /// </summary>
        /// <param name="dateSelection">The date selection.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/17/2008
        /// </remarks>
        public void SetDateSelection(DateSelections dateSelection)
        {
            if (dateSelection.Count == 1) //from day view
            {
                navigationMonthCalendarTeamView.SelectedDates.BeginUpdate();
                navigationMonthCalendarTeamView.SelectedDates.Clear();
                //teamview loads data for a single day only
                navigationMonthCalendarTeamView.SelectedDates.Add(dateSelection[0]);
                navigationMonthCalendarTeamView.SelectedDates.EndUpdate(true);

                navigationMonthCalendarTeamView.DateValue = dateSelection[0];
                HandleDateChange();
            }
        }

		private static IEnumerable<SchedulePartDto> LoadScheduleDataWithLoadOption(GroupForPeople groupForPeople, DateOnlyDto dateOnlyDto)
		{
			var query = new GetSchedulesByGroupPageGroupQueryDto
			            	{
			            		GroupPageGroupId = groupForPeople.GroupId,
			            		QueryDate = dateOnlyDto,
			            		TimeZoneId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId
			            	};
			return SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);
		}

		private static IEnumerable<SchedulePartDto> LoadScheduleDataWithLoadOption(PersonDto person, DateOnlyDto dateOnlyDto)
		{
			var query = new GetSchedulesByPersonQueryDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				StartDate = dateOnlyDto,
				EndDate = dateOnlyDto,
				TimeZoneId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId
			};
			return SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);
        }

		private void LoadTeamMembersScheduleData(ITeamAndPeopleSelection agentTeamMemberCollection)
		{
			_schedules.Clear();

			if (SdkServiceHelper.OrganizationService != null)
			{
				List<SchedulePartDto> schedulePartDtos = LoadScheduleForSelection(agentTeamMemberCollection);

				CreateModels(agentTeamMemberCollection, schedulePartDtos);
			}
		}

    	private List<SchedulePartDto> LoadScheduleForSelection(ITeamAndPeopleSelection agentTeamMemberCollection)
    	{
    		var dateOnlyDto = new DateOnlyDto { DateTime = _startDateForTeamView };
    		var schedulePartDtos = new List<SchedulePartDto>();

    		if (agentTeamMemberCollection.SelectedTeams.Count==0)
    		{
    			foreach (var personDto in agentTeamMemberCollection.SelectedPeople)
    			{
    				schedulePartDtos.AddRange(LoadScheduleDataWithLoadOption(personDto, dateOnlyDto));
    			}
    		}
    		else
    		{
    			try
    			{
    				//try to load on team
    				foreach (var teamDto in agentTeamMemberCollection.SelectedTeams)
    				{
    					schedulePartDtos.AddRange(LoadScheduleDataWithLoadOption( teamDto , dateOnlyDto));
    				}
    			}
    			catch (WebException)
    			{
    				//if load on team failed, load for each personDto instead
    				schedulePartDtos.Clear();
    				foreach (var personDto in agentTeamMemberCollection.SelectedPeople)
    				{
    					schedulePartDtos.AddRange(LoadScheduleDataWithLoadOption(personDto, dateOnlyDto));
    				}
    			}
    		}
    		return schedulePartDtos;
    	}

		private void CreateModels(ITeamAndPeopleSelection agentTeamMemberCollection, List<SchedulePartDto> schedulePartDtos)
    	{
    		IList<ActivityVisualLayer> activityVisualLayers;
    		foreach (SchedulePartDto schedulePartDto in schedulePartDtos)
    		{
    			string dayOffName = string.Empty;
    			bool isDayOff = false;
    			activityVisualLayers = new List<ActivityVisualLayer>();
    			if (schedulePartDto.PersonDayOff != null && schedulePartDto.ProjectedLayerCollection.Count == 0)
    			{
    				isDayOff = true;
    				dayOffName = schedulePartDto.PersonDayOff.Name;
    			}
    			foreach (ProjectedLayerDto projectedLayerDto in schedulePartDto.ProjectedLayerCollection)
    			{
    				int startHour = projectedLayerDto.Period.LocalStartDateTime.Hour;
    				int endHour = projectedLayerDto.Period.LocalEndDateTime.Hour;
    				int startMinute = projectedLayerDto.Period.LocalStartDateTime.Minute;
    				int endMinute = projectedLayerDto.Period.LocalEndDateTime.Minute;
    				int startDay = 0;
    				int endDay = 0;
    				if (projectedLayerDto.Period.LocalStartDateTime.Hour > projectedLayerDto.Period.LocalEndDateTime.Hour)
    					endDay = 24;
    				if (StartDateForTeamView.Date < projectedLayerDto.Period.LocalStartDateTime.Date)
    					startDay = 24;
    				if (StartDateForTeamView.Date < projectedLayerDto.Period.LocalEndDateTime.Date)
    					endDay = 24;
    				TimePeriod timePeriod = new TimePeriod(startHour + startDay, startMinute, endHour + endDay,
    				                                       endMinute);
    				ActivityVisualLayer activityVisualLayer = new ActivityVisualLayer(timePeriod,
    				                                                                  ColorHelper.CreateColorFromDto(projectedLayerDto.DisplayColor),
    				                                                                  projectedLayerDto.Description);
    				activityVisualLayers.Add(activityVisualLayer);
    			}

    			PersonDto currentPerson = null;
    			foreach (PersonDto personDto in agentTeamMemberCollection.SelectedPeople)
    			{
    				if (personDto.Id == schedulePartDto.PersonId)
    				{
    					currentPerson = personDto;
    					break;
    				}
    			}
    			if (currentPerson != null)
    				_schedules.Add(new VisualProjection(currentPerson, activityVisualLayers, dayOffName, isDayOff));
    		}
    	}

    	private void CreateDisplayDate() 
        {
            PersonDto loggedOnPerson = _scheduleTeamView.Presenter.ScheduleStateHolder.Person;
            string displayDate;
            if (!loggedOnPerson.CultureLanguageId.HasValue)
                displayDate = StartDateForTeamView.ToShortDateString();
            else
            {
                CultureInfo c = (loggedOnPerson.CultureLanguageId.HasValue
                                               ? CultureInfo.GetCultureInfo(loggedOnPerson.CultureLanguageId.Value)
                                               : CultureInfo.CurrentCulture).FixPersianCulture();
                displayDate = string.Format(c, StartDateForTeamView.ToShortDateString());
            }
            
            autoLabelSelectedDate.Text = displayDate;
        }

        /// <summary>
        /// Handles the date change.
        /// </summary>
        /// <remarks>
        /// Created by:  Muhamad Risath
        /// Created date: 4/23/2008
        /// </remarks>
        public void HandleDateChange()
        {
            Cursor = Cursors.WaitCursor;

            if (navigationMonthCalendarTeamView.SelectedDates != null &&
                navigationMonthCalendarTeamView.SelectedDates.Count == 0)
            {
                _startDateForTeamView = DateTime.SpecifyKind(navigationMonthCalendarTeamView.DateValue.Date,DateTimeKind.Unspecified);
            }
            else if (navigationMonthCalendarTeamView.SelectedDates != null &&
                     navigationMonthCalendarTeamView.SelectedDates.Count > 0)
            {
            	_startDateForTeamView = DateTime.SpecifyKind(navigationMonthCalendarTeamView.SelectedDates[0].Date,
            	                                             DateTimeKind.Unspecified);
            }
            //Argh.. 00:00 is not a valid time for all dates eg. Jordan timeszone 2011-04-01
            //Move the standard 00:00 12 hours forward to get rid of ilegal date
            _startDateForTeamView = _startDateForTeamView.AddHours(12); 
            try
            {
            	LoadTeams();

                var oldSelectedItem = comboSiteAndTeam.SelectedItem as GroupDetailModel;
                reloadGroups(oldSelectedItem!=null ? (Guid?)oldSelectedItem.Id : null);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        /// <summary>
        /// Initializes the team view.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-04-03
        /// </remarks>
        public void InitializeScheduleTeamView()
        {
            splitContainerAdvTeamView.RightToLeft = RightToLeft.No;
            navigationMonthCalendarTeamView.RightToLeft = RightToLeft;
            gridControlTeamSchedules.RightToLeft = RightToLeft;

            _startDateForTeamView = DateTime.SpecifyKind(DateTime.Today,DateTimeKind.Unspecified);
            navigationMonthCalendarTeamView.DateValue = _startDateForTeamView;

			LoadTeams();
            if (isPermittedToViewCustomTeamSchedule())
                enableGroupCombo();
            else disableGroupCombo();
            InitializeGroupCombo();
            InitializeTeamViewGrid();

            navigationMonthCalendarTeamView.CalendarGrid.ContextMenu = new ContextMenu(); //Dummy menu for blocking right clicks in calander
        }

        private static bool isPermittedToViewCustomTeamSchedule()
        {
            return PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.ViewCustomTeamSchedule);
        }

        private void disableGroupCombo()
        {
            comboBoxAdvGroup.Visible = false;
            comboSiteAndTeam.Location = comboBoxAdvGroup.Location;
            comboSiteAndTeam.Size = comboBoxAdvGroup.Size;
        }

        private void enableGroupCombo()
        {
            comboBoxAdvGroup.Visible = true;
        }

        private void InitializeGroupCombo()
		{
			_allGroupPages = new List<GroupPageDto>();

			GroupPageDto businessHierarchyPage = null;
			var groupPageDtos = SdkServiceHelper.OrganizationService.GroupPagesByQuery(new GetAvailableCustomGroupPagesQueryDto());
			foreach (var groupPageDto in groupPageDtos)
			{
				if (groupPageDto.PageName.StartsWith("xx", StringComparison.OrdinalIgnoreCase))
				{
					var tmp = LanguageResourceHelper.Translate(groupPageDto.PageName);
					if (tmp != null)
						groupPageDto.PageName = tmp.Replace("\r\n", " ");
				}
				if (groupPageDto.Id==PageMain)
				{
					businessHierarchyPage = groupPageDto;
					continue;
				}
				_allGroupPages.Add(groupPageDto);
			}

			if (businessHierarchyPage!=null)
			{
				_allGroupPages.Insert(0,businessHierarchyPage);
			}

			comboBoxAdvGroup.DataSource = _allGroupPages;
			comboBoxAdvGroup.DisplayMember = "PageName";

            foreach (var groupPage in _allGroupPages)
            {
                if (groupPage.Id == PageMain)
                {
                    comboBoxAdvGroup.SelectedItem = groupPage;
                    break;
                }
            }
		}

		private void LoadTeams()
		{
            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);
            DateTime startDateUtc = TimeZoneHelper.ConvertToUtc(_startDateForTeamView, timeZone);
			startDateUtc = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Utc);
			
			_loggedOnPersonTeam = SdkServiceHelper.OrganizationService.GetLoggedOnPersonTeam(startDateUtc);
		}

    	/// <summary>
        /// Initializes the grid related items.
        /// </summary>
        /// <remarks>
        /// Created by: MuhamadR
        /// Created date: 2008-03-13
        /// </remarks>
        private void InitializeTeamViewGrid()
        {
            gridControlTeamSchedules.Cols.Size[0] = 200;
            gridControlTeamSchedules.Cols.Size[1] = gridControlTeamSchedules.Width - gridControlTeamSchedules.Cols.Size[0] - 5;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.AgentPortal.Reports.Grid.ScheduleGridColumnGridHelper`1<Teleopti.Ccc.AgentPortalCode.Common.VisualProjection>"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.AgentPortal.Reports.Grid.ScheduleGridColumnGridHelper`1<Teleopti.Ccc.AgentPortal.Schedules.VisualProjection>")]
        private void Reload(bool filterEnabled)
        {
            InitializeTeamViewGrid();

        	var selectedTeam = (GroupDetailModel)comboSiteAndTeam.SelectedItem;
        	
        	ITeamAndPeopleSelection selection;
			if (selectedTeam.Id==_teamAll)
			{
				selection = new AllTeamsSelection(_startDateForTeamView, selectedTeam, comboSiteAndTeam.Items);
			}
			else
			{
				selection = new BasicSelection(_startDateForTeamView, selectedTeam);
			}

            selection.Initialize(filterEnabled);
        	LoadTeamMembersScheduleData(selection);

			_presenter = new VisualProjectionGridPresenter(gridControlTeamSchedules, _schedules);
			CreateDisplayDate();
			new ScheduleGridColumnGridHelper<VisualProjection>(gridControlTeamSchedules, _presenter.GridColumns, _schedules);
	        _lastRightClickedPerson = null;
        }

        #region Event Handlers

        private void NavigationMonthCalendarTeamViewDateValueChanged(object sender, EventArgs e)
        {
            HandleDateChange();
        }

        /// <summary>
        /// Handles the CellClick event of the gridControlTeamSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellClickEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-04-04
        /// </remarks>
        private void gridControlTeamSchedules_CellClick(object sender, GridCellClickEventArgs e)
        {
            if (!IsColumnHeader(e.RowIndex)) { e.Cancel = true; return; }
            var comparer = _presenter.GridColumns[e.ColIndex].ColumnComparer;
            _schedules.Sort(comparer);
            if (_isAscending)
            {
                _schedules.Reverse();
                _isAscending = false;
            }
            else
                _isAscending = true;
            gridControlTeamSchedules.Refresh();
        }

        private static bool IsColumnHeader(int rowIndex)
        {
            return rowIndex == 0;
        }

        private void gridControlTeamSchedules_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rolIndex, colIndex;
                bool cellFound = gridControlTeamSchedules.PointToRowCol(e.Location, out rolIndex, out colIndex);
                if (cellFound)
                {
                    VisualProjection visualProjection = gridControlTeamSchedules[rolIndex, 0].Tag as VisualProjection;
                    _lastRightClickedPerson = null;
                    if (visualProjection != null)
                    {
                        // TODO: Preliminary code to prevent shift trade of DayOff's...
                        //if (!visualProjection.IsDayOff)
                        {
                            _lastRightClickedPerson = visualProjection.Person;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Handles the CurrentCellActivating event of the gridControlTeamSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCurrentCellActivatingEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-04-04
        /// </remarks>
        private void gridControlTeamSchedules_CurrentCellActivating(object sender, GridCurrentCellActivatingEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Handles the ResizingColumns event of the gridControlTeamSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridResizingColumnsEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-04-04
        /// </remarks>
        private void gridControlTeamSchedules_ResizingColumns(object sender, GridResizingColumnsEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Handles the ResizingRows event of the gridControlTeamSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridResizingRowsEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-04-04
        /// </remarks>
        private void gridControlTeamSchedules_ResizingRows(object sender, GridResizingRowsEventArgs e)
        {
            e.Cancel = true;
        }

        #endregion

        private void gridControlTeamSchedules_Resize(object sender, EventArgs e)
        {
            gridControlTeamSchedules.Cols.Size[0] = 200;
            gridControlTeamSchedules.Cols.Size[1] = gridControlTeamSchedules.Width - gridControlTeamSchedules.Cols.Size[0] - 5;
        }

        private void buttonPreviousDate_Click(object sender, EventArgs e)
        {
            int daysToAdd = -1;
            StepDayWithButton(daysToAdd);
        }

        private void buttonNextDate_Click(object sender, EventArgs e)
        {
            int daysToAdd = 1;
            StepDayWithButton(daysToAdd);
        }

        private void StepDayWithButton(int daysToAdd)
        {
            DateTime[] navigatedDays = new DateTime[1];

            if (navigationMonthCalendarTeamView.SelectedDates.Count == 0)
            {
                navigatedDays[0] = navigationMonthCalendarTeamView.Today.Date;
                navigationMonthCalendarTeamView.SelectedDates.AddRange(navigatedDays);
            }

            navigationMonthCalendarTeamView.SelectedDates.BeginUpdate();
            navigatedDays[0] = navigationMonthCalendarTeamView.SelectedDates[0].AddDays(daysToAdd);
            navigationMonthCalendarTeamView.SelectedDates.Clear();
            navigationMonthCalendarTeamView.SelectedDates.AddRange(navigatedDays);
            navigationMonthCalendarTeamView.DateValue = navigatedDays[0].Date;
            navigationMonthCalendarTeamView.SelectedDates.EndUpdate(true);
            HandleDateChange();
        }

        private void comboSiteAndTeam_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboSiteAndTeam == null || comboSiteAndTeam.SelectedItem==null)
                return;
            Cursor = Cursors.WaitCursor;
            Reload(FilterPeopleForShiftTrade);
            Cursor = Cursors.Default;
        }

        private void populateSiteTeamCombo(IList<GroupDetailModel> teams, Guid? preselectedGroupdId)
        {
            comboSiteAndTeam.SelectedIndexChanged -= comboSiteAndTeam_SelectedIndexChanged;
			comboSiteAndTeam.DataSource = teams;
			comboSiteAndTeam.DisplayMember = "DisplayText";
            comboSiteAndTeam.SelectedIndexChanged += comboSiteAndTeam_SelectedIndexChanged;
            comboSiteAndTeam.SelectedItem = null;

            if (preselectedGroupdId.HasValue)
            {
            	foreach (var groupDetailModel in teams)
            	{
            		if (preselectedGroupdId.Value == groupDetailModel.Id)
            		{
						comboSiteAndTeam.SelectedItem = groupDetailModel;
						break;
            		}
            	}
            }

            if (hasItemsButNoSelected())
            {
                comboSiteAndTeam.SelectedItem = teams[0]; 
            }
        }

    	private bool hasItemsButNoSelected()
    	{
    		return comboSiteAndTeam.SelectedItem==null &&
    		       comboSiteAndTeam.Items.Count>0;
    	}

    	private void comboBoxAdvGroup_SelectedIndexChanged(object sender, EventArgs e)
    	{
            reloadGroups(_loggedOnPersonTeam != null ? _loggedOnPersonTeam.Id : null);
    	}

        private void reloadGroups(Guid? preselectedGroupId)
        {
            var dataSourceItems = new List<GroupDetailModel>();
            var selectedItem = ((GroupPageDto) comboBoxAdvGroup.SelectedItem);

            var groups = SdkServiceHelper.OrganizationService.GroupPageGroupsByQuery(new GetGroupsForGroupPageAtDateQueryDto
                {
                    PageId = selectedItem.Id.GetValueOrDefault(),
                    QueryDate =
                        new DateOnlyDto
                            {
                                DateTime = _startDateForTeamView,
                            }
                });
            foreach (var groupPageGroupDto in groups)
            {
                dataSourceItems.Add(new GroupDetailModel
                    {
                        DisplayText =
                            groupPageGroupDto.GroupName.StartsWith("xx", StringComparison.OrdinalIgnoreCase)
                                ? LanguageResourceHelper.Translate(groupPageGroupDto.GroupName)
                                : groupPageGroupDto.GroupName,
                        Id = groupPageGroupDto.Id.GetValueOrDefault(),
                        Object = groupPageGroupDto
                    });
            }
            if (selectedItem.Id == PageMain)
            {
                dataSourceItems.Insert(0, new GroupDetailModel {DisplayText = Resources.All, Id = _teamAll, Object = null});
            }
            populateSiteTeamCombo(dataSourceItems, preselectedGroupId);
        }

        private void panelBetweenRibbonAndGridRightResize(object sender, EventArgs e)
        {
            int otherWidth = 265;
            if(comboBoxAdvGroup.Visible)
                otherWidth += 230;

            autoLabelSelectedDate.Left = otherWidth;
            autoLabelSelectedDate.Width = panelBetweenRibbonAndGridRight.Width - (otherWidth +40);
            
        }
    }
}
