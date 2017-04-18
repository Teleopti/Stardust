using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsPresenterTest
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsView _view;
		private IAgentRestrictionsModel _model;
		private IAgentRestrictionsWarningDrawer _warningDrawer;
		private IAgentRestrictionsDrawer _loadingDrawer;
		private IAgentRestrictionsDrawer _notAvailableDrawer;
		private IAgentRestrictionsDrawer _availableDrawer;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow;
			
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = _mocks.StrictMock<IAgentRestrictionsModel>();
			_view = _mocks.StrictMock<IAgentRestrictionsView>();
			_warningDrawer = _mocks.StrictMock<IAgentRestrictionsWarningDrawer>();
			_loadingDrawer = _mocks.StrictMock<IAgentRestrictionsDrawer>();
			_notAvailableDrawer = _mocks.StrictMock<IAgentRestrictionsDrawer>();
			_availableDrawer = _mocks.StrictMock<IAgentRestrictionsDrawer>();
			_presenter = new AgentRestrictionsPresenter(_view, _model, _warningDrawer, _loadingDrawer, _notAvailableDrawer, _availableDrawer);
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_agentRestrictionsDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgument()
		{
			Assert.Throws<ArgumentNullException>(() => _presenter.GridQueryCellInfo(null, null));		
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
				Assert.AreEqual(1, rows);
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
				using(_mocks.Record())
				{
					Expect.Call(_model.DisplayRowFromRowIndex(0)).Return(_agentRestrictionsDisplayRow).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(_notAvailableDrawer.Draw(_view, null, null)).Return(false).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(_loadingDrawer.Draw(_view, null, null)).Return(false).IgnoreArguments().Repeat.AtLeastOnce();
					Expect.Call(_availableDrawer.Draw(_view, null, null)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
				}

				using(_mocks.Playback()) 
				{
					using (var gridStyleInfo = new GridStyleInfo())
					{
						var args = new GridQueryCellInfoEventArgs(2, i, gridStyleInfo);
						_presenter.GridQueryCellInfo(null, args);
						Setup();
					}
				}
			}
		}

		[Test]
		public void ShouldDrawWarnings()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _warningDrawer.Draw(null, _model));
			}

			using(_mocks.Playback())
			{
				_presenter.GridCellDrawn(null);	
			}
		}

		[Test]
		public void ShouldSortAgentName()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _model.SortAgentName(true));
				_view.RefreshGrid();
			}

			using(_mocks.Playback())
			{
				_presenter.Sort(0);	
			}		
		}

		[Test]
		public void ShouldSortWarnings()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortWarnings(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(1);
			}
		}

		[Test]
		public void ShouldSortPeriodType()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortPeriodType(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(2);
			}
		}

		[Test]
		public void ShouldSortStartDate()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortStartDate(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(3);
			}
		}

		[Test]
		public void ShouldSortEndDate()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortEndDate(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(4);
			}
		}

		[Test]
		public void ShouldSortContractTargetTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortContractTargetTime(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(5);
			}
		}

		[Test]
		public void ShouldSortTargetDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortTargetDayOffs(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(6);
			}
		}

		[Test]
		public void ShouldSortContractCurrentTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortContractCurrentTime(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(7);
			}
		}

		[Test]
		public void ShouldSortCurrentDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortCurrentDayOffs(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(8);
			}
		}

		[Test]
		public void ShouldSortMinimumPossibleTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortMinimumPossibleTime(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(9);
			}
		}

		[Test]
		public void ShouldSortMaximumPossibleTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortMaximumPossibleTime(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(10);
			}
		}

		[Test]
		public void ShouldSortScheduleAndRestrictionsDayOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortScheduledAndRestrictionDayOffs(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(11);
			}
		}

		[Test]
		public void ShouldSortOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _model.SortOk(true));
				_view.RefreshGrid();
			}

			using (_mocks.Playback())
			{
				_presenter.Sort(12);
			}
		}
	}
}
