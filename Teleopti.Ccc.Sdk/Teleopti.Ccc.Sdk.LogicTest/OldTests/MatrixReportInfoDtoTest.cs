using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class MatrixReportInfoDtoTest
    {
        private MatrixReportInfoDto _target;

        [SetUp]
        public void Setup()
        {
            _target = new MatrixReportInfoDto();
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            _target.ReportId = 1;
            Assert.AreEqual(1,_target.ReportId);
            _target.ReportName = "Agent KPI";
            Assert.AreEqual("Agent KPI", _target.ReportName);
            _target.ReportUrl = "http://matrixhost/agentKPI.aspx";
            Assert.AreEqual("http://matrixhost/agentKPI.aspx", _target.ReportUrl);
            _target.TargetFrame = "_blank";
            Assert.AreEqual("_blank", _target.TargetFrame);
        }
        
    }
}