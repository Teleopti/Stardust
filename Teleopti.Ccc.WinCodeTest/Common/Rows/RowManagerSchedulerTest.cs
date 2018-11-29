using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class RowManagerSchedulerTest
    {
        private RowManager<IGridRow, int> _target;
        private ITeleoptiGridControl _grid;
        private ISchedulerStateHolder _schedulerState;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _grid = _mocks.StrictMock<ITeleoptiGridControl>();
            _schedulerState = _mocks.StrictMock<ISchedulerStateHolder>();
            _target = new RowManagerScheduler<IGridRow, int>(_grid, new List<IntervalDefinition>(), 15, _schedulerState);
        }

        [Test] 
        public void VerifyTimeZoneInfo()
        {
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            using (_mocks.Record())
            {
                Expect.Call(_schedulerState.TimeZoneInfo).Return(timeZoneInfo);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(timeZoneInfo.DisplayName, _target.TimeZoneInfo.DisplayName);
            }
        }


    }
}