using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogGridWorkRow : IBacklogGridRow
	{
		private readonly BacklogCategory _category;
		private readonly string _cellType;
		private readonly string _rowHeaderText;
		private readonly Func<int, ISkill, double> _dataSource;
		private readonly BacklogModel _model;

		public BacklogGridWorkRow(BacklogCategory category, string cellType, string rowHeaderText,
			Func<int, ISkill, double> dataSource, BacklogModel model)
		{
			_category = category;
			_cellType = cellType;
			_rowHeaderText = rowHeaderText;
			_dataSource = dataSource;
			_model = model;
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
			var amount = _dataSource.Invoke(cellInfo.ColIndex, skill);
			cellInfo.Style.CellValue = amount;

			var date = _model.GetDateOnIndex(cellInfo.ColIndex);
			if (Category == BacklogCategory.ProductionPlan && date < planningStartDate)
				cellInfo.Style.BackColor = Color.LightGray;

			if (Category == BacklogCategory.Scheduled && date >= planningStartDate)
				cellInfo.Style.BackColor = Color.LightGray;

			if (_model.IsClosedOnIndex(cellInfo.ColIndex, skill))
				cellInfo.Style.BackColor = Color.LightSalmon;
		}
	}
}