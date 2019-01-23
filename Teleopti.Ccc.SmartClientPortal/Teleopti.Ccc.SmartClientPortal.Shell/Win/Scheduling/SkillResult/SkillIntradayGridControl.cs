using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult
{
	public class SkillIntraDayGridControl : TeleoptiGridControl, IHelpContext, ITaskOwnerGrid
    {
        private const int headerWidth = 200;
        private const int headerHeight12HourClock = 26;
        private readonly SkillIntradayGridPresenter _presenter;

        public SkillIntraDayGridControl(ChartSettings chartSettings, ISkillPriorityProvider skillPriorityProvider)
        {
            _presenter = new SkillIntradayGridPresenter(this, chartSettings, skillPriorityProvider);
            QueryCellInfo += gridSkillDataQueryCellInfo;
            ColWidths[0] = headerWidth;
            if (!TimeHelper.CurrentCultureUsing24HourClock()) RowHeights[0] = headerHeight12HourClock;
            DefaultColWidth = 70;
	        TeleoptiStyling = true;
        }

        public SkillIntraDayGridControl(string settingName, ISkillPriorityProvider skillPriorityProvider)
        {
            _presenter = new SkillIntradayGridPresenter(this, settingName, skillPriorityProvider);
            QueryCellInfo += gridSkillDataQueryCellInfo;
            ColWidths[0] = headerWidth;
            if (!TimeHelper.CurrentCultureUsing24HourClock()) RowHeights[0] = headerHeight12HourClock;
            DefaultColWidth = 65;
				TeleoptiStyling = true;
        }

        private void gridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0 ) return;
            if (_presenter.GridRows.Count<=e.RowIndex) return;
            
            _presenter.GridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

            e.Handled = true;
        }

		public TimeSpan StartInterval()
		{
			if (_presenter.Intervals.Count == 0)
				return TimeSpan.MinValue;

			return _presenter.Intervals.First().TimeSpan;
		}

        public void SetupDataSource(IList<ISkillStaffPeriod> skillStaffPeriods, ISkill skill, ISchedulerStateHolder stateHolder)
        {
            SetupDataSource(skillStaffPeriods, skill, false, stateHolder);
        }

        public void SetupDataSource(IList<ISkillStaffPeriod> skillStaffPeriods, ISkill skill, bool includeStatistics, ISchedulerStateHolder stateHolder)
        {
            _presenter.SetDataSource(skillStaffPeriods, skill, includeStatistics, stateHolder);
        }
    	public SkillIntradayGridPresenter Presenter => _presenter;

		public void SetRowsAndCols()
        {
            RowCount = _presenter.GridRows.Count - 1;
            ColCount = _presenter.Intervals.Count;
        }

        public void RefreshGrid()
        {
			Refresh();
        }

        public AbstractDetailView Owner { get; set; }

        public event EventHandler<DateChangedEventArgs> DateChanged;

        public void GoToDate(DateOnly theDate)
		{
			DateChanged?.Invoke(this, new DateChangedEventArgs { NewDate = theDate });
		}

        public void SetRowVisibility(string key, bool enabled)
        {
            _presenter.SetRowVisibility(key, enabled);
        }

        public DateOnly GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IDictionary<int, GridRow> EnabledChartGridRows => _presenter.EnabledChartGridRows;

		public ReadOnlyCollection<GridRow> AllGridRows => new ReadOnlyCollection<GridRow>(new List<GridRow>(_presenter.GridRows.OfType<GridRow>()));

		public void ReloadChartSettings(ChartSettings chartSettings)
        {
            _presenter.ReloadChartSettings(chartSettings);
        }

        public void SaveSetting()
        {
            _presenter.SaveChartSettings();
        }

        public int MainHeaderRow => 0;

		public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (var key in _presenter.ChartSettings.SelectedRows)
            {
                foreach (var gridRow in _presenter.GridRows.OfType<GridRow>())
                {
                    if (gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret;
        }

        public bool HasColumns => _presenter.Intervals.Count > 0;

		public GridRow CurrentSelectedGridRow
        {
            get
            {
                if (Selections.Ranges.Count == 0) return null;
                return _presenter.GridRows[Selections.Ranges[0].Top] as GridRow;
            }
        }

		public void TurnoffHelp()
		{
			_presenter.HasHelp = false;
		}

        #region IHelpContext Members

        public bool HasHelp => _presenter.HasHelp;

		public string HelpId => Name;

		#endregion
    }
}
