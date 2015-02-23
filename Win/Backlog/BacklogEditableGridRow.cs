using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogEditableGridRow : IBacklogGridRow
	{
		private readonly string _cellType;
		private readonly string _rowHeaderText;
		private readonly Func<int, ISkill, TimeSpan?> _dataSource;
		private readonly BacklogModel _model;

		public BacklogEditableGridRow(BacklogCategory category, string cellType, string rowHeaderText,
			Func<int, ISkill, TimeSpan?> dataSource, BacklogModel model)
		{
			_cellType = cellType;
			_rowHeaderText = rowHeaderText;
			_dataSource = dataSource;
			_model = model;
			Category = category;
		}

		public BacklogCategory Category { get; private set; }

		public void SetCell(ISkill skill, GridQueryCellInfoEventArgs cellInfo, DateOnly planningStartDate)
		{
			if (cellInfo.ColIndex == -1)
				return;

			if (cellInfo.ColIndex == 0)
			{
				cellInfo.Style.CellValue = _rowHeaderText;
				return;
			}

			cellInfo.Style.CellType = _cellType;
			var time = _dataSource.Invoke(cellInfo.ColIndex, skill);
			cellInfo.Style.CellValue = time;

			var date = _model.GetDateOnIndex(cellInfo.ColIndex);
			if (Category == BacklogCategory.ProductionPlan && date < planningStartDate)
				cellInfo.Style.BackColor = Color.LightGray;

			if (Category == BacklogCategory.Scheduled && date >= planningStartDate)
				cellInfo.Style.BackColor = Color.LightGray;

			if (_model.IsClosedOnIndex(cellInfo.ColIndex, skill))
				cellInfo.Style.BackColor = Color.LightSalmon;


			cellInfo.Style.BackColor = Color.Khaki;
		}
	}
}