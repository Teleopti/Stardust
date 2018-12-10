using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.Rta;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration.AlarmControl
{
	[TestFixture]
	public class AlarmColorTest
	{
		[Test]
		public void ShouldHaveAlarmColor()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(), new[] { new AlarmColorColumn() });

			target.Columns.SingleOrDefault(x => x.Text == Resources.AlarmColor).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveIsAlarmColumnAfterIsAlarmColumn()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(),
				new IAlarmControlPresenterDecorator[] { new IsAlarmColumn(), new AlarmColorColumn()});

			var isAlarm = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.IsAlarm));
			var alarmColor = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.AlarmColor));
			alarmColor.Should().Be(isAlarm + 1);
		}


		[Test]
		public void ShouldDisplayAlarmColor()
		{
			var rule = new RtaRule { AlarmColor = Color.Aqua, IsAlarm = true };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AlarmColorColumn() });
			var alarmColor = target.Columns.Single(x => x.Text == Resources.AlarmColor);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, alarmColor.Index, info));

			info.CellValue.Should().Be(Color.Aqua);
		}

		[Test]
		public void ShouldUpdateAlarmColor()
		{
			var rule = new RtaRule { AlarmColor = Color.Aqua };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AlarmColorColumn() });
			var alarmColor = target.Columns.Single(x => x.Text == Resources.AlarmColor);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, alarmColor.Index, new GridStyleInfo { CellValue = Color.AliceBlue }, new StyleModifyType()));

			rule.AlarmColor.Should().Be(Color.AliceBlue);
		}

		[Test]
		public void ShouldBeDisabledWhenItIsNotAlarm()
		{
			var rule = new RtaRule {AlarmColor = Color.Aqua, IsAlarm = false};
			var target = new AlarmControlPresenter(new IRtaRule[] {rule}, new FakeView(), new[] {new AlarmColorColumn()});
			var alarmColor = target.Columns.Single(x => x.Text == Resources.AlarmColor);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, alarmColor.Index, info));

			info.Enabled.Should().Be.False();
		}

		[Test]
		public void ShouldShowColumnIsDisabledIfNotAlarm()
		{
			var rule = new RtaRule {AlarmColor = Color.Aqua, IsAlarm = false};
			var target = new AlarmControlPresenter(new IRtaRule[] {rule}, new FakeView(), new[] {new AlarmColorColumn()});
			var alarmColor = target.Columns.Single(x => x.Text == Resources.AlarmColor);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, alarmColor.Index, info));

			info.CellValue.Should().Be(Color.FromArgb(AlarmColorColumn.DisabledOpacity, Color.Aqua));
		}
	}
}