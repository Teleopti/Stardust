using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public interface IBacklogGridRow
	{
		BacklogCategory Category { get; }
		void SetCell(ISkill skill, GridQueryCellInfoEventArgs cellInfo, DateOnly planningStartDate);
	}

	public class BacklogGridRow : IBacklogGridRow
	{
		private readonly BacklogCategory _category;
		private readonly string _cellType;
		private readonly string _rowHeaderText;
		private readonly Func<int, ISkill, TimeSpan?> _dataSource;

		public BacklogGridRow(BacklogCategory category, string cellType, string rowHeaderText, Func<int, ISkill, TimeSpan?> dataSource)
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