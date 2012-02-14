using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
    public class SkillDataView
    {
        private readonly TabControl _tabControl;
        private readonly DataGridView _intradayGrid;
        private readonly DataGridView _dayGrid;
        private readonly TableLayoutPanel _panel;
        private IList<SkillDay> _skillData = new List<SkillDay>();
        private TimeZoneInfo _timeZoneInfo;

        public SkillDataView(TabControl tabControl, DataGridView dayGrid, DataGridView intradayGrid, TableLayoutPanel panel, TimeZoneInfo timeZoneInfo)
        {
            _tabControl = tabControl;
            _tabControl.Selected += _tabControl_Selected;
            _timeZoneInfo = timeZoneInfo;
            _intradayGrid = intradayGrid;
            _panel = panel;
            _dayGrid = dayGrid;
        }

        void _tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == null)
                return;

            drawSkillData(e.TabPage);
        }

        private void drawSkillData(TabPage page)
        {
            _panel.Visible = false;
            SkillDay skillDay = (SkillDay)page.Tag;
            _intradayGrid.Columns.Clear();
            _intradayGrid.Rows.Clear();
            _panel.Parent = page;

            
            IList<SkillDataDto> skillData = new List<SkillDataDto>(skillDay.Dto.SkillDataCollection);
            if(skillData.Count==0)
                return;
            drawDay(skillDay);
            drawIntraday(skillData);
            _panel.Visible = true;
        }

        private void drawDay(SkillDay skillDay)
        {
            _dayGrid.Columns.Clear();
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.ReadOnly = true;
            column.HeaderText = skillDay.Dto.DisplayDate.DateTime.ToShortDateString();
            column.Name = column.HeaderText;
            column.Width = 150;
            _dayGrid.Columns.Add(column);

            addRowToGrid(_dayGrid, "Forecasted agents");
            addRowToGrid(_dayGrid, "Scheduled agents");
            addRowToGrid(_dayGrid, "ESL");

            IList<SkillDataDto> skillData = new List<SkillDataDto>(skillDay.Dto.SkillDataCollection);
            _dayGrid[0, 0].Value = daySumForecasted(skillData).TotalHours;
            _dayGrid[0, 1].Value = daySumScheduled(skillData).TotalHours;
            _dayGrid[0, 2].Value = (skillDay.Dto.Esl * 100) + "%";
        }

        private static void addRowToGrid(DataGridView grid, string headerText)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.HeaderCell.Value = headerText;
            row.ReadOnly = true;
            grid.Rows.Add(row);
        }

        private void drawIntraday(IList<SkillDataDto> skillData)
        {
            foreach (SkillDataDto dto in skillData)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.ReadOnly = true;
                column.HeaderText = localTimeString(dto);
                column.Name = column.HeaderText;
                column.Width = 45;
                _intradayGrid.Columns.Add(column);
                
            }
            addRowToGrid(_intradayGrid, "Forecasted agents");
            addRowToGrid(_intradayGrid, "Scheduled agents");
            addRowToGrid(_intradayGrid, "Scheduled heads");
            addRowToGrid(_intradayGrid, "ESL");
            addRowToGrid(_intradayGrid, "Interval Stdev");

            foreach (SkillDataDto dto in skillData)
            {
                string key = localTimeString(dto);
                _intradayGrid[key, 0].Value = dto.ForecastedAgents;
                _intradayGrid[key, 1].Value = dto.ScheduledAgents;
                _intradayGrid[key, 2].Value = dto.ScheduledHeads;
                _intradayGrid[key, 3].Value = (dto.EstimatedServiceLevel * 100) + "%";
                _intradayGrid[key, 4].Value = dto.IntervalStandardDeviation;
            }

            _intradayGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        }

        public void SetSkillData(IList<SkillDay> skillData)
        {
            _skillData = skillData;
            drawSkillDayView();
            drawSkillData(_tabControl.SelectedTab);
        }

        private void drawSkillDayView()
        {
            _tabControl.TabPages.Clear();
            foreach (SkillDay day in _skillData)
            {
                TabPage page = new TabPage(day.Dto.SkillName);
                page.Tag = day;
                _tabControl.TabPages.Add(page);
            }
            if (_tabControl.TabPages.Count > 0)
                _tabControl.TabPages[0].Select();
        }

        private string localTimeString(SkillDataDto skillDataDto)
        {
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(skillDataDto.Period.UtcStartTime, _timeZoneInfo);
            return local.ToShortTimeString();
        }

        private static TimeSpan daySumForecasted(IList<SkillDataDto> skillData)
        {
            TimeSpan ret = TimeSpan.Zero;
            foreach (SkillDataDto dto in skillData)
            {
                ret = ret.Add(traffToTime(dto.ForecastedAgents, dto.Period));
            }

            return ret;
        }

        private static TimeSpan daySumScheduled(IList<SkillDataDto> skillData)
        {
            TimeSpan ret = TimeSpan.Zero;
            foreach (SkillDataDto dto in skillData)
            {
                ret = ret.Add(traffToTime(dto.ScheduledAgents, dto.Period));
            }

            return ret;
        }

        private static TimeSpan traffToTime(double traff, DateTimePeriodDto period)
        {
            TimeSpan elapsedTime = period.UtcEndTime.Subtract(period.UtcStartTime);
            return traffToTime(traff, elapsedTime);
        }

        private static TimeSpan traffToTime(double traff, TimeSpan periodLength)
        {
            return TimeSpan.FromMinutes(periodLength.TotalMinutes*traff);
        }
    }
}
