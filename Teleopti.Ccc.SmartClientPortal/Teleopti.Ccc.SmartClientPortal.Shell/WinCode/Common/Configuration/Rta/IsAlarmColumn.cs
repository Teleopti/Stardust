using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta
{
	public class IsAlarmColumn : IAlarmControlPresenterDecorator
	{
		public void Decorate(AlarmControlPresenter alarmControlPresenter)
		{
			var threshold = alarmControlPresenter.Columns.ToList().FindIndex(x => x.Text == Resources.Threshold);
			alarmControlPresenter.Columns.Insert(threshold + 1,
				new AlarmControlPresenter.Column
				{
					Text = Resources.IsAlarm,
					Get = getCellValue,
					Update = updateIsAlarm
				});
		}

		private static void updateIsAlarm(IRtaRule rule, IEnumerable<IRtaRule> rules, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			rule.IsAlarm = (bool)e.Style.CellValue;
			view.RefreshRow(e.RowIndex);
		}

		private static void getCellValue(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "CheckBox";
			e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
			e.Style.CellValueType = typeof(bool);
			e.Style.CheckBoxOptions.CheckedValue = bool.TrueString;
			e.Style.CheckBoxOptions.UncheckedValue = bool.FalseString;
			e.Style.CellValue = rule.IsAlarm;
		}
	}
}