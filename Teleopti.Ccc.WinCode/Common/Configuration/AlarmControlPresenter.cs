using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class AlarmControlPresenter
	{
		private readonly IAlarmControlView _view;
		private readonly IList<IAlarmType> _alarmTypes;
		private IAlarmType _alarm;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public AlarmControlPresenter(IList<IAlarmType> alarmTypes, IAlarmControlView view)
		{
			_view = view;
			_alarmTypes = alarmTypes;
		}

		public void QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _alarmTypes.Count;
			e.Handled = true;
		}

		public void QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = 6;
			e.Handled = true;
		}

		public void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColIndex < 0)
				return; // Bad index
			if (e.RowIndex == 0 && e.ColIndex == 0)
				return;
			if (e.RowIndex == 0)
				QueryHeader(e);
			else
			{
				if (e.ColIndex == ColumnHeader.Name)
					QueryName(e);
				if (e.ColIndex == ColumnHeader.Time)
					QueryTimespan(e);
				if (e.ColIndex == ColumnHeader.Color)
					QueryColorPicker(e);
				if (e.ColIndex == ColumnHeader.StaffingEffect)
					QueryStaffingEffect(e);
				if (e.ColIndex == ColumnHeader.UpdatedOn)
					QueryUpdatedOn(e);
				if (e.ColIndex == ColumnHeader.UpdatedBy)
					QueryUpdatedBy(e);
			}
			e.Handled = true;
		}

		private void QueryStaffingEffect(GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "NumericCell";
			e.Style.CellValue = _alarmTypes[e.RowIndex - 1].StaffingEffect;
		}

		private void QueryName(GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = _alarmTypes[e.RowIndex - 1].Description.Name;
		}

		private void QueryColorPicker(GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ColorPickerCell";
			e.Style.CellValueType = typeof(Color);
			e.Style.CellValue = _alarmTypes[e.RowIndex - 1].DisplayColor;
		}

		private void QueryTimespan(GridQueryCellInfoEventArgs e)
		{
			e.Style.CellValue = _alarmTypes[e.RowIndex - 1].ThresholdTime.TotalSeconds;
			e.Style.CellType = "NumericCell";
		}

		private void QueryUpdatedBy(GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (_alarmTypes[e.RowIndex - 1].UpdatedBy != null)
				e.Style.CellValue = _alarmTypes[e.RowIndex - 1].UpdatedBy.Name;
		}

		private void QueryUpdatedOn(GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (_alarmTypes[e.RowIndex - 1].UpdatedOn.HasValue)
				e.Style.CellValue = _localizer.UpdatedTimeInUserPerspective(_alarmTypes[e.RowIndex - 1]);
		}

		private static void QueryHeader(GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex == ColumnHeader.Name)
				e.Style.Text = Resources.Name;
			if (e.ColIndex == ColumnHeader.Time)
				e.Style.Text = Resources.Time;
			if (e.ColIndex == ColumnHeader.Color)
				e.Style.Text = Resources.Color;
			if (e.ColIndex == ColumnHeader.StaffingEffect)
				e.Style.Text = Resources.StaffingEffect;
			if (e.ColIndex == ColumnHeader.UpdatedOn)
				e.Style.Text = Resources.UpdatedOn;
			if (e.ColIndex == ColumnHeader.UpdatedBy)
				e.Style.Text = Resources.UpdatedBy;
		}

		public class ColumnHeader : IEnumerable<Column>
		{
			public static readonly IList<Column> Columns = new List<Column>(); 

			public static Column Name = new Column { Index = 1 };
			public static Column StaffingEffect = new Column { Index = 3 };
			public static Column Color = new Column { Index = 4 };
			public static Column Time = new Column { Index = 2 };
			public static Column UpdatedBy = new Column { Index = 5 };
			public static Column UpdatedOn = new Column { Index = 6 };

			static ColumnHeader()
			{
				Columns.Add(Name);
				Columns.Add(StaffingEffect);
				Columns.Add(Color);
				Columns.Add(Time);
				Columns.Add(UpdatedBy);
				Columns.Add(UpdatedOn);
			}

			public IEnumerator<Column> GetEnumerator()
			{
				return Columns.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class Column
		{
			public static implicit operator int(Column column)
			{
				return column.Index;
			}

			public int Index { get; set; }
		}

		public void CellClick(object sender, GridCellCancelEventArgs e)
		{
			_view.ShowThisItem(e.RowIndex - 1);
		}

		public void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.RowIndex == 0 || e.ColIndex == 0) return;

			//find alarmtype
			if (_alarmTypes.Count >= 1)
				_alarm = _alarmTypes[e.RowIndex - 1];
			else
				return;

			if (e.ColIndex == ColumnHeader.Name)
				NewName(e);
			if (e.ColIndex == ColumnHeader.Time)
				NewTimeSpan(e);
			if (e.ColIndex == ColumnHeader.Color)
				NewColor(e);
			if (e.ColIndex == ColumnHeader.StaffingEffect)
				NewStaffingEffect(e);

			e.Handled = true;
		}

		private void NewStaffingEffect(GridSaveCellInfoEventArgs e)
		{
			_alarm.StaffingEffect = (double)e.Style.CellValue;
		}

		private void NewColor(GridSaveCellInfoEventArgs e)
		{
			Color t = (Color)e.Style.CellValue;
			if (t == Color.Empty) return;
			_alarm.DisplayColor = t;
		}

		private void NewTimeSpan(GridSaveCellInfoEventArgs e)
		{
			double d = (double)e.Style.CellValue;
			if (d < 0) return;

			_alarm.ThresholdTime = TimeSpan.FromSeconds(d);
		}

		private void NewName(GridSaveCellInfoEventArgs e)
		{
			var name = (string)e.Style.CellValue;

			if (string.IsNullOrWhiteSpace(name))
			{
				return;
			}

			IAlarmType alarmType = _alarmTypes.SingleOrDefault(a => a.Description.Name == name);
			if (alarmType != null)
			{
				_view.Warning(Resources.NameAlreadyExists);
				return;
			}

			_alarm.Description = new Description(name);
		}
	}

}
