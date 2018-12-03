using System;
using System.Collections.Generic;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class IntervalHeaderGridRowTest
    {
        private IntervalHeaderGridRow _target;
        private IList<IntervalDefinition> _timeSpans;

        [SetUp]
        public void Setup()
        {
            _timeSpans = new List<IntervalDefinition>
                             {
                                 new IntervalDefinition(new DateTime(), TimeSpan.FromHours(1)),
                                 new IntervalDefinition(new DateTime(), TimeSpan.FromHours(2))
                             };
            _target = new IntervalHeaderGridRow(_timeSpans);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(DateTime.MinValue, _target.BaseDate);

            DateTime baseDate = new DateTime(2008,10,23,0,0,0,DateTimeKind.Utc);
            _target.BaseDate = baseDate;
            Assert.AreEqual(baseDate,_target.BaseDate);
        }

        [Test]
        public void VerifyOnQueryCellInfo()
        {
            CellInfo cellInfo = new CellInfo
                                    {
                                        ColIndex = 1,
                                        RowHeaderCount = 2,
                                        Style = new GridStyleInfo()
                                    };
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual(string.Empty, cellInfo.Style.BaseStyle);

            cellInfo.ColIndex = 5;
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual(string.Empty, cellInfo.Style.BaseStyle);

            cellInfo.ColIndex = 3;
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual("Header", cellInfo.Style.BaseStyle);
            Assert.AreEqual("02:00",cellInfo.Style.CellValue);

            cellInfo.Style = new GridStyleInfo();
            _target = new IntervalHeaderGridRow(new List<IntervalDefinition>());
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual(string.Empty, cellInfo.Style.BaseStyle);
        }

        [Test]
        public void VerifyMethodsFromInterface()
        {
            _target.SaveCellInfo(null);
            _target.OnSelectionChanged(null,0);
            Assert.IsTrue(true);
        }
    }
}
