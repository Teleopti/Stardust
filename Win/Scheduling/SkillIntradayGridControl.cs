﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class SkillIntradayGridControl : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext
    {
        private const int HeaderWidth = 200;
        private const int HeaderHeight12HourClock = 26;
        private readonly SkillIntradayGridPresenter _presenter;

        public SkillIntradayGridControl(ChartSettings chartSettings)
        {
            _presenter = new SkillIntradayGridPresenter(this, chartSettings);
            QueryCellInfo += GridSkillDataQueryCellInfo;
            ColWidths[0] = HeaderWidth;
            if (!TimeHelper.CurrentCultureUsing24HourClock()) RowHeights[0] = HeaderHeight12HourClock;
            DefaultColWidth = 65;
        }

        public SkillIntradayGridControl(string settingName)
        {
            _presenter = new SkillIntradayGridPresenter(this, settingName);
            QueryCellInfo += GridSkillDataQueryCellInfo;
            ColWidths[0] = HeaderWidth;
            if (!TimeHelper.CurrentCultureUsing24HourClock()) RowHeights[0] = HeaderHeight12HourClock;
            DefaultColWidth = 65;
        }

        private void GridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
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
    	public SkillIntradayGridPresenter Presenter
    	{
			get { return _presenter; }
    	}
        public void SetRowsAndCols()
        {
            RowCount = _presenter.GridRows.Count - 1;
            ColCount = _presenter.Intervals.Count;
        }

        public void RefreshGrid()
        {
            using (PerformanceOutput.ForOperation("Refreshing SkillIntradayGridControl"))
            {
                Refresh();
            }
        }

        public AbstractDetailView Owner { get; set; }

        public event EventHandler<DateChangedEventArgs> DateChanged;

        /// <summary>
        /// Goes to date.
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public void GoToDate(DateTime theDate)
        {
            if (DateChanged != null)
            {
                DateChanged.Invoke(this, new DateChangedEventArgs { NewDate = theDate });
            }
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            _presenter.SetRowVisibility(key, enabled);
        }

        public DateTime GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {
                return _presenter.EnabledChartGridRows;
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(_presenter.GridRows.OfType<GridRow>()));
            }
        }

        public void ReloadChartSettings(ChartSettings chartSettings)
        {
            _presenter.ReloadChartSettings(chartSettings);
        }

        public void SaveSetting()
        {
            _presenter.SaveChartSettings();
        }

        public int MainHeaderRow
        {
            get { return 0; }
        }

        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (string key in _presenter.ChartSettings.SelectedRows)
            {
                foreach (GridRow gridRow in _presenter.GridRows.OfType<GridRow>())
                {
                    if (gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret;
        }

        public bool HasColumns
        {
            get { return _presenter.Intervals.Count > 0; }
        }

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

        public bool HasHelp
        {
            get { return _presenter.HasHelp; }
        }

        public string HelpId
        {
            get { return Name; }
        }

        #endregion
    }
}
