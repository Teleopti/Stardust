using System;
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
				queryHeader(e);
			else
			{
				if (e.ColIndex == 0)
					return;
				var alarmType = _alarmTypes[e.RowIndex - 1];
				Columns.Single(c => c.Index == e.ColIndex).Get(alarmType, e);
			}
			e.Handled = true;
		}
		
		private static void getStaffingEffect(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "NumericCell";
			e.Style.CellValue = alarmType.StaffingEffect;
		}

		private static void getName(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = alarmType.Description.Name;
		}

		private static void getColor(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ColorPickerCell";
			e.Style.CellValueType = typeof(Color);
			e.Style.CellValue = alarmType.DisplayColor;
		}

		private static void getTime(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellValue = alarmType.ThresholdTime.TotalSeconds;
			e.Style.CellType = "NumericCell";
		}

		private static void getUpdatedBy(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (alarmType.UpdatedBy != null)
				e.Style.CellValue = alarmType.UpdatedBy.Name;
		}

		private static void getUpdatedOn(IAlarmType alarmType, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (alarmType.UpdatedOn.HasValue)
				e.Style.CellValue = new LocalizedUpdateInfo().UpdatedTimeInUserPerspective(alarmType);
		}

		private static void queryHeader(GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = Columns.Single(c => c.Index == e.ColIndex).Text;
		}

		public static readonly IList<Column> Columns = new List<Column>();

		public class ColumnHeader
		{
			public static Column Name = new Column { Index = 1, Text = Resources.Name, Get = getName, Update = updateName};
			public static Column StaffingEffect = new Column { Index = 3, Text = Resources.StaffingEffect, Get = getStaffingEffect, Update = updateStaffingEffect };
			public static Column Color = new Column { Index = 4, Text = Resources.Color, Get = getColor, Update = updateColor };
			public static Column Time = new Column { Index = 2, Text = Resources.Time, Get = getTime, Update = updateTime };
			public static Column UpdatedBy = new Column { Index = 5, Text = Resources.UpdatedBy, Get = getUpdatedBy };
			public static Column UpdatedOn = new Column { Index = 6, Text = Resources.UpdatedOn, Get = getUpdatedOn };

			static ColumnHeader()
			{
				Columns.Add(Name);
				Columns.Add(StaffingEffect);
				Columns.Add(Color);
				Columns.Add(Time);
				Columns.Add(UpdatedBy);
				Columns.Add(UpdatedOn);
			}
		}

		public class Column
		{
			public static implicit operator int(Column column)
			{
				return column.Index;
			}

			public int Index { get; set; }
			public string Text { get; set; }
			public Action<IAlarmType, GridQueryCellInfoEventArgs> Get { get; set; }
			public Action<IAlarmType, IEnumerable<IAlarmType>, IAlarmControlView, GridSaveCellInfoEventArgs> Update { get; set; }
		}

		public void CellClick(object sender, GridCellCancelEventArgs e)
		{
			_view.ShowThisItem(e.RowIndex - 1);
		}

		public void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.RowIndex == 0 || e.ColIndex == 0) return;

			if (_alarmTypes.Count == 0)
				return;

			var column = Columns.SingleOrDefault(c => c.Index == e.ColIndex && c.Update != null);

			if (column != null)
			{
				var alarmType = _alarmTypes[e.RowIndex - 1];
				column.Update(alarmType, _alarmTypes, _view, e);
			}

			e.Handled = true;
		}

		private static void updateStaffingEffect(IAlarmType alarm, IEnumerable<IAlarmType> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			alarm.StaffingEffect = (double)e.Style.CellValue;
		}

		private static void updateColor(IAlarmType alarm, IEnumerable<IAlarmType> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var t = (Color)e.Style.CellValue;
			if (t == Color.Empty) return;
			alarm.DisplayColor = t;
		}

		private static void updateTime(IAlarmType alarm, IEnumerable<IAlarmType> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var d = (double)e.Style.CellValue;
			if (d < 0) return;

			alarm.ThresholdTime = TimeSpan.FromSeconds(d);
		}

		private static void updateName(IAlarmType alarm, IEnumerable<IAlarmType> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var name = (string)e.Style.CellValue;

			if (String.IsNullOrWhiteSpace(name))
			{
				return;
			}

			var alarmType = alarmTypes.SingleOrDefault(a => a.Description.Name == name);
			if (alarmType != null)
			{
				view.Warning(Resources.NameAlreadyExists);
				return;
			}

			alarm.Description = new Description(name);
		}
	}

}
