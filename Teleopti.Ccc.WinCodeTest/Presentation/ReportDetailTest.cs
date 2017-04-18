using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportDetailTest
    {
        private ReportDetail _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportDetail {File = "my_report.rdlc", FunctionPath = "raptor/onlinereports/myreport", DisplayName = "My Report", FunctionCode = "myreport"};
        }

        [Test]
        public void ShouldBeAbleToGetAndSetProperties()
        {
            Assert.AreEqual("my_report.rdlc", _target.File);
            Assert.AreEqual("raptor/onlinereports/myreport", _target.FunctionPath);
            Assert.AreEqual("My Report", _target.DisplayName);
            Assert.AreEqual("myreport", _target.FunctionCode);

            _target.File = "new_report.rdlc";
            _target.FunctionPath = "new/report/function";
            _target.DisplayName = "My New Report";
        	_target.FunctionCode = "function";

            Assert.AreEqual("new_report.rdlc", _target.File);
            Assert.AreEqual("new/report/function", _target.FunctionPath);
            Assert.AreEqual("My New Report", _target.DisplayName);
            Assert.AreEqual("function", _target.FunctionCode);
        }
    }
}
