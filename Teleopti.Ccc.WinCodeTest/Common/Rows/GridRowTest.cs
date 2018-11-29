using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class GridRowTest
    {
        private GridRow _target;
        private IRowManager _rowManager;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _target = new GridRow("MyType", "Display", "MyHeaderText");
            _mocks = new MockRepository();
            _rowManager = _mocks.StrictMock<IRowManager>();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("MyType", _target.CellType);
            Assert.AreEqual("Display", _target.DisplayMember);
            Assert.AreEqual("MyHeaderText", _target.RowHeaderText);

            ChartSeriesSetting seriesSetting = new ChartSeriesSetting("test", Color.DimGray, ChartSeriesDisplayType.Bar, true, AxisLocation.Right);
            _target.ChartSeriesSettings = seriesSetting;

            Assert.AreEqual(seriesSetting, _target.ChartSeriesSettings);
        }

        [Test]
        public void VerifyQueryCellInfo()
        {
            CellInfo cellInfo = new CellInfo { ColIndex = 0, Style = new GridStyleInfo() };
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual("MyHeaderText", cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyMethodsFromInterface()
        {
            _target.SaveCellInfo(null);
            _target.OnSelectionChanged(null, 0);
            Assert.IsTrue(true);
        }

        [Test]
        public void VerifyVerticalHeaderSettings()
        {
            CellInfo cellInfo = new CellInfo { Style = new GridStyleInfo() };
            string parentRowHeaderText = string.Concat("Parent", _target.RowHeaderText);
            GridRow.VerticalRowHeaderSettings(cellInfo, parentRowHeaderText);

            Assert.AreEqual(GridMergeCellDirection.RowsInColumn, cellInfo.Style.MergeCell);
            Assert.AreEqual(GridVerticalAlignment.Middle, cellInfo.Style.VerticalAlignment);
            Assert.AreEqual(GridHorizontalAlignment.Center, cellInfo.Style.HorizontalAlignment);
            Assert.AreEqual(270, cellInfo.Style.Font.Orientation);
            Assert.AreEqual(parentRowHeaderText, cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyGetColSpan()
        {
            ITeleoptiGridControl teleoptiGridControl = _mocks.StrictMock<ITeleoptiGridControl>();

            using (_mocks.Record())
            {
                Expect.Call(_rowManager.Grid).Return(teleoptiGridControl).Repeat.AtLeastOnce();
                Expect.Call(teleoptiGridControl.ColCount).Return(2).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.IntervalLength).Return(15).Repeat.AtLeastOnce();
            }

            DateTime date = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period = new DateTimePeriod(date, date);
            using (_mocks.Playback())
            {
                int result = GridRow.GetColSpan(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(15)));
                Assert.AreEqual(1, result);

                result = GridRow.GetColSpan(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(30)));
                Assert.AreEqual(2, result);

                result = GridRow.GetColSpan(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(45)));
                Assert.AreEqual(2, result);
            }
        }

        [Test]
        public void VerifyGetStartPosition()
        {
            ITeleoptiGridControl teleoptiGridControl = _mocks.StrictMock<ITeleoptiGridControl>();

            DateTime date = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc);

            using (_mocks.Record())
            {
                Expect.Call(_rowManager.Grid).Return(teleoptiGridControl).Repeat.AtLeastOnce();
                Expect.Call(teleoptiGridControl.ColCount).Return(7).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.Intervals).Return(new List<IntervalDefinition>
                                                             {
                                                                 new IntervalDefinition(date, TimeSpan.Zero),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15)),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(30)), TimeSpan.FromMinutes(30)),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(45)), TimeSpan.FromMinutes(45)),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(60)), TimeSpan.FromMinutes(60))
                                                             }).Repeat.AtLeastOnce();
            }

            DateTimePeriod period = new DateTimePeriod(date, date);
            using (_mocks.Playback())
            {
                int visibleColSpan = 1;
                int result = GridRow.GetStartPosition(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(15)), 2, ref visibleColSpan);
                Assert.AreEqual(2, result);
                Assert.AreEqual(1, visibleColSpan);

                visibleColSpan = 2;
                result = GridRow.GetStartPosition(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(45)).ChangeStartTime(TimeSpan.FromMinutes(15)), 2, ref visibleColSpan);
                Assert.AreEqual(3, result);
                Assert.AreEqual(2, visibleColSpan);

                visibleColSpan = 7;
                result = GridRow.GetStartPosition(_rowManager, period.ChangeEndTime(TimeSpan.FromMinutes(75)), 2, ref visibleColSpan);
                Assert.AreEqual(2, result);
                Assert.AreEqual(6, visibleColSpan);
            }
        }

        [Test]
        public void VerifyGetObjectAtPosition()
        {
            IRowManager<IGridRow, int> typedRowManager = _mocks.StrictMock<IRowManager<IGridRow, int>>();

            using (_mocks.Record())
            {
                Expect.Call(typedRowManager.DataSource).Return(new List<int> { 4, 3, 2, 1 }).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                int result = GridRow.GetObjectAtPosition(typedRowManager, 2, 1);
                Assert.AreEqual(3, result);

                result = GridRow.GetObjectAtPosition(typedRowManager, -2, 1);
                Assert.AreEqual(4, result);

                result = GridRow.GetObjectAtPosition(typedRowManager, 4, 1);
                Assert.AreEqual(1, result);
            }
        }

        [Test]
        public void VerifyGetObjectAtPositionForInterval()
        {
            DateTime date = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            IRowManager<IGridRow, IPeriodized> typedRowManager = _mocks.StrictMock<IRowManager<IGridRow, IPeriodized>>();
            IPeriodized periodized = _mocks.StrictMock<IPeriodized>();
            using (_mocks.Record())
            {
                Expect.Call(typedRowManager.Intervals).Return(new List<IntervalDefinition>
                                                                  {
                                                                      new IntervalDefinition(date, TimeSpan.Zero),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15)),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(30)), TimeSpan.FromMinutes(30))
                                                                  }).
                    Repeat.AtLeastOnce();
                Expect.Call(periodized.Period).Return(
                    new DateTimePeriod(date, date).MovePeriod(TimeSpan.FromMinutes(15)).ChangeEndTime(
                        TimeSpan.FromMinutes(15))).Repeat.AtLeastOnce();
                Expect.Call(typedRowManager.DataSource).Return(new List<IPeriodized> { periodized }).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                IPeriodized result = _target.GetObjectAtPositionForInterval(typedRowManager, 2, 1);
                Assert.AreEqual(periodized, result);

                result = _target.GetObjectAtPositionForInterval(typedRowManager, 3, 1);
                Assert.AreEqual(periodized, result);
            }
        }

        [Test]
        public void VerifyGetObjectAtPositionForIntervalWithoutMerge()
        {
            _target = new GridRowWithoutMergeForTest("MyType", "Display", "MyHeaderText");
            
            DateTime date = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc);
            IRowManager<IGridRow, IPeriodized> typedRowManager = _mocks.StrictMock<IRowManager<IGridRow, IPeriodized>>();
            IPeriodized periodized = _mocks.StrictMock<IPeriodized>();
            using (_mocks.Record())
            {
                Expect.Call(typedRowManager.Intervals).Return(new List<IntervalDefinition> { 
                                                                 new IntervalDefinition(date, TimeSpan.Zero),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15)),
                                                                 new IntervalDefinition(date.Add(TimeSpan.FromMinutes(30)), TimeSpan.FromMinutes(30)) 
                }).
                    Repeat.AtLeastOnce();
                Expect.Call(periodized.Period).Return(
                    new DateTimePeriod(date, date).MovePeriod(TimeSpan.FromMinutes(15)).ChangeEndTime(
                        TimeSpan.FromMinutes(15))).Repeat.AtLeastOnce();
                Expect.Call(typedRowManager.DataSource).Return(new List<IPeriodized> { periodized }).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                IPeriodized result = _target.GetObjectAtPositionForInterval(typedRowManager, 2, 1);
                Assert.AreEqual(periodized, result);

                Assert.IsNull(_target.GetObjectAtPositionForInterval(typedRowManager,3,1));
            }
        }

        private class GridRowWithoutMergeForTest : GridRow
        {
            public GridRowWithoutMergeForTest(string cellType, string displayMember, string rowHeaderText)
                : base(cellType, displayMember, rowHeaderText)
            {
            }

            protected override bool AllowMerge
            {
                get
                {
                    return false;
                }
            }
        }
    }
}
