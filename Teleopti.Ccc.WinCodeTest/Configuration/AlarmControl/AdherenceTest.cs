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
	public class AdherenceTest
	{
		[Test]
		public void ShouldHaveAdherenceColumn()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] {}, new FakeView(), new[] {new AdherenceColumn()});

			target.Columns.SingleOrDefault(x => x.Text == Resources.Adherence).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveAdherenceColumnAfterStaffingEffect()
		{
			var target = new AlarmControlPresenter(new IRtaRule[] {}, new FakeView(), new[] {new AdherenceColumn()});

			var staffingEffect = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.StaffingEffect));
			var adherence = target.Columns.IndexOf(target.Columns.Single(x => x.Text == Resources.Adherence));
			adherence.Should().Be(staffingEffect + 1);
		}

		[Test]
		public void ShouldDisplayInAdherence()
		{
			var rule = new RtaRule {Adherence = Adherence.In};
			var target = new AlarmControlPresenter(new IRtaRule[] {rule}, new FakeView(), new[] {new AdherenceColumn()});
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.InAdherence);
		}

		[Test]
		public void ShouldDisplayOutOfAdherence()
		{
			var rule = new RtaRule {Adherence = Adherence.Out};
			var target = new AlarmControlPresenter(new IRtaRule[] {rule}, new FakeView(), new[] {new AdherenceColumn()});
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.OutOfAdherence);
		}

		[Test]
		public void ShouldDisplayNeutralAdherence()
		{
			var rule = new RtaRule { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			var info = new GridStyleInfo();
			target.QueryCellInfo(this, new GridQueryCellInfoEventArgs(1, adherence.Index, info));

			info.CellValue.Should().Be(Resources.NeutralAdherence);
		}

		[Test]
		public void ShouldBeComboBox()
		{
			var rule = new RtaRule { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
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
			var rule = new RtaRule { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.InAdherence }, new StyleModifyType()));

			rule.Adherence.Should().Be(Adherence.In);
		}

		[Test]
		public void ShouldUpdateOutOfAdherence()
		{
			var rule = new RtaRule { Adherence = Adherence.Neutral };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.OutOfAdherence }, new StyleModifyType()));

			rule.Adherence.Should().Be(Adherence.Out);
		}

		[Test]
		public void ShouldUpdateNeutralAdherence()
		{
			var rule = new RtaRule { Adherence = Adherence.In };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = Resources.NeutralAdherence }, new StyleModifyType()));

			rule.Adherence.Should().Be(Adherence.Neutral);
		}

		[Test]
		public void ShouldUpdateEmpty()
		{
			var rule = new RtaRule { Adherence = Adherence.In };
			var target = new AlarmControlPresenter(new IRtaRule[] { rule }, new FakeView(), new[] { new AdherenceColumn() });
			var adherence = target.Columns.Single(x => x.Text == Resources.Adherence);

			target.SaveCellInfo(this, new GridSaveCellInfoEventArgs(1, adherence.Index, new GridStyleInfo { CellValue = string.Empty }, new StyleModifyType()));

			rule.Adherence.Should().Be(null);
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

		public void RefreshRow(int rowIndex)
		{
		}
	}
}