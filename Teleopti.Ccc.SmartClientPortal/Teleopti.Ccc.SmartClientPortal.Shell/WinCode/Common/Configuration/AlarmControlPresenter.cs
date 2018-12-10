using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public interface IAlarmControlPresenterDecorator
	{
		void Decorate(AlarmControlPresenter alarmControlPresenter);
	}

	public class AlarmControlPresenter
	{
		private readonly IAlarmControlView _view;
		private readonly IEnumerable<IRtaRule> _rules;
		private readonly IList<Column> _columns = new List<Column>();

		public AlarmControlPresenter(IEnumerable<IRtaRule> rules, IAlarmControlView view, IEnumerable<IAlarmControlPresenterDecorator> decorators)
		{
			decorators = decorators ?? new IAlarmControlPresenterDecorator[] {};

			_view = view;
			_rules = rules;

			_columns.Add(ColumnHeader.Name);
			_columns.Add(ColumnHeader.StaffingEffect);
			_columns.Add(ColumnHeader.Color);
			_columns.Add(ColumnHeader.Time);

			decorators.ForEach(d => d.Decorate(this));

			_columns.ForEach(c => c.Index = _columns.IndexOf(c) + 1);
		}

		public IList<Column> Columns { get { return _columns; } }

		public class ColumnHeader
		{
			public static Column Name = new Column { Text = Resources.Name, Get = getName, Update = updateName };
			public static Column StaffingEffect = new Column { Text = Resources.StaffingEffect, Get = getStaffingEffect, Update = updateStaffingEffect };
			public static Column Color = new Column { Text = Resources.Color, Get = getColor, Update = updateColor };
			public static Column Time = new Column { Text = Resources.Time, Get = getTime, Update = updateTime };
		}

		public class Column
		{
			// so we dont have the break the tests who assumed this was an enum
			public static implicit operator int(Column column)
			{
				return column.Index;
			}

			public string Text { get; set; }
			public int Index { get; set; }
			public Action<IRtaRule, GridQueryCellInfoEventArgs> Get { get; set; }
			public Action<IRtaRule, IEnumerable<IRtaRule>, IAlarmControlView, GridSaveCellInfoEventArgs> Update { get; set; }
		}

		public void QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _rules.Count();
			e.Handled = true;
		}

		public void QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = Columns.Count();
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
				var alarmType = _rules.ElementAt(e.RowIndex - 1);
				Columns.Single(c => c.Index == e.ColIndex).Get(alarmType, e);
			}
			e.Handled = true;
		}
		
		private static void getStaffingEffect(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "NumericCell";
			e.Style.CellValue = rule.StaffingEffect;
		}

		private static void getName(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = rule.Description.Name;
		}

		private static void getColor(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ColorPickerCell";
			e.Style.CellValueType = typeof(Color);
			e.Style.CellValue = rule.DisplayColor;
		}

		private static void getTime(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellValue = rule.ThresholdTime;
			e.Style.CellType = "NumericCell";
		}

		private void queryHeader(GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = Columns.Single(c => c.Index == e.ColIndex).Text;
		}

		public void CellClick(object sender, GridCellCancelEventArgs e)
		{
			_view.ShowThisItem(e.RowIndex - 1);
		}

		public void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.RowIndex == 0 || e.ColIndex == 0) return;

			if (!_rules.Any())
				return;

			var column = Columns.SingleOrDefault(c => c.Index == e.ColIndex && c.Update != null);

			if (column != null)
			{
				var alarmType = _rules.ElementAt(e.RowIndex - 1);
				column.Update(alarmType, _rules, _view, e);
			}

			e.Handled = true;
		}

		private static void updateStaffingEffect(IRtaRule alarm, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			alarm.StaffingEffect = (double)e.Style.CellValue;
		}

		private static void updateColor(IRtaRule alarm, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var t = (Color)e.Style.CellValue;
			if (t == Color.Empty) return;
			alarm.DisplayColor = t;
		}

		private static void updateTime(IRtaRule alarm, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var d = (int) e.Style.CellValue;
			if (d < 0) return;

			alarm.ThresholdTime = d;
		}

		private static void updateName(IRtaRule alarm, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
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
