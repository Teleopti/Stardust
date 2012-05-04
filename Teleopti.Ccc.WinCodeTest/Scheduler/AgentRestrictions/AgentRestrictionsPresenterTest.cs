using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsPresenterTest
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsView _view;
		private IAgentRestrictionsModel _model;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = _mocks.StrictMock<IAgentRestrictionsModel>();
			_view = _mocks.StrictMock<IAgentRestrictionsView>();
			_presenter = new AgentRestrictionsPresenter(_view, _model);
		}

		[Test]
		public void ShouldReturnColCountEqual12()
		{
			Assert.AreEqual(12, _presenter.GridQueryColCount);
		}

		[Test]
		public void ShouldReturnRowCount()
		{
			using (_mocks.Record())
			{
				Expect.Call(_model.DisplayRows).Return(new List<AgentRestrictionsDisplayRow>());
			}

			using (_mocks.Playback())
			{
				var rows = _presenter.GridQueryRowCount;
				Assert.AreEqual(0, rows);
			}
		}

		[Test]
		public void ShouldGetTextBoxAsCellTypeOnHeaders()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					using (var gridStyleInfo = new GridStyleInfo())
					{
						var args = new GridQueryCellInfoEventArgs(i, j, gridStyleInfo);
						_presenter.GridQueryCellInfo(null, args);
						Assert.AreEqual("TextBox", args.Style.CellType);
					}
				}
			}
		}

		[Test]
		public void ShouldHaveHaveCorrectHeaderTextsFirstHeader()
		{
			for (var i = 0; i < 12; i++)
			{
				using (var gridStyleInfo = new GridStyleInfo())
				{
					var args = new GridQueryCellInfoEventArgs(0, i, gridStyleInfo);

					_presenter.GridQueryCellInfo(null, args);
					if (i < 2) Assert.AreEqual(string.Empty, args.Style.Text);
					if (i > 1 && i < 7) Assert.AreEqual(UserTexts.Resources.SchedulePeriod, args.Style.Text);
					if (i > 6 && i < 9) Assert.AreEqual(UserTexts.Resources.Schedule, args.Style.Text);
					if (i > 8 && i < 12) Assert.AreEqual(UserTexts.Resources.SchedulePlusRestrictions, args.Style.Text);
				}
			}
		}

		[Test]
		public void ShouldHaveCorrectHeaderTextsSecondHeader()
		{
			for (var i = 0; i < 12; i++)
			{
				using (var gridStyleInfo = new GridStyleInfo())
				{
					var args = new GridQueryCellInfoEventArgs(1, i, gridStyleInfo);

					_presenter.GridQueryCellInfo(null, args);
					if (i == 0) Assert.AreEqual(UserTexts.Resources.Name, args.Style.Text);
					if (i == 1) Assert.AreEqual(UserTexts.Resources.Warnings, args.Style.Text);
					if (i == 2) Assert.AreEqual(UserTexts.Resources.Type, args.Style.Text);
					if (i == 3) Assert.AreEqual(UserTexts.Resources.From, args.Style.Text);
					if (i == 4) Assert.AreEqual(UserTexts.Resources.To, args.Style.Text);
					if (i == 5) Assert.AreEqual(UserTexts.Resources.ContractTargetTime, args.Style.Text);
					if (i == 6) Assert.AreEqual(UserTexts.Resources.DaysOff, args.Style.Text);
					if (i == 7) Assert.AreEqual(UserTexts.Resources.ContractTime, args.Style.Text);
					if (i == 8) Assert.AreEqual(UserTexts.Resources.DaysOff, args.Style.Text);
					if (i == 9) Assert.AreEqual(UserTexts.Resources.Min, args.Style.Text);
					if (i == 10) Assert.AreEqual(UserTexts.Resources.Max, args.Style.Text);
					if (i == 11) Assert.AreEqual(UserTexts.Resources.DaysOff, args.Style.Text);
					if (i == 12) Assert.AreEqual(UserTexts.Resources.Ok, args.Style.Text);
				}
			}
		}

		[Test]
		public void ShouldHaveCorrectCellTypes()
		{
			for (var i = 0; i < 12; i++)
			{
				using (var gridStyleInfo = new GridStyleInfo())
				{
					var args = new GridQueryCellInfoEventArgs(2, i, gridStyleInfo);
					_presenter.GridQueryCellInfo(null, args);

					if (i == 0) Assert.AreEqual("Header", args.Style.CellType);
					if (i == 1) Assert.AreEqual("NumericReadOnlyCellModel", args.Style.CellType);
					if (i == 2) Assert.AreEqual("Static", args.Style.CellType);
					if (i == 3) Assert.AreEqual("Static", args.Style.CellType);
					if (i == 4) Assert.AreEqual("Static", args.Style.CellType);
					if (i == 5) Assert.AreEqual("TimeSpan", args.Style.CellType);
					if (i == 6) Assert.AreEqual("NumericReadOnlyCellModel", args.Style.CellType);
					if (i == 7) Assert.AreEqual("TimeSpan", args.Style.CellType);
					if (i == 8) Assert.AreEqual("NumericReadOnlyCellModel", args.Style.CellType);
					if (i == 9) Assert.AreEqual("TimeSpan", args.Style.CellType);
					if (i == 10) Assert.AreEqual("TimeSpan", args.Style.CellType);
					if (i == 11) Assert.AreEqual("NumericReadOnlyCellModel", args.Style.CellType);
					if (i == 12) Assert.AreEqual("Static", args.Style.CellType);
				}
			}
		}
	}
}
