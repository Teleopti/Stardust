using System;
using System.Drawing;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
	class TaskOwnerDayFilterGridRow : GridRow
	{
		private readonly PropertyReflector _propertyReflector = new PropertyReflector();
		private readonly RowManager<TaskOwnerDayFilterGridRow, WorkloadDayWithFilterStatus> _rowManager;

		public event EventHandler<FromCellEventArgs<WorkloadDayWithFilterStatus>> QueryCellValue;
		public event EventHandler<WorkloadDayIncludedInTemplateEventArgs> WorkloadDayIncludedChanged;

		public TaskOwnerDayFilterGridRow(RowManager<TaskOwnerDayFilterGridRow, WorkloadDayWithFilterStatus> rowManager, string cellType,
										 string displayMember, string rowHeaderText)
			: base(cellType, displayMember, rowHeaderText)
		{
			_rowManager = rowManager;
		}

		public override void QueryCellInfo(CellInfo cellInfo)
		{
			if (cellInfo.ColIndex == 0)
			{
				cellInfo.Style.CellValue = RowHeaderText;
			}
			else if (cellInfo.ColIndex == 1 && cellInfo.RowIndex == 1) return;
			else
			{
				if (_rowManager.DataSource.Count == 0) return;
				if (_rowManager.DataSource.Count < cellInfo.ColIndex) return;
				
				var taskOwnerDayWithFilterStatus = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);

				cellInfo.Style.CellType = CellType;

				if (cellInfo.Style.CellType.Equals("CheckBox"))
				{
					cellInfo.Style.TriState = false;
					cellInfo.Style.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
					cellInfo.Style.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
				}
				else
				{
					cellInfo.Style.ReadOnly = true;
					cellInfo.Style.TextColor = Color.DimGray;
				}

				cellInfo.Style.CellValue = GetValue(taskOwnerDayWithFilterStatus);

				var handler = QueryCellValue;
				if (handler!= null)
				{
					handler.Invoke(this, new FromCellEventArgs<WorkloadDayWithFilterStatus>
					{
						Item = taskOwnerDayWithFilterStatus,
						Style = cellInfo.Style
					});
				}
			}
		}

		private object GetValue(WorkloadDayWithFilterStatus workloadDayWithFilterStatus)
		{
			return _propertyReflector.GetValue(workloadDayWithFilterStatus, DisplayMember);
		}

		private void SetValue(WorkloadDayWithFilterStatus workloadDayWithFilterStatus, object value)
		{
			_propertyReflector.SetValue(workloadDayWithFilterStatus, DisplayMember, value);
		}

		public override void SaveCellInfo(CellInfo cellInfo)
		{
			if (_rowManager.DataSource.Count == 0) return;
			if (cellInfo.ColIndex == 0) return;

			var taskOwnerDayWithFilterStatus = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);
			if (cellInfo.Style.CellValue != null && cellInfo.Style.CellType.Equals("CheckBox"))
			{
				var newValue = cellInfo.Style.CellValue.Equals("True");
				cellInfo.Style.CellValue = newValue;
				SetValue(taskOwnerDayWithFilterStatus, cellInfo.Style.CellValue);
				TriggerWorkloadDayIncludedChanged(new WorkloadDayIncludedInTemplateEventArgs(cellInfo.ColIndex, newValue));
				return;
			}
			SetValue(taskOwnerDayWithFilterStatus, cellInfo.Style.CellValue);
		}

		public void TriggerWorkloadDayIncludedChanged(WorkloadDayIncludedInTemplateEventArgs e)
		{
			var handler = WorkloadDayIncludedChanged;
			if (handler != null) handler(this, e);
		}
	}

	public class WorkloadDayIncludedInTemplateEventArgs : EventArgs
	{
		private readonly int _colIndex;
		private readonly bool _checkValue;

		public WorkloadDayIncludedInTemplateEventArgs(int colIndex, bool checkValue)
		{
			_colIndex = colIndex;
			_checkValue = checkValue;
		}

		public int ColIndex
		{
			get { return _colIndex; }
		}

		public bool CheckValue
		{
			get { return _checkValue; }
		}
	}
}
