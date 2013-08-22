using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    
    [TestFixture]
    public class DistributionInformationExtractorTest
    {
        private MockRepository _mock;
        private List<IScheduleDay> _scheduleDays;
        private IDistributionInformationExtractor _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDays = new List<IScheduleDay>();
            _target = new DistributionInformationExtractor(_scheduleDays);
        }

        [Test]
        public void TestThis()
        {
            
        }
    }

    
}
