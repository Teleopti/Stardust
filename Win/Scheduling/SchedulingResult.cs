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
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class SchedulingResult : BaseRibbonForm
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

        private void SchedulingResult_Load(object sender, EventArgs e)
        {
            masterGrid.TableControl.PrepareViewStyleInfo += TableControl_PrepareViewStyleInfo;
            detailGrid.TableControl.PrepareViewStyleInfo += TableControl2_PrepareViewStyleInfo;
            masterGrid.TableControl.CurrentCellStartEditing += TableControl_CurrentCellStartEditing;
            detailGrid.TableControl.CurrentCellStartEditing += TableControl_CurrentCellStartEditing;
            LoadData();
        }
        private void LoadData()
        {
            IList<IWorkShiftFinderResult> finderResult = Result();
            SetUpGrid(finderResult);
            SetColorAndColumName();
        }

        private IList<IWorkShiftFinderResult> Result()
        {
            return _workShiftFinderResultHolder.GetResults(_latestOnly, true);
        }

        private void buttonAdvCloseClick(object sender, EventArgs e)
        {
            this.Close();
        }

        #region set up grid

        private void SetColorAndColumName()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            masterGrid.TableDescriptor.Columns[1].HeaderText = UserTexts.Resources.PersonName;
            masterGrid.TableDescriptor.Columns[2].HeaderText = UserTexts.Resources.ScheduleDate;
            masterGrid.TableDescriptor.Columns[3].HeaderText = UserTexts.Resources.Message;
            detailGrid.TableDescriptor.Columns[0].HeaderText = UserTexts.Resources.Message;
            detailGrid.TableDescriptor.Columns[1].HeaderText = UserTexts.Resources.WorkShiftsBefore;
            detailGrid.TableDescriptor.Columns[2].HeaderText = UserTexts.Resources.WorkShiftsAfter;
        }
        private void SetUpGrid(IList<IWorkShiftFinderResult> finderResults)
        {
            IDictionary<string, IWorkShiftFinderResult> bugfixDic = new Dictionary<string, IWorkShiftFinderResult>();
            foreach (IWorkShiftFinderResult finderResult in finderResults)
            {
                if (!bugfixDic.ContainsKey(finderResult.PersonDateKey))
                    bugfixDic.Add(finderResult.PersonDateKey, finderResult);
            }

            finderResults = new List<IWorkShiftFinderResult>(bugfixDic.Values);

            DataSet dSet = new DataSet();
            dSet.Locale = CultureInfo.CurrentCulture;
            detailGrid.TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;
            masterGrid.TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;
            DataTable topTable = GetTopTable(finderResults);
            DataTable detailTable = GetDetailTable(finderResults);
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

            masterGrid.TableOptions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            detailGrid.TableOptions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            detailGrid.TableOptions.AllowDragColumns = false;
            masterGrid.TableOptions.AllowDragColumns = false;

            detailGrid.TableDescriptor.VisibleColumns.Remove("PersonDateKey");
            masterGrid.TableDescriptor.VisibleColumns.Remove("PersonDateKey");
            masterGrid.TableModel.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
            detailGrid.TableModel.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
        }


        private DataTable GetTopTable(IList<IWorkShiftFinderResult> finderResults)
        {
            DataTable dt = new DataTable("TopTable");
            dt.Locale = CultureInfo.CurrentCulture;

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
        private static DataTable GetDetailTable(IList<IWorkShiftFinderResult> finderResults)
        {
            DataTable dt = new DataTable("DetailTable");
            dt.Locale = CultureInfo.CurrentCulture;

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
        #endregion

        #region grid events etc

        void TableControl_CurrentCellStartEditing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        void TableControl_PrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
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
        void TableControl2_PrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
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
        #endregion
    }
}
