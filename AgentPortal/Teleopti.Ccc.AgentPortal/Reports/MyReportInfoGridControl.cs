using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Cells;
using Teleopti.Ccc.AgentPortal.Common.Configuration.Columns;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Reports
{
	public partial class MyReportInfoGridControl : BaseUserControl
	{
	    private readonly List<AdherenceInfoDto> _source = new List<AdherenceInfoDto>();
		private SFGridColumnGridHelper<AdherenceInfoDto> _columnGridHelper;

	    public MyReportInfoGridControl()
		{
			InitializeComponent();
			TimeSpanTicksHourMinutesCellModel model = new TimeSpanTicksHourMinutesCellModel(gridInfo.Model);
			model.UseSeconds = true;
			DateOnlyCellModel dateOnlyModel = new DateOnlyCellModel(gridInfo.Model);
			gridInfo.CellModels.Add("HourMinutes", model);
			gridInfo.CellModels.Add("DateOnly", dateOnlyModel);
			if (!DesignMode)
			{
				SetTexts();
			}

		}

		/// <summary>
		/// Initializes the schedule info control.
		/// </summary>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 10/13/2008
		/// </remarks>
		public void InitializeScheduleInfoControl()
		{
			LoadSourceCollection();
			BindForGrid();
		}

		/// <summary>
		/// Loads the source collection.
		/// </summary>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-14
		/// </remarks>
		private void LoadSourceCollection()
		{
			IList<AdherenceInfoDto> infoCollection = MyReportControl.StateHolder.InformationCollection;
			DateTime startDate = MyReportControl.StateHolder.SelectedDateTimePeriodDto.UtcStartTime;

			if (infoCollection != null)
			{
				foreach (AdherenceInfoDto dto in infoCollection)
				{
					//dto.LogOnDate = startDate;
					_source.Add(dto);
					startDate = startDate.AddDays(1);
				}
			}
		}

		/// <summary>
		/// Binds for grid.
		/// </summary>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-14
		/// </remarks>
		private void BindForGrid()
		{
			//bind for the grid.
			ReadOnlyCollection<SFGridColumnBase<AdherenceInfoDto>> configGrid = ConfigureGrid();
			_columnGridHelper = new SFGridColumnGridHelper<AdherenceInfoDto>(gridInfo, configGrid, _source);
			//set column widths.
			gridInfo.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}

		/// <summary>
		/// Configures the grid.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-13
		/// </remarks>
		private ReadOnlyCollection<SFGridColumnBase<AdherenceInfoDto>> ConfigureGrid()
		{
			IList<SFGridColumnBase<AdherenceInfoDto>> gridColumns = new List<SFGridColumnBase<AdherenceInfoDto>>();

			gridInfo.Rows.HeaderCount = 0;
			// Grid must have a Header column
			gridColumns.Add(new SFGridRowHeaderColumn<AdherenceInfoDto>(string.Empty));
			gridColumns.Add(new SFGridDateOnlyColumn<AdherenceInfoDto>("DateOnlyDto", UserTexts.Resources.Date, 150));
			gridColumns.Add(new SFGridHourMinutesColumn<AdherenceInfoDto>("ScheduleWorkCtiTime", UserTexts.Resources.ScheduleWorkCtiTime, 150));
			gridColumns.Add(new SFGridHourMinutesColumn<AdherenceInfoDto>("LoggedInTime", UserTexts.Resources.LoggedInTime, 150));
			gridColumns.Add(new SFGridHourMinutesColumn<AdherenceInfoDto>("IdleTime", UserTexts.Resources.NotReadyTime, 150));
			gridColumns.Add(new SFGridHourMinutesColumn<AdherenceInfoDto>("AvailableTime", UserTexts.Resources.AvailableTime, 150));

			gridInfo.RowCount = gridRowCount();
			gridInfo.ColCount = gridColumns.Count - 1;  //col index starts on 0
			gridInfo.ColHiddenEntries.Add(new GridColHidden(0));
			return new ReadOnlyCollection<SFGridColumnBase<AdherenceInfoDto>>(gridColumns);
		}

		private int gridRowCount()
		{
			return _source.Count + gridInfo.Rows.HeaderCount;
		}

		/// <summary>
		/// Refreshes the control.
		/// </summary>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-14
		/// </remarks>
		public void RefreshControl()
		{
			_columnGridHelper.SourceList.Clear();
			LoadSourceCollection();

			gridInfo.RowCount = _source.Count;
			gridInfo.Invalidate();
			gridInfo.Refresh();
		}
	}
}
