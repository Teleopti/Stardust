using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogEditableGridRow : BacklogGridRow, IBacklogGridRow
	{
		public BacklogEditableGridRow(BacklogCategory category, string cellType, string rowHeaderText,
			Func<int, ISkill, TimeSpan?> dataSource, BacklogModel model)
			: base(category, cellType, rowHeaderText, dataSource, model)
		{
		}

		public void SetCell(ISkill skill, GridQueryCellInfoEventArgs cellInfo, DateOnly planningStartDate)
		{
			base.SetCell(skill, cellInfo,planningStartDate);
			cellInfo.Style.BackColor = Color.Khaki;
		}
	}
}