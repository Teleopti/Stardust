using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration.AlarmControl
{
	[TestFixture]
	public class AdherenceTest
	{
		[Test]
		public void ShouldHaveAdherenceColumn()
		{
			var target = new AlarmControlPresenter(new IAlarmType[] {}, new FakeView(), new[] {new AdherenceColumn()});

			target.Columns.SingleOrDefault(x => x.Text == Resources.Adherence).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveAdherenceColumnAfterStaffingEffect()
		{
			var target = new AlarmControlPresenter(new IAlarmType[] {}, new FakeView(), new[] {new AdherenceColumn()});

			var staffingEffect = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.StaffingEffect));
			var adherence = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.Adherence));
			adherence.Should().Be(staffingEffect + 1);
		}

		[Test]
		public void ShouldDisplayInAdherence()
		{
			var alarmType = new AlarmType {Adherence = Adherence.In};
			var target = new AlarmControlPresenter(new IAlarmType[] {alarmType}, new FakeView(), new[] {new AdherenceColumn()});
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.InAdherence);
		}

		[Test]
		public void ShouldDisplayOutOfAdherence()
		{
			var alarmType = new AlarmType {Adherence = Adherence.Out};
			var target = new AlarmControlPresenter(new IAlarmType[] {alarmType}, new FakeView(), new[] {new AdherenceColumn()});
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.OutOfAdherence);
		}

		[Test]
		public void ShouldDisplayNeutralAdherence()
		{
			var alarmType = new AlarmType { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IAlarmType[] { alarmType }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.NeutralAdherence);
		}

		[Test]
		public void ShouldBeComboBox()
		{
			var alarmType = new AlarmType { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IAlarmType[] { alarmType }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellType.Should().Be("ComboBox");
			info.ChoiceList.Cast<string>()
				.Should()
				.Have.SameValuesAs(new[] {Resources.InAdherence, Resources.OutOfAdherence, Resources.NeutralAdherence});
			info.DropDownStyle.Should().Be(GridDropDownStyle.Exclusive);
		}

		[Test]
		public void ShouldUpdateInAdherence()
		{
			var alarmType = new AlarmType { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IAlarmType[] { alarmType }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.InAdherence }, new StyleModifyType()));

			alarmType.Adherence.Should().Be(Adherence.In);
		}

		[Test]
		public void ShouldUpdateOutOfAdherence()
		{
			var alarmType = new AlarmType { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IAlarmType[] { alarmType }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.OutOfAdherence }, new StyleModifyType()));

			alarmType.Adherence.Should().Be(Adherence.Out);
		}

		[Test]
		public void ShouldUpdateNeutralAdherence()
		{
			var alarmType = new AlarmType { Adherence = Adherence.In };
			var target = new AlarmControlPresenter(new IAlarmType[] { alarmType }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.NeutralAdherence }, new StyleModifyType()));

			alarmType.Adherence.Should().Be(Adherence.Neutral);
		}
	}

	public class FakeView : IAlarmControlView
	{
		public void ShowThisItem(int alarmListItemId)
		{
		}

		public void Warning(string message)
		{
		}
	}
}