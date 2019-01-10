using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class RowManagerTest
    {
        private MockRepository mocks;
        private RowManager<IGridRow, int> _target;
        private ITeleoptiGridControl _teleoptiGridControl;
        private IList<IntervalDefinition> _timeSpanList;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _teleoptiGridControl = mocks.StrictMock<ITeleoptiGridControl>();
            _timeSpanList = new List<IntervalDefinition>
                                {
                                    new IntervalDefinition(new DateTime(), TimeSpan.FromHours(1)),
                                    new IntervalDefinition(new DateTime(), TimeSpan.FromHours(2))
                                };
            mocks.ReplayAll();
            _target = new RowManager<IGridRow, int>(_teleoptiGridControl, _timeSpanList, 15);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(15,_target.IntervalLength);
            Assert.AreEqual(_teleoptiGridControl,_target.Grid);
            Assert.AreEqual(_timeSpanList,_target.Intervals);
            Assert.AreEqual(DateTime.MinValue,_target.BaseDate);

            DateTime baseDate = new DateTime(2008,10,23,0,0,0,DateTimeKind.Utc);
            IList<IntervalDefinition> timeSpans = new List<IntervalDefinition>
                                            {
                                                new IntervalDefinition(new DateTime(), TimeSpan.FromHours(3)),
                                                new IntervalDefinition(new DateTime(), TimeSpan.FromHours(4))
                                            };
            _target.IntervalLength = 20;
            _target.Intervals = timeSpans;
            _target.BaseDate = baseDate;

            Assert.AreEqual(20,_target.IntervalLength);
            Assert.AreEqual(timeSpans,_target.Intervals);
            Assert.AreEqual(baseDate,_target.BaseDate);
        }

        [Test]
        public void VerifyCanSetDataSource()
        {
            Assert.AreEqual(0,_target.DataSource.Count);
            IList<int> intList = new List<int>{1,2,3,4};
            _target.SetDataSource(intList);
            Assert.AreEqual(intList, _target.DataSource);
            _target.SetDataSource(null);
            Assert.AreEqual(0,_target.DataSource.Count);
        }

        [Test]
        public void VerifyCanAddRows()
        {
            Assert.AreEqual(0,_target.Rows.Count);
            IGridRow gridRow = mocks.StrictMock<IGridRow>();
            var result = _target.AddRow(gridRow);
            Assert.AreEqual(gridRow,result);
            Assert.AreEqual(1,_target.Rows.Count);
            Assert.AreEqual(gridRow,_target.Rows[0]);
        }

        [Test]
        public void VerifyTimeZoneInfo()
        {
            TimeZoneInfo timeZoneInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
            Assert.AreEqual(timeZoneInfo.DisplayName, _target.TimeZoneInfo.DisplayName);
        }
    }
}
