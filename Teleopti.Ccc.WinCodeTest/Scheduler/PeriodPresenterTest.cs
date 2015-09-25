using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class PeriodPresenterTest
    {
        private PeriodPresenter target;
        private MockRepository mocks;
        private IScheduleViewBase viewBase;
        private IScenario scenario;
        private GridlockManager gridlockManager;
        private ClipHandler<IScheduleDay> clipHandlerSchedulePart;
        private SchedulerStateHolder schedulerState;
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;
    	private GridControl _grid;

        [SetUp]
        public void Setup()
        {
            
            mocks = new MockRepository();
			_grid = new GridControl();
			viewBase = mocks.StrictMock<IScheduleViewBase>();
            scenario = mocks.StrictMock<IScenario>();
            gridlockManager = new GridlockManager();
            clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
			schedulerState = new SchedulerStateHolder(scenario, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), new List<IPerson>(), mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuardWrapper());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = mocks.StrictMock<IScheduleDayChangeCallback>();
            target = new PeriodPresenter(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
        }

		[TearDown]
		public void Teardown()
		{
			_grid.Dispose();
		}

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldSetCellToStaticIfNotHeaderOrFixedColumn()
		{
			using (mocks.Record())
			{
				Expect.Call(viewBase.RowHeaders).Return(1).Repeat.AtLeastOnce();
				Expect.Call(viewBase.TheGrid).Return(_grid).Repeat.AtLeastOnce();
			}

			using(mocks.Playback())
			{
				GridStyleInfo style = new GridStyleInfo();
				style.CellType = "CheckBox";
				GridQueryCellInfoEventArgs args = new GridQueryCellInfoEventArgs(0, (int)ColumnType.StartScheduleColumns - 2, style);
				target.QueryCellInfo(this, args);
				Assert.IsFalse(style.CellType == "Static");

				style = new GridStyleInfo();
				args = new GridQueryCellInfoEventArgs(2, (int)ColumnType.StartScheduleColumns, style);
				target.QueryCellInfo(this, args);
				Assert.IsTrue(style.CellType == "Static");

				style.Dispose();
			}
		}
    }
}
