using System;
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
            var id = new Guid("F1A2966C-E241-4DAC-88CF-83ECA0DEA0EB");
            _target.Id = id;
            Assert.AreEqual(id,_target.Id);
            _target.ReportName = "Agent KPI";
            Assert.AreEqual("Agent KPI", _target.ReportName);
            _target.ReportUrl = "http://matrixhost/agentKPI.aspx";
            Assert.AreEqual("http://matrixhost/agentKPI.aspx", _target.ReportUrl);
            _target.TargetFrame = "_blank";
            Assert.AreEqual("_blank", _target.TargetFrame);
        }
        
    }
}