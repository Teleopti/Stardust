using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{

    [TestFixture]
    public class MatrixReportInfoTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private MatrixReportInfo _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = new MatrixReportInfo(Guid.NewGuid(), "Report");
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void VerifyConstructor1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructor2()
        {
            _target = new MatrixReportInfo();
            Assert.IsNotNull(_target);
        }

        #endregion

        #region Static Method Tests


        #endregion

        #region Method Tests

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyFindByReportId()
        {
            var findGuid = Guid.NewGuid();
            IList<MatrixReportInfo> list = new List<MatrixReportInfo>
                                               {
                                                   new MatrixReportInfo(findGuid, "Report1"),
                                                   new MatrixReportInfo(Guid.NewGuid(), "Report2"),
                                                   new MatrixReportInfo(Guid.NewGuid(), "Report3")
                                               };

            MatrixReportInfo foundItem = MatrixReportInfo.FindByReportId(list, findGuid);
            Assert.IsNotNull(foundItem);
            Assert.AreEqual("Report1", foundItem.ReportName);

            foundItem = MatrixReportInfo.FindByReportId(list, Guid.NewGuid());
            Assert.IsNull(foundItem);
        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifyReportId()
        {
            // Declare variable to hold property set method
           var setValue = Guid.NewGuid();

            // Test set method
            _target.ReportId = setValue;

            // Test get method
            Guid getValue = _target.ReportId;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyReportName()
        {
            // Declare variable to hold property set method
            const string setValue = "Report";

            // Test set method
            _target.ReportName = setValue;

            // Test get method
            string getValue = _target.ReportName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyReportUrl()
        {
            // Declare variable to hold property set method
            const string setValue = "ReportUrl";

            // Test set method
            _target.ReportUrl = setValue;

            // Test get method
            string getValue = _target.ReportUrl;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyTargetFrame()
        {
            _target.TargetFrame = "_self";
            Assert.AreEqual("_self", _target.TargetFrame);
        }

        #endregion


    }

}
