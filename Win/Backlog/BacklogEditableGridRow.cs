using System;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public class BacklogEditableGridRow : BacklogGridRow
	{
		public BacklogEditableGridRow(BacklogCategory category, string cellType, string rowHeaderText, Func<int, ISkill, TimeSpan?> dataSource, BacklogModel model) : base(category, cellType, rowHeaderText, dataSource, model)
		{
		}
	}
}