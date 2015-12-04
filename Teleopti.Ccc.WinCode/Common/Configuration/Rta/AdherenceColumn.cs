using System.Collections.Generic;
using System.Collections.Specialized;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration.Rta
{
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

		private static void updateAdherence(IRtaRule rule, IEnumerable<IRtaRule> alarmTypes, IAlarmControlView view, GridSaveCellInfoEventArgs e)
		{
			rule.SetAdherenceByText(e.Style.CellValue as string);
		}

		private static void getAdherence(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ComboBox";
			e.Style.ChoiceList = new StringCollection { Resources.InAdherence, Resources.OutOfAdherence, Resources.NeutralAdherence };
			e.Style.DropDownStyle = GridDropDownStyle.Exclusive;
			e.Style.CellValue = rule.AdherenceText;
		}
	}
}