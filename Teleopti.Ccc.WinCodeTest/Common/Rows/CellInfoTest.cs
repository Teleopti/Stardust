using NUnit.Framework;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class CellInfoTest
    {
        private CellInfo _target;

        [SetUp]
        public void Setup()
        {
            _target = new CellInfo();
        }

        [Test]
        public void VerifyProperties()
        {
            GridStyleInfo styleInfo = new GridStyleInfo();
            _target.Style = styleInfo;
            _target.ColIndex = 3;
            _target.RowIndex = 4;
            _target.Handled = true;
            _target.ColCount = 10;
            _target.RowCount = 5;
            _target.ColHeaderCount = 2;
            _target.RowHeaderCount = 1;

            Assert.AreEqual(styleInfo,_target.Style);
            Assert.AreEqual(3,_target.ColIndex);
            Assert.AreEqual(4,_target.RowIndex);
            Assert.AreEqual(10, _target.ColCount);
            Assert.AreEqual(5, _target.RowCount);
            Assert.AreEqual(2, _target.ColHeaderCount);
            Assert.AreEqual(1, _target.RowHeaderCount);
            Assert.IsTrue(_target.Handled);
        }
    }
}
