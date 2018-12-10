using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta
{
	public class AlarmColorColumn : IAlarmControlPresenterDecorator
	{
		public const int DisabledOpacity = 50;

		public void Decorate(AlarmControlPresenter alarmControlPresenter)
		{
			var isAlarm = alarmControlPresenter.Columns.ToList().FindIndex(x => x.Text == Resources.IsAlarm);
			alarmControlPresenter.Columns.Insert(isAlarm + 1,
				new AlarmControlPresenter.Column
				{
					Text = Resources.AlarmColor,
					Get = getColor,
					Update = updateColor
				});
		}

		private static void updateColor(IRtaRule rule, IEnumerable<IRtaRule> rules, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			rule.AlarmColor = (Color)e.Style.CellValue;
		}

		private static void getColor(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.Enabled = rule.IsAlarm;
			e.Style.CellType = "ColorPickerCell";
			e.Style.CellValueType = typeof (Color);
			e.Style.CellValue = rule.IsAlarm ? rule.AlarmColor : Color.FromArgb(DisabledOpacity, rule.AlarmColor);
		}
	}
}