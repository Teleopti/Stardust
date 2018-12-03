using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SkillResult
{
	public abstract class SkillResultGridControlBase : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext
	{
		private ChartSettings _chartSettings;
		private readonly ChartSettings _defaultChartSettings = new ChartSettings();
		private GridRow _currentSelectedGridRow;
		private IList<IGridRow> _gridRows;
		public AbstractDetailView Owner { get; set; }

		protected SkillResultGridControlBase()
		{
			TeleoptiStyling = true;
		}

		public void InitializeBase(string settingName)
		{
			setupChartDefault();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_chartSettings = new PersonalSettingDataRepository(uow).FindValueByKey(settingName, _defaultChartSettings);
			}
		}

		public bool HasHelp
		{
			get
			{
				return true;
			}
		}

		public string HelpId
		{
			get
			{
				return Name;
			}
		}

		public GridRow CurrentSelectedGridRow
		{
			get
			{
				return _currentSelectedGridRow;
			}
		}

		public abstract void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill);

		public abstract void DrawDayGrid(ISchedulerStateHolder stateHolder, ISkill skill);

		public void SaveSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonalSettingDataRepository(uow).PersistSettingValue(_chartSettings);
				uow.PersistAll();
			}
		}

		public IChartSeriesSetting ConfigureSetting(string key)
		{
			var ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
			ret.Enabled = _chartSettings.SelectedRows.Contains(key);
			return ret;
		}

		private void setupChartDefault()
		{
			_defaultChartSettings.SelectedRows.Add("ForecastedHours");
			_defaultChartSettings.SelectedRows.Add("ScheduledHours");
			_defaultChartSettings.SelectedRows.Add("RelativeDifference");
		}

		public abstract bool HasColumns { get;}

		public void RefreshGrid()
		{
			Refresh();
		}

		public void GoToDate(DateOnly theDate)
		{
			RefreshGrid();
		}

		public DateOnly GetLocalCurrentDate(int column)
		{
			throw new NotImplementedException();
		}

		public IDictionary<int, GridRow> EnabledChartGridRows
		{
			get
			{

				if (GridRows == null)
					return new Dictionary<int, GridRow>();

				IDictionary<int, GridRow> settings = (from r in GridRows.OfType<GridRow>()
													  where r.ChartSeriesSettings != null &&
															r.ChartSeriesSettings.Enabled
													  select r).ToDictionary(k => GridRows.IndexOf(k), v => v);

				return settings;
			}
		}

		public ReadOnlyCollection<GridRow> AllGridRows
		{
			get
			{
				if (GridRows == null)
					return new ReadOnlyCollection<GridRow>(new List<GridRow>());
				return new ReadOnlyCollection<GridRow>(new List<GridRow>(GridRows.OfType<GridRow>()));
			}
		}

		public virtual int MainHeaderRow
		{
			get
			{
				return 0;
			}
		}

		public IList<IGridRow> GridRows
		{
			get { return _gridRows; }
			set { _gridRows = value; }
		}

		public IList<GridRow> EnabledChartGridRowsMicke65()
		{
			IList<GridRow> ret = new List<GridRow>();
			foreach (string key in _chartSettings.SelectedRows)
			{
				foreach (GridRow gridRow in GridRows.OfType<GridRow>())
				{
					if (gridRow.DisplayMember == key)
						ret.Add(gridRow);
				}
			}

			return ret;
		}

		public void SetRowVisibility(string key, bool enabled)
		{
			if (enabled)
				_chartSettings.SelectedRows.Add(key);
			else
			{
				_chartSettings.SelectedRows.Remove(key);
			}
		}

		protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");
			if (e.Range.Top > 0)
			{
				var gridRow = GridRows[e.Range.Top] as GridRow;

				if (gridRow != null)
				{
					_currentSelectedGridRow = gridRow;
				}
			}
			base.OnSelectionChanged(e);
		}
	}
}