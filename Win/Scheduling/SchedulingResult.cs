using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class SchedulingResult : BaseDialogForm
	{
		private readonly IWorkShiftFinderResultHolder _workShiftFinderResultHolder;
		private readonly bool _latestOnly;
		private readonly ICommonNameDescriptionSetting _commonNameDescription;

		public SchedulingResult(IWorkShiftFinderResultHolder workShiftFinderResultHolder, bool latestOnly, ICommonNameDescriptionSetting commonNameDescription)
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_latestOnly = latestOnly;
			_commonNameDescription = commonNameDescription;
		}

		private void schedulingResultLoad(object sender, EventArgs e)
		{
			masterGrid.TableControl.PrepareViewStyleInfo += tableControlPrepareViewStyleInfo;
			detailGrid.TableControl.PrepareViewStyleInfo += tableControl2PrepareViewStyleInfo;
			masterGrid.TableControl.CurrentCellStartEditing += TableControl_CurrentCellStartEditing;
			detailGrid.TableControl.CurrentCellStartEditing += TableControl_CurrentCellStartEditing;
			loadData();
		}

		private void loadData()
		{
			IList<IWorkShiftFinderResult> finderResult = result();
			setUpGrid(finderResult);
			setColorAndColumName();
		}

		private IList<IWorkShiftFinderResult> result()
		{
			return _workShiftFinderResultHolder.GetResults(_latestOnly, true);
		}

		private void buttonAdvCloseClick(object sender, EventArgs e)
		{
			Close();
		}

		private void setColorAndColumName()
		{
			masterGrid.TableDescriptor.Columns[1].HeaderText = UserTexts.Resources.PersonName;
			masterGrid.TableDescriptor.Columns[2].HeaderText = UserTexts.Resources.ScheduleDate;
			masterGrid.TableDescriptor.Columns[3].HeaderText = UserTexts.Resources.Message;
			detailGrid.TableDescriptor.Columns[0].HeaderText = UserTexts.Resources.Message;
			detailGrid.TableDescriptor.Columns[1].HeaderText = UserTexts.Resources.WorkShiftsBefore;
			detailGrid.TableDescriptor.Columns[2].HeaderText = UserTexts.Resources.WorkShiftsAfter;
		}
		private void setUpGrid(IList<IWorkShiftFinderResult> finderResults)
		{
			IDictionary<string, IWorkShiftFinderResult> bugfixDic = new Dictionary<string, IWorkShiftFinderResult>();
			foreach (IWorkShiftFinderResult finderResult in finderResults)
			{
				if (!bugfixDic.ContainsKey(finderResult.PersonDateKey))
					bugfixDic.Add(finderResult.PersonDateKey, finderResult);
			}

			finderResults = new List<IWorkShiftFinderResult>(bugfixDic.Values);

			var dSet = new DataSet {Locale = CultureInfo.CurrentCulture};
			detailGrid.TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;
			masterGrid.TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;
			DataTable topTable = getTopTable(finderResults);
			DataTable detailTable = getDetailTable(finderResults);
			dSet.Tables.AddRange(new[] { topTable, detailTable});

			//setup the relations
			DataColumn parentColumn = topTable.Columns["PersonDateKey"];
			DataColumn childColumn = detailTable.Columns["PersonDateKey"];
			dSet.Relations.Add("ParentToChild", parentColumn, childColumn);

			masterGrid.DataSource = topTable;
			masterGrid.TableDescriptor.Relations.Clear();

			detailGrid.DataSource = topTable;
			detailGrid.DataMember = "ParentToChild";
			detailGrid.TableDescriptor.Relations.Clear();

			masterGrid.TableOptions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			detailGrid.TableOptions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			detailGrid.Office2007ScrollBars = false;
			masterGrid.Office2007ScrollBars = false;
			detailGrid.TableOptions.AllowDragColumns = false;
			masterGrid.TableOptions.AllowDragColumns = false;

			detailGrid.TableDescriptor.VisibleColumns.Remove("PersonDateKey");
			masterGrid.TableDescriptor.VisibleColumns.Remove("PersonDateKey");
			masterGrid.TableModel.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			detailGrid.TableModel.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}


		private DataTable getTopTable(IEnumerable<IWorkShiftFinderResult> finderResults)
		{
			var dt = new DataTable("TopTable") {Locale = CultureInfo.CurrentCulture};

			dt.Columns.Add(new DataColumn("PersonDateKey"));
			dt.Columns.Add(new DataColumn("PersonName"));
			dt.Columns.Add(new DataColumn("ScheduleDate"));
			dt.Columns.Add(new DataColumn("Message"));

			foreach (IWorkShiftFinderResult result in finderResults)
			{
				DataRow dr = dt.NewRow();
				dr[0] = result.PersonDateKey;
				dr[1] = _commonNameDescription.BuildCommonNameDescription(result.Person);
				dr[2] = result.ScheduleDate.Date;
				dr[3] = string.Empty;
				if (result.FilterResults.Count>0)
					dr[3] = result.FilterResults[result.FilterResults.Count - 1].Message;
				dt.Rows.Add(dr);
			}
			return dt;
		}

		private static DataTable getDetailTable(IEnumerable<IWorkShiftFinderResult> finderResults)
		{
			var dt = new DataTable("DetailTable") {Locale = CultureInfo.CurrentCulture};

			dt.Columns.Add(new DataColumn("Message"));
			dt.Columns.Add(new DataColumn("WorkShiftsBefore"));
			dt.Columns.Add(new DataColumn("WorkShiftsAfter"));
			dt.Columns.Add(new DataColumn("PersonDateKey"));

			foreach (IWorkShiftFinderResult result in finderResults)
			{
				foreach (IWorkShiftFilterResult filterResult in result.FilterResults)
				{
					DataRow dr = dt.NewRow();
					dr[0] = filterResult.Message;
					dr[1] = filterResult.WorkShiftsBefore;
					dr[2] = filterResult.WorkShiftsAfter;
					dr[3] = result.PersonDateKey;
					dt.Rows.Add(dr);
				}
			}
			return dt;
		}
		
		void TableControl_CurrentCellStartEditing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		void tableControlPrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			GridCurrentCell cc = masterGrid.TableControl.CurrentCell;
			GridControlBase grid = masterGrid.TableControl.CurrentCell.Grid;
			if (e.RowIndex > grid.Model.Rows.HeaderCount && e.ColIndex > grid.Model.Cols.HeaderCount
					&& cc.HasCurrentCellAt(e.RowIndex))
			{
				e.Style.Interior = new BrushInfo(SystemColors.Highlight);
				e.Style.TextColor = SystemColors.HighlightText;
			}
		}

		void tableControl2PrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			GridCurrentCell cc = detailGrid.TableControl.CurrentCell;
			GridControlBase grid = detailGrid.TableControl.CurrentCell.Grid;
			if (e.RowIndex > grid.Model.Rows.HeaderCount && e.ColIndex > grid.Model.Cols.HeaderCount
					&& cc.HasCurrentCellAt(e.RowIndex))
			{
				e.Style.Interior = new BrushInfo(SystemColors.Highlight);
				e.Style.TextColor = SystemColors.HighlightText;
			}
		}
	}
}
