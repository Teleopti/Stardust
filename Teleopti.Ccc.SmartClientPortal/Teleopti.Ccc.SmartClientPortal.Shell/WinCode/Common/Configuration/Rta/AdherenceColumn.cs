using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta
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
			var value = (string) e.Style.CellValue;
			if (value == Resources.InAdherence)
				rule.Adherence = Adherence.In;
			if (value == Resources.OutOfAdherence)
				rule.Adherence = Adherence.Out;
			if (value == Resources.NeutralAdherence)
				rule.Adherence = Adherence.Neutral;
			if (string.IsNullOrEmpty(value))
				rule.Adherence = null;
		}

		private static void getAdherence(IRtaRule rule, GridQueryCellInfoEventArgs e)
		{
			e.Style.CellType = "ComboBox";
			e.Style.ChoiceList = new StringCollection {Resources.InAdherence, Resources.OutOfAdherence, Resources.NeutralAdherence};
			e.Style.DropDownStyle = GridDropDownStyle.Exclusive;
			var resource = resourceForAdherence.SingleOrDefault(x => x.Adherence == rule.Adherence)?.TextResource;
			e.Style.CellValue = resource == null ? string.Empty : Resources.ResourceManager.GetString(resource, Resources.Culture);
		}

		private static readonly IEnumerable<adherenceWithResourceText> resourceForAdherence = new[]
		{
			new adherenceWithResourceText
			{
				Adherence = Adherence.In,
				TextResource = nameof(Resources.InAdherence)
			},
			new adherenceWithResourceText
			{
				Adherence = Adherence.Out,
				TextResource = nameof(Resources.OutOfAdherence)
			},
			new adherenceWithResourceText
			{
				Adherence = Adherence.Neutral,
				TextResource = nameof(Resources.NeutralAdherence)
			}
		};

		private class adherenceWithResourceText
		{
			public Adherence Adherence { get; set; }
			public string TextResource { get; set; }
		}
	}
}