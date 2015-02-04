using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogGridPercentRow : IBacklogGridRow
	{
		private readonly BacklogCategory _category;
		private readonly string _cellType;
		private readonly string _rowHeaderText;
		private readonly Func<int, ISkill, Percent> _dataSource;

		public BacklogGridPercentRow(BacklogCategory category, string cellType, string rowHeaderText,
			Func<int, ISkill, Percent> dataSource)
		{
			_category = category;
			_cellType = cellType;
			_rowHeaderText = rowHeaderText;
			_dataSource = dataSource;
		}

		public BacklogCategory Category
		{
			get { return _category; }
		}

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
		}
	}
}