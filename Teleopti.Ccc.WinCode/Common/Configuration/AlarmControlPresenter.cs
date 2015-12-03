using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public interface IAlarmControlPresenterDecorator
	{
		void Decorate(AlarmControlPresenter alarmControlPresenter);
	}

	public class AlarmControlPresenter
	{
		private readonly IAlarmControlView _view;
		private readonly IEnumerable<IRtaRule> _alarmTypes;
		private readonly IList<Column> _columns = new List<Column>();

		public AlarmControlPresenter(IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, IEnumerable<IAlarmControlPresenterDecorator> decorators)
		{
			decorators = decorators ?? new IAlarmControlPresenterDecorator[] {};

			_view = view;
			_alarmTypes = alarmTypes;

			_columns.Add(ColumnHeader.Name);
			_columns.Add(ColumnHeader.StaffingEffect);
			_columns.Add(ColumnHeader.Color);
			_columns.Add(ColumnHeader.Time);
			_columns.Add(ColumnHeader.UpdatedBy);
			_columns.Add(ColumnHeader.UpdatedOn);

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
			public static Column UpdatedBy = new Column { Text = Resources.UpdatedBy, Get = getUpdatedBy };
			public static Column UpdatedOn = new Column { Text = Resources.UpdatedOn, Get = getUpdatedOn };
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
			e.Count = _alarmTypes.Count();
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
				var alarmType = _alarmTypes.ElementAt(e.RowIndex - 1);
				Columns.Single(c => c.Index == e.ColIndex).Get(alarmType, e);
			}
			e.Handled = true;
		}
		
		private static void getStaffingEffect(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "NumericCell";
			e.Style.CellValue = _rtaRule.StaffingEffect;
		}

		private static void getName(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.Text = _rtaRule.Description.Name;
		}

		private static void getColor(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ColorPickerCell";
			e.Style.CellValueType = typeof(Color);
			e.Style.CellValue = _rtaRule.DisplayColor;
		}

		private static void getTime(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellValue = _rtaRule.ThresholdTime.TotalSeconds;
			e.Style.CellType = "NumericCell";
		}

		private static void getUpdatedBy(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (_rtaRule.UpdatedBy != null)
				e.Style.CellValue = _rtaRule.UpdatedBy.Name;
		}

		private static void getUpdatedOn(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "Static";
			if (_rtaRule.UpdatedOn.HasValue)
				e.Style.CellValue = new LocalizedUpdateInfo().UpdatedTimeInUserPerspective(_rtaRule);
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

			if (!_alarmTypes.Any())
				return;

			var column = Columns.SingleOrDefault(c => c.Index == e.ColIndex && c.Update != null);

			if (column != null)
			{
				var alarmType = _alarmTypes.ElementAt(e.RowIndex - 1);
				column.Update(alarmType, _alarmTypes, _view, e);
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
			var d = (double)e.Style.CellValue;
			if (d < 0) return;

			alarm.ThresholdTime = TimeSpan.FromSeconds(d);
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

	public class AdherenceColumn : IAlarmControlPresenterDecorator
	{
		public void Decorate(AlarmControlPresenter alarmControlPresenter)
		{
			var staffingEffect = alarmControlPresenter.Columns.IndexOf(AlarmControlPresenter.ColumnHeader.StaffingEffect);
			alarmControlPresenter.Columns.Insert(staffingEffect + 1,
				new AlarmControlPresenter.Column
				{
					Text = Resources.Adherence,
					Get = getAdherence,
					Update = updateAdherence
				});
		}

		private static void updateAdherence(IRtaRule _rtaRule, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			_rtaRule.SetAdherenceByText(e.Style.CellValue as string);
		}

		private static void getAdherence(IRtaRule _rtaRule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ComboBox";
			e.Style.ChoiceList = new StringCollection { Resources.InAdherence, Resources.OutOfAdherence, Resources.NeutralAdherence };
			e.Style.DropDownStyle = GridDropDownStyle.Exclusive;
			e.Style.CellValue = _rtaRule.AdherenceText;
		}
	}

}
