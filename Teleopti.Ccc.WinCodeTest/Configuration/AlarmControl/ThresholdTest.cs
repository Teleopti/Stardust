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
	public class ThresholdTest
	{
		[Test]
		public void ShouldDisplayThresholdTextInColumnHeader()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(), new[] { new ThresholdColumn() });

			target.Columns.SingleOrDefault(x => x.Text == Resources.Threshold).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotDisplayTimeColumn()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(), new[] { new ThresholdColumn() });

			target.Columns.SingleOrDefault(x => x.Text == Resources.Time).Should().Be.Null();
		}

		[Test]
		public void ShouldHaveThresholdColumnAfterColorColumn()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] { }, new FakeView(), new[] { new ThresholdColumn() });

			var color = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.Color));
			var threshold = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.Threshold));
			threshold.Should().Be(color + 1);
		}

		[Test]
		public void ShouldDisplayThreshold()
		{
			var rule = new RtaRule { ThresholdTime = 60 };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new ThresholdColumn() });
			var threshold = target.Columns.Single(x => x.Text == Resources.Threshold);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, threshold.Index, info));

			info.CellValue.Should().Be(60);
		}

		[Test]
		public void ShouldUpdateThreshold()
		{
			var rule = new RtaRule();
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new ThresholdColumn() });
			var threshold = target.Columns.Single(x => x.Text == Resources.Threshold);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, threshold.Index, new GridStyleInfo { CellValue = 60 }, new StyleModifyType()));

			rule.ThresholdTime.Should().Be(60);
		}

		[Test]
		public void ShouldNotUpdateWithNegativeNumber()
		{
			var rule = new RtaRule();
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new ThresholdColumn() });
			var threshold = target.Columns.Single(x => x.Text == Resources.Threshold);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, threshold.Index, new GridStyleInfo { CellValue = -1 }, new StyleModifyType()));

			rule.ThresholdTime.Should().Be(0);
		}
	}
}
