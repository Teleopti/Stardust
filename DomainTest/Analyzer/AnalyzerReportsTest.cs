using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analyzer;

namespace Teleopti.Ccc.DomainTest.Analyzer
{
    /// <summary>
    /// Test class for AnalyzerReports
    /// </summary>
    [TestFixture]
    public class AnalyzerReportsTest
    {
        private AnalyzerReport target;
        [SetUp]
        public void Setup()
        {
            target = new AnalyzerReport();
            target.Depth = 1;
            target.FolderName = "Folder1";
            target.ReportId = 20;
            target.ReportName = "Report1";
        }

        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        [Test]
        public void CanCreateAnalyzerReport()
        {
            Assert.IsNotNull(target);
            Assert.AreEqual(20, target.ReportId);
        }

        /// <summary>
        /// Determines whether can read properties.
        /// </summary>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-15    
        /// /// </remarks>
        [Test]
        public void CanReadProperties()
        {

            Assert.AreEqual(1, target.Depth);
            Assert.AreEqual(20, target.ReportId);
            Assert.AreEqual("Folder1", target.FolderName);
            Assert.AreEqual("Report1", target.ReportName);

        }

    }
}
