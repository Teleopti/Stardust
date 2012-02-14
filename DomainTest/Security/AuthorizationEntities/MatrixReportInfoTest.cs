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
            _target = new MatrixReportInfo(1, "Report");
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
            IList<MatrixReportInfo> list = new List<MatrixReportInfo>();
            list.Add(new MatrixReportInfo(1, "Report1"));
            list.Add(new MatrixReportInfo(2, "Report2"));
            list.Add(new MatrixReportInfo(3, "Report3"));

            MatrixReportInfo foundItem;

            foundItem = MatrixReportInfo.FindByReportId(list, 1);
            Assert.IsNotNull(foundItem);
            Assert.AreEqual("Report1", foundItem.ReportName);

            foundItem = MatrixReportInfo.FindByReportId(list, -1);
            Assert.IsNull(foundItem);
        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifyReportId()
        {
            // Declare variable to hold property set method
            System.Int32 setValue = 2;

            // Test set method
            _target.ReportId = setValue;

            // Declare return variable to hold property get method
            System.Int32 getValue = 0;

            // Test get method
            getValue = _target.ReportId;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyReportName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Report";

            // Test set method
            _target.ReportName = setValue;

            // Declare return variable to hold property get method
            System.String getValue;

            // Test get method
            getValue = _target.ReportName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyReportUrl()
        {
            // Declare variable to hold property set method
            System.String setValue = "ReportUrl";

            // Test set method
            _target.ReportUrl = setValue;

            // Declare return variable to hold property get method
            System.String getValue;

            // Test get method
            getValue = _target.ReportUrl;

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
