using NUnit.Framework;
using Teleopti.Ccc.WinCode.Presentation;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportDetailTest
    {
        private ReportDetail _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportDetail {File = "my_report.rdlc", Function = "raptor/onlinereports/myreport", DisplayName = "My Report"};
        }

        [Test]
        public void ShouldBeAbleToGetAndSetProperties()
        {
            Assert.AreEqual("my_report.rdlc", _target.File);
            Assert.AreEqual("raptor/onlinereports/myreport", _target.Function);
            Assert.AreEqual("My Report", _target.DisplayName);

            _target.File = "new_report.rdlc";
            _target.Function = "new/report/function";
            _target.DisplayName = "My New Report";

            Assert.AreEqual("new_report.rdlc", _target.File);
            Assert.AreEqual("new/report/function", _target.Function);
            Assert.AreEqual("My New Report", _target.DisplayName);
        }
    }
}
