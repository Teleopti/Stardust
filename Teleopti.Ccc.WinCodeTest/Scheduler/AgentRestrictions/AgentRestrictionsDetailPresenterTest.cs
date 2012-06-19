using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailPresenterTest : IDisposable
	{
		private AgentRestrictionsDetailPresenter _presenter;
		private IAgentRestrictionsDetailView _view;
		private IAgentRestrictionsDetailModel _model;
		private MockRepository _mocks;
		private ISchedulerStateHolder _schedulerStateHolder;
		private IGridlockManager _gridlockManager;
		private ClipHandler<IScheduleDay> _clipHandler;
		private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private GridStyleInfo _info;
		private Dictionary<int, IPreferenceCellData> _detailData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IAgentRestrictionsDetailView>();
			_model = _mocks.StrictMock<IAgentRestrictionsDetailModel>();
			_schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_gridlockManager = _mocks.StrictMock<IGridlockManager>();
			_clipHandler = new ClipHandler<IScheduleDay>();
			_overriddenBusinessRulesHolder = _mocks.StrictMock<IOverriddenBusinessRulesHolder>();
			_scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
			_presenter = new AgentRestrictionsDetailPresenter(_view, _model, _schedulerStateHolder, _gridlockManager, _clipHandler, SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
			_info = new GridStyleInfo();
			_detailData = new Dictionary<int, IPreferenceCellData>();
		}

		[Test]
		public void ShouldHaveSevenCols()
		{
			Assert.AreEqual(7, _presenter.ColCount);	
		}

		[Test]
		public void ShouldReturnRowCount()
		{
			_detailData.Add(0, null);
			_detailData.Add(1, null);

			using(_mocks.Record())
			{
				Expect.Call(_model.DetailData()).Return(_detailData);
			}

			using(_mocks.Playback())
			{
				//because of constructor in SchedulePresenterBase
				var period = _schedulerStateHolder.RequestedPeriod;
				Assert.AreEqual(1, _presenter.RowCount);	
			}	
		}

		[Test]
		public void ShouldQueryCellInfo()
		{
			//just testing testdata at the moment
			var e = new GridQueryCellInfoEventArgs(-1, -1, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual(string.Empty, e.Style.CellValue.ToString());

			e = new GridQueryCellInfoEventArgs(0, 0, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual(string.Empty, e.Style.CellValue.ToString());

			e = new GridQueryCellInfoEventArgs(0, 1, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual("--Veckodag--", e.Style.CellValue.ToString());

			e = new GridQueryCellInfoEventArgs(1, 0, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual("--VeckoNum--", e.Style.CellValue.ToString());

			e = new GridQueryCellInfoEventArgs(1, 1, _info);
			_presenter.QueryCellInfo(null, e);
			Assert.AreEqual("--ScheduleDay--", e.Style.CellValue.ToString());
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_info.Dispose();
			}
		}
	}
}
