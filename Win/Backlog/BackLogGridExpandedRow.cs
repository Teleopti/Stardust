using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogGridExpandedRow : IBacklogGridRow
	{
		private readonly string _cellType;
		private readonly BacklogModel _model;
		private readonly int _fixedRows;

		public BacklogGridExpandedRow(string cellType, BacklogModel model, int fixedRows)
		{
			_cellType = cellType;
			_model = model;
			_fixedRows = fixedRows;
		}

		public BacklogCategory Category { get; private set; }
		public void SetCell(ISkill skill, GridQueryCellInfoEventArgs cellInfo, DateOnly planningStartDate)
		{
			if (cellInfo.ColIndex == -1)
				return;

			if (cellInfo.ColIndex == 0)
			{
				cellInfo.Style.CellValue = "Time on Task";
				return;
			}

			var dateOnIndex = _model.GetDateOnIndex(cellInfo.ColIndex);
			TimeSpan? t;
			if (dateOnIndex < planningStartDate)
			{
				t = _model.GetScheduledTimeOnTaskForDate(cellInfo.ColIndex, cellInfo.RowIndex, _fixedRows, skill);
			}
			else
			{
				t = _model.GetPlannedTimeOnTaskForDate(cellInfo.ColIndex, cellInfo.RowIndex, _fixedRows, skill);
			}
			if (!t.HasValue)
			{
				cellInfo.Style.CellType = "Ignore";
				cellInfo.Style.CellValue = null;
			}
			else
			{
				cellInfo.Style.CellType = _cellType;
				cellInfo.Style.CellValue = t.Value;
			}
		}
	}
}