﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GridTest;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;
using TeleoptiControls.DataGridViewColumns;
using TimePeriod=GridTest.TimePeriod;

namespace SdkTestWinGui
{
    public partial class Form1 : Form
    {
        private ServiceApplication _service;
        private BusinessUnitDto _businessUnit;
        private Organization _organization;
        private IList<AgentDay> _schedules;
        private SkillDataView _skillDataView;
        private PersonView _personView;
        //The timezone of the viewer
        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        private  readonly ApplicationFunctionDto _applicationFunctionDto = new ApplicationFunctionDto();
        private string _logonName;
        private string _passWord;
        private DateTime _currentDate;
        private WriteProtectionView _writeProtctectionView;

        public Form1()
        {
            InitializeComponent();
            _skillDataView = new SkillDataView(tabControl2, dataGridViewSkillDay, dataGridViewIntraday, tableLayoutPanelSkillData, _timeZoneInfo);      
        }

        public ServiceApplication Service
        {
            get { return _service; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer1.Enabled = false;
            //Organization and schedules will be viewed with this user rights
            _applicationFunctionDto.FunctionPath = "Raptor/Global/ViewSchedules";

            toolStripStatusLabel1.Text = "Starting...";
            Show();

            logOn();
        }

        private void logOn()
        {
            toolStripStatusLabel1.Text = "Logging on...";
            using (LogonDialog dialog = new LogonDialog())
            {
                DialogResult result = dialog.ShowDialog(this);
                if (result != DialogResult.OK)
                    return;
                _logonName = dialog.LogonName;
                _passWord = dialog.PassWord;
            }

            backgroundWorkerLogon.RunWorkerAsync();

        }

        private void backgroundWorkerLogon_DoWork(object sender, DoWorkEventArgs e)
        {
            //Create the Service object
            _service = new ServiceApplication(_logonName, _passWord);
            _businessUnit = Service.BusinessUnit;
            _currentDate = monthCalendar1.SelectionStart;
        }

        private void backgroundWorkerLogon_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                rethrowBackgroundException(backgroundWorkerLogon, e);
            }

            if (Service.BusinessUnit == null)
            {
                MessageBox.Show("Invalid username or password.");
                logOn();
                return;
            }

            treeView1.Nodes["All"].Text = _businessUnit.Name;
            refreshAll();
        }

        private void refreshAll()
        {
            if (backgroundWorkerLoadTree.IsBusy)
                return;

            splitContainer1.Enabled = false;

            toolStripStatusLabel1.Text = "Drawing tree...";
            listView1.Items.Clear();
            listView2.Items.Clear();
            backgroundWorkerLoadTree.RunWorkerAsync(_currentDate);
        }

        private void backgroundWorkerLoadTree_DoWork(object sender, DoWorkEventArgs e)
        {
            _organization = new Organization();
            DateTime utcDate = (DateTime) e.Argument;
            utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            _organization.Load(Service, _applicationFunctionDto, utcDate);
        }

        private void backgroundWorkerLoadTree_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _organization.DrawTree(treeView1);
            _personView = new PersonView(_service, listView1, listView2, _currentDate, splitContainer1, _organization);
            _writeProtctectionView = new WriteProtectionView(_service, listViewWriteProtect,
                                                             dateTimePickerSetProtectDate, _organization);
            //Enabled = true;
            tryEnable();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(tabControl1.SelectedTab == tabPageAgentInfo)
            {
                _personView.DrawAgentInfo(e.Node);
                return;
            }
            if (tabControl1.SelectedTab == tabPageSchedule)
            {
                loadSchedules(e.Node);
                return;
            }

			if (tabControl1.SelectedTab == tabPagePersonPeriod)
			{
				DrawPersonPeriods(e.Node);
				return;
			}
            if (tabControl1.SelectedTab == tabPageWriteProtect)
            {
                _writeProtctectionView.RedrawListView(e.Node);
                return;
            }
            
        }

        private void loadSkillData()
        {
            if (backgroundWorkerLoadSkillData.IsBusy)
                return;
            splitContainer1.Enabled = false;
            tableLayoutPanelSkillData.Visible = false;
            toolStripStatusLabel1.Text = "Drawing skill data...";
            SkillDataLoader loader = new SkillDataLoader(_currentDate, _timeZoneInfo, Service);
            backgroundWorkerLoadSkillData.RunWorkerAsync(loader);
        }

        private void loadSchedules(TreeNode selectedNode)
        {
            if (backgroundWorkerLoadSchedules.IsBusy)
                return;

            splitContainer1.Enabled = false;

            toolStripStatusLabel1.Text = "Drawing schedules...";
            //listView1.Items.Clear();
            IList<Agent> selectedAgents = _organization.SelectedAgents(selectedNode);
            ScheduleLoader loader = new ScheduleLoader(Service, selectedAgents, _currentDate, _timeZoneInfo);
            backgroundWorkerLoadSchedules.RunWorkerAsync(loader);
        }

        private void backgroundWorkerLoadSchedules_DoWork(object sender, DoWorkEventArgs e)
        {
            ScheduleLoader loader = (ScheduleLoader) e.Argument;
            IList<AgentDay> result = loader.Load();
            loader.LoadPublicNotes(result);
            e.Result = result;
        }

        private void backgroundWorkerLoadSchedules_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (rethrowBackgroundException((BackgroundWorker)sender, e))
                return; // Application.Exit();

            _schedules = (IList<AgentDay>)e.Result;
            drawDayView();

            //_presenter.SetDataSource(schedules);
            tryEnable();

        }

        private List<VisualProjection> createViewSchedules()
        {
            List<VisualProjection> schedules = new List<VisualProjection>();
            foreach (AgentDay agentDay in _schedules)
            {
                IList<VisualLayer> layerCollection = new List<VisualLayer>();
                foreach (ProjectedLayerDto layerDto in agentDay.Dto.ProjectedLayerCollection)
                {
                    DateTime localStart = TimeZoneInfo.ConvertTimeFromUtc(layerDto.Period.UtcStartTime,
                                                                          _timeZoneInfo);
                    DateTime localEnd = TimeZoneInfo.ConvertTimeFromUtc(layerDto.Period.UtcEndTime,
                                                                        _timeZoneInfo);
                    TimeSpan endTime = localEnd.TimeOfDay;
                    if (localStart.Date<localEnd.Date)
                        endTime = endTime.Add(TimeSpan.FromDays(1));
                    TimePeriod period = new TimePeriod(localStart.TimeOfDay, endTime);
                    ColorDto colorDto = layerDto.DisplayColor;
                    Color color = Color.FromArgb(colorDto.Alpha, colorDto.Red, colorDto.Green, colorDto.Blue);
                    layerCollection.Add(new VisualLayer(period, color, layerDto.Description));
                }
                bool isDayOff = false;
                string dayOffName = string.Empty;
                if(agentDay.Dto.PersonDayOff != null)
                {
                    isDayOff = true;
                    dayOffName = agentDay.Dto.PersonDayOff.Name;
                }
                VisualProjection projection = new VisualProjection(agentDay.Agent.Dto.Name, layerCollection, isDayOff, dayOffName, agentDay.PublicNote);
                schedules.Add(projection);
            }
            return schedules;
        }

        private static TimePeriod calculateViewSpan(List<VisualProjection> projections)
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;
            foreach (VisualProjection projection in projections)
            {
                if(projection.Period().HasValue)
                {
                    if (projection.Period().Value.StartTime < min)
                        min = projection.Period().Value.StartTime;
                    if (projection.Period().Value.EndTime > max)
                        max = projection.Period().Value.EndTime;
                }
            }

            if (min == TimeSpan.MaxValue)
                return new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(18));

            if (min >= TimeSpan.MinValue.Add(TimeSpan.FromHours(1)))
                min = min.Add(TimeSpan.FromHours(-1));
            if (max <= TimeSpan.MaxValue.Add(TimeSpan.FromHours(-1)))
                max = max.Add(TimeSpan.FromHours(1));
            return new TimePeriod(min, max);
        }

        private void drawDayView()
        {
            List<VisualProjection> schedules = createViewSchedules();

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            VisualProjectionColumn scheduleColumn = new VisualProjectionColumn();
            scheduleColumn.Name = "schedule";
            scheduleColumn.ReadOnly = true;
            scheduleColumn.ViewSpan = calculateViewSpan(schedules);
            dataGridView1.Columns.Add(scheduleColumn);

            foreach (VisualProjection visualProjection in schedules)
            {
                int index = dataGridView1.Rows.Add();
                DataGridViewRow row = dataGridView1.Rows[index];
                row.HeaderCell.Value = visualProjection.AgentName;
                if (visualProjection.PublicNote != null)
                {
                    row.HeaderCell.ToolTipText = visualProjection.PublicNote;
                    row.HeaderCell.Value = row.HeaderCell.Value + "*";
                }
                row.Cells["schedule"].Value = visualProjection;
            }
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

		private void DrawPersonPeriods(TreeNode selectedNode)
		{
            if (backgroundWorkerLoadPersonPeriods.IsBusy)
                return;
            backgroundWorkerLoadPersonPeriods.RunWorkerAsync(selectedNode);
		}

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (AgentDay agentDay in _schedules)
            {
                if(agentDay.MainShift!=null)
                {
                    // add a layer containing the same activity as in the topmost layer int he workshift
                    ActivityLayerDto activityLayerDto = new ActivityLayerDto();
                    activityLayerDto.Activity =
                        agentDay.MainShift.LayerCollection[agentDay.MainShift.LayerCollection.Count - 1].Dto.Activity;
                    DateTimePeriodDto period = new DateTimePeriodDto();
                    
                    // indent the layer 30 minutes from bottom layer start and end
                    period.UtcStartTime = agentDay.MainShift.LayerCollection[0].Dto.Period.UtcStartTime.AddMinutes(30);
                    period.UtcEndTime = agentDay.MainShift.LayerCollection[0].Dto.Period.UtcEndTime.AddMinutes(-30);

                    // if not set then null values will be returned to server
                    period.UtcStartTimeSpecified = true;
                    period.UtcEndTimeSpecified = true;

                    activityLayerDto.Period = period;
                    ActivityLayer activityLayer = new ActivityLayer(activityLayerDto);
                    agentDay.MainShift.LayerCollection.Add(activityLayer);

                    // convert to dto
                    List<ActivityLayerDto> layers = new List<ActivityLayerDto>();
                    foreach (ActivityLayer layer in agentDay.MainShift.LayerCollection)
                    {
                        layers.Add(layer.Dto); 
                    }
                    agentDay.MainShift.Dto.LayerCollection = layers.ToArray();

                    // save
                    Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
                    
                }
            }
            loadSchedules(treeView1.SelectedNode);
            
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    foreach (AgentDay agentDay in _schedules)
        //    {
        //        if (agentDay.MainShift != null)
        //        {
        //            // pick an activity
        //            ActivityDto activity = agentDay.MainShift.LayerCollection[agentDay.MainShift.LayerCollection.Count - 1].Dto.Activity;
        //            DateTime current = agentDay.MainShift.LayerCollection[0].Dto.Period.UtcStartTime.AddHours(1);

        //            // add a 5 minute layer once every 60 minutes
        //            while (current < agentDay.MainShift.LayerCollection[0].Dto.Period.UtcEndTime)
        //            {
        //                // create a new period of 5 minutes
        //                DateTimePeriodDto period = new DateTimePeriodDto();
        //                period.UtcStartTime = current;
        //                period.UtcEndTime = current.AddMinutes(5);
        //                // if not set then null values will be returned to server
        //                period.UtcStartTimeSpecified = true;
        //                period.UtcEndTimeSpecified = true;

        //                // create a new ActivityLayerDto
        //                ActivityLayerDto activityLayerDto = new ActivityLayerDto();
        //                activityLayerDto.Activity = activity;
        //                // assign the period to the new ActivityLayerDto
        //                activityLayerDto.Period = period;

        //                // add to mainshift layercollection
        //                ActivityLayer activityLayer = new ActivityLayer(activityLayerDto);
        //                agentDay.MainShift.LayerCollection.Add(activityLayer);

        //                // increase current
        //                current = current.AddHours(1);
        //            }

        //            // convert the mainshifts layercollection to the MainShiftDto's layerarray.
        //            List<ActivityLayerDto> layers = new List<ActivityLayerDto>();
        //            foreach (ActivityLayer layer in agentDay.MainShift.LayerCollection)
        //            {
        //                layers.Add(layer.Dto);
        //            }
        //            agentDay.MainShift.Dto.LayerCollection = layers.ToArray();

        //            // save
        //            Service.SchedulingService.SaveSchedulePart(agentDay.Dto);

        //        }
        //    }
        //    loadSchedules(treeView1.SelectedNode);

        //}

        private bool rethrowBackgroundException(BackgroundWorker sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ThreadExceptionDialog d = new ThreadExceptionDialog(e.Error);
                d.ShowDialog(this);
                sender.Dispose();
                Close();
                return true;
            }
            return false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedTab == tabPageSchedule)
                loadSchedules(treeView1.SelectedNode);
            if(tabControl1.SelectedTab == tabPageSkillData)
                loadSkillData();
			if(tabControl1.SelectedTab == tabPagePersonPeriod)
				DrawPersonPeriods(treeView1.SelectedNode);
            if (tabControl1.SelectedTab == tabPageWriteProtect)
            {
                _writeProtctectionView.RedrawListView(treeView1.SelectedNode);
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (AgentDay agentDay in _schedules)
            {
                if (agentDay.MainShift != null)
                {
                    agentDay.MainShift.LayerCollection[0].Dto.Period.UtcStartTime =
                        agentDay.MainShift.LayerCollection[0].Dto.Period.UtcStartTime.AddMinutes(-17);
                    agentDay.MainShift.LayerCollection[0].Dto.Period.UtcEndTime =
                        agentDay.MainShift.LayerCollection[0].Dto.Period.UtcEndTime.AddMinutes(-17);
                    Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
                }
            }
            loadSchedules(treeView1.SelectedNode);
        }

        private void backgroundWorkerLoadSkillData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (rethrowBackgroundException((BackgroundWorker) sender, e))
                return; // Application.Exit();

            _skillDataView.SetSkillData((IList<SkillDay>) e.Result);
            tryEnable();
        }

        private void backgroundWorkerLoadSkillData_DoWork(object sender, DoWorkEventArgs e)
        {
            IList<SkillDay> result = ((SkillDataLoader)e.Argument).Load();
            e.Result = result;
        }

        private bool backGroundWorkersReady()
        {
            if (backgroundWorkerLogon.IsBusy)
                return false;
            if(backgroundWorkerLoadTree.IsBusy)
                return false;
            if(backgroundWorkerLoadSkillData.IsBusy)
                return false;
            if(backgroundWorkerLoadSchedules.IsBusy)
                return false;

            return true;
        }

        private void tryEnable()
        {
            if(backGroundWorkersReady())
            {
                splitContainer1.Enabled = true;
                toolStripStatusLabel1.Text = string.Format("Ready (Date: {0})",_currentDate.ToShortDateString());
                monthCalendar1.DateSelected += monthCalendar1_DateSelected;
            }
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (monthCalendar1.InvokeRequired)
            {
                monthCalendar1.Invoke(new Action<object, DateRangeEventArgs>(monthCalendar1_DateSelected), sender, e);
            }
            else
            {
                if (e.Start != _currentDate)
                {
                    monthCalendar1.DateSelected -= monthCalendar1_DateSelected;
                    _currentDate = e.Start;
                    refreshAll();
                    if (tabControl1.SelectedTab == tabPageSkillData)
                        loadSkillData();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (AgentDay agentDay in _schedules)
            {
                if (agentDay.MainShift != null)
                {
                    List<ActivityLayerDto> toKeep = new List<ActivityLayerDto>();
                    foreach (ActivityLayer layer in agentDay.MainShift.LayerCollection)
                    {
                        if (layer.Dto.Activity.Description != "Lunch")
                            toKeep.Add(layer.Dto);
                    }
                    if(agentDay.MainShift.LayerCollection.Count != toKeep.Count)
                    {
                        agentDay.MainShift.Dto.LayerCollection = toKeep.ToArray();
                        Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
                    }
                }
            }
            loadSchedules(treeView1.SelectedNode);
        }

		private void AddDayOffToSelectedAgents()
		{
			foreach (var agentDay in _schedules)
			{
				var dateTime = agentDay.Dto.Date.DateTime;
				var anchorDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 12, 0, 0, DateTimeKind.Unspecified);

				DateTime utcAnchorDateTime;
				bool res;
				Service.SdkService.ConvertToUtc(anchorDateTime, true, "W. Europe Standard Time", out utcAnchorDateTime, out res);

				if (agentDay.Dto.PersonAbsenceCollection.Length > 0)
					agentDay.Dto.PersonAbsenceCollection = new PersonAbsenceDto[0];

				if (agentDay.Dto.PersonAssignmentCollection.Length > 0)
					agentDay.Dto.PersonAssignmentCollection = new PersonAssignmentDto[0];
				
				agentDay.Dto.PersonDayOff = new PersonDayOffDto
											{
												Anchor = utcAnchorDateTime,
												AnchorSpecified = true,
												Length = "P1D",
												Flexibility = "PT0H",
												Name = "DayOff",
												ShortName = "DO",
												Color = new ColorDto()
											};

				 
				Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
			}

			loadSchedules(treeView1.SelectedNode);
		}

		
        private void button4_Click(object sender, EventArgs e)
        {
            //Hold shift down to delete all overtime from shifts
            if (ModifierKeys==Keys.Shift)
            {
                foreach (AgentDay agentDay in _schedules)
                {
                    if (agentDay.MainShift==null || agentDay.OvertimeShift==null || agentDay.OvertimeShift.LayerCollection.Count==0) continue;
                    agentDay.MainShift.PersonAssignmentDto.OvertimeShiftCollection = new ShiftDto[0];
                    Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
                }
                loadSchedules(treeView1.SelectedNode);
                return;
            }
            var contracts = Service.OrganizationService.GetContracts(new LoadOptionDto{LoadDeleted = false,LoadDeletedSpecified = true});
            foreach (AgentDay agentDay in _schedules)
            {
                if (agentDay.MainShift == null || agentDay.MainShift.LayerCollection.Count==0) continue;

            	PersonPeriodDto currentPeriod = null; 
				
                foreach (var period in agentDay.Agent.Dto.PersonPeriodCollection)
                {
                    if (period.Period.StartDate.DateTime <= agentDay.Dto.Date.DateTime &&
                        period.Period.EndDate.DateTime >= agentDay.Dto.Date.DateTime)
                    {
                        currentPeriod = period;
                        break;
                    }
                }
                if (currentPeriod == null || currentPeriod.PersonContract == null) continue;

                ContractDto currentContract = null;
                foreach (var contract in contracts)
                {
                    if (contract.Id == currentPeriod.PersonContract.ContractId)
                    {
                        currentContract = contract;
                        break;
                    }
                }
                if (currentContract == null || currentContract.AvailableOvertimeDefinitionSets.Length == 0) continue;

                ActivityDto activityToUse = agentDay.MainShift.LayerCollection[0].Dto.Activity;
                string overtimeDefinitionToUse = currentContract.AvailableOvertimeDefinitionSets[0];

                DateTime lastEndTime = agentDay.MainShift.LayerCollection[0].Dto.Period.UtcEndTime;
                foreach (ActivityLayer layer in agentDay.MainShift.LayerCollection)
                {
                    if (layer.Dto.Period.UtcEndTime>lastEndTime)
                    {
                        lastEndTime = layer.Dto.Period.UtcEndTime;
                    }
                }
                if (agentDay.OvertimeShift==null)
                {
                    agentDay.OvertimeShift = new OvertimeShift(new ShiftDto());
                }
                foreach (OvertimeLayer layer in agentDay.OvertimeShift.LayerCollection)
                {
                    if (layer.Dto.Period.UtcEndTime > lastEndTime)
                    {
                        lastEndTime = layer.Dto.Period.UtcEndTime;
                    }
                }

                ShiftDto overtimeShift;
                if (agentDay.MainShift.PersonAssignmentDto.OvertimeShiftCollection.Length>0)
                {
                    overtimeShift = agentDay.MainShift.PersonAssignmentDto.OvertimeShiftCollection[0];
                }
                else
                {
                    overtimeShift = new ShiftDto{LayerCollection = new ActivityLayerDto[0]};
                    agentDay.MainShift.PersonAssignmentDto.OvertimeShiftCollection = new[] {overtimeShift};
                }
                List<ActivityLayerDto> activityLayerDtos = new List<ActivityLayerDto>(overtimeShift.LayerCollection);
                activityLayerDtos.Add(new OvertimeLayerDto
                                          {
                                              Activity = activityToUse,
                                              OvertimeDefinitionSetId = overtimeDefinitionToUse,
                                              Period =
                                                  new DateTimePeriodDto
                                                      {
                                                          UtcStartTime = lastEndTime,
                                                          UtcEndTime = lastEndTime.AddMinutes(30),
                                                          UtcStartTimeSpecified = true,
                                                          UtcEndTimeSpecified = true
                                                      }
                                          });
                overtimeShift.LayerCollection = activityLayerDtos.ToArray();

                Service.SchedulingService.SaveSchedulePart(agentDay.Dto);
            }
            loadSchedules(treeView1.SelectedNode);
        }

		private void backgroundWorkerLoadPersonPeriods_DoWork(object sender, DoWorkEventArgs e)
		{
			_organization.LoadAllPersonPeriods(Service);
		    e.Result = e.Argument;
		}

        private void backgroundWorkerLoadPersonPeriods_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listViewPersonPeriods.BeginUpdate();
            listViewPersonPeriods.Items.Clear();

            TreeNode selectedNode = (TreeNode) e.Result;

            foreach (var agent in _organization.SelectedAgents(selectedNode))
            {
                var personPeriods = _organization.GetPersonPeriods(agent);

                foreach (var personPeriod in personPeriods)
                {
                    var item = new ListViewItem(personPeriod.PersonName) { Tag = personPeriod };
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.StartDate));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.TeamDescription));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.Note));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.ContractDescription));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.PartTimePercentageDescription));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.ContractScheduleDescription));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.AcdLogOnOriginalIdList));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, personPeriod.AcdLogOnNameList));

                    listViewPersonPeriods.Items.Add(item);
                }
            }

            foreach (ColumnHeader header in listViewPersonPeriods.Columns)
            {
                header.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }

            listViewPersonPeriods.EndUpdate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (ModifierKeys==Keys.Shift)
            {
                var people = _service.OrganizationService.GetPersonsByQuery(new GetPersonByEmploymentNumberQueryDto
                                                                   {EmploymentNumber = "137784"});
                Debug.Assert(people.Length==1);

                people = _service.OrganizationService.GetPersonsByQuery(new GetPersonByIdQueryDto { PersonId = "11610FE4-0130-4568-97DE-9B5E015B2564" });
                Debug.Assert(people.Length == 1);
                return;
            }

            foreach (AgentDay agentDay in _schedules)
            {
                string note = string.Format(CultureInfo.CurrentCulture, "Public Note on date '{0}' for agent '{1}'",
                                            agentDay.Dto.Date.DateTime.ToShortDateString(), agentDay.Agent.Dto.Name);
                var publicNoteDto = new PublicNoteDto
                                        {
                                            Person = agentDay.Agent.Dto,
                                            Date = agentDay.Dto.Date,
                                            ScheduleNote = note
                                        };

                _service.SchedulingService.SavePublicNote(publicNoteDto);
            }

            loadSchedules(treeView1.SelectedNode);
        }

		private void Button6Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			AddDayOffToSelectedAgents();
			Cursor = Cursors.Default;
		}

        private void buttonSetNewWriteProtectionDate_Click(object sender, EventArgs e)
        {
            try
            {
                _writeProtctectionView.SetDateOnPersons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
