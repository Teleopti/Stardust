using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta
{
	public class ThresholdColumn : IAlarmControlPresenterDecorator
	{
		public void Decorate(AlarmControlPresenter alarmControlPresenter)
		{
			var time = alarmControlPresenter.Columns.IndexOf(AlarmControlPresenter.ColumnHeader.Time);

			alarmControlPresenter.Columns.RemoveAt(time);
			alarmControlPresenter.Columns.Insert(time,
				new AlarmControlPresenter.Column
				{
					Text = Resources.Threshold,
					Get = getTime,
					Update = updateTime
				});
		}

		private static void getTime(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellValue = rule.ThresholdTime;
			e.Style.CellType = "NumericCell";
		}

		private static void updateTime(IRtaRule rule, IEnumerable<IRtaRule> rules, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			var d = Convert.ToInt32(e.Style.CellValue);
			if (d < 0) return;

			rule.ThresholdTime = d;
		}
	}
}