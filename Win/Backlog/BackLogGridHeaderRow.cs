using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogGridHeaderRow : IBacklogGridRow
	{
		private readonly BacklogModel _model;

		public BacklogGridHeaderRow(BacklogModel model)
		{
			_model = model;
		}

		public BacklogCategory Category { get; private set; }
		public void SetCell(ISkill skill, GridQueryCellInfoEventArgs cellInfo, DateOnly planningStartDate)
		{
			if (cellInfo.ColIndex < 1)
				return;

			cellInfo.Style.CellValue = _model.GetDateStringForIndex(cellInfo.ColIndex);
		}
	}
}