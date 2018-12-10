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
	public class IsAlarmTest
	{
		[Test]
		public void ShouldHaveIsAlarm()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(), new[] { new IsAlarmColumn() });

			target.Columns.SingleOrDefault(x => x.Text == Resources.IsAlarm).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveIsAlarmColumnAfterThresholdColumn()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(),
				new IAlarmControlPresenterDecorator[] { new ThresholdColumn(), new IsAlarmColumn() });

			var threshold = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.Threshold));
			var isAlarm = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.IsAlarm));
			isAlarm.Should().Be(threshold + 1);
		}


		[Test]
		public void ShouldDisplayIsAlarm()
		{
			var rule = new RtaRule { IsAlarm = true };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new IsAlarmColumn() });
			var isAlarm = target.Columns.Single(x => x.Text == Resources.IsAlarm);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, isAlarm.Index, info));

			info.CellValue.Should().Be(true);
		}

		[Test]
		public void ShouldUpdateIsAlarm()
		{
			var rule = new RtaRule { IsAlarm = true };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new IsAlarmColumn() });
			var isAlarm = target.Columns.Single(x => x.Text == Resources.IsAlarm);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, isAlarm.Index, new GridStyleInfo { CellValue = false }, new StyleModifyType()));

			rule.IsAlarm.Should().Be(false);
		}
	}
}