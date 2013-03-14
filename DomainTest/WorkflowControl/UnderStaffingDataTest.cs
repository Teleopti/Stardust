using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class UnderStaffingDataTest
    {
        private IUnderStaffingData target;

        [SetUp]
        public void Setup()
        {
            target = new UnderStaffingData
                {
                    UnderStaffingDates =
                        new Dictionary<string, IList<string>> {{"UnderStaffingDates", new List<string>() {"2012-01-08"}}},
                    UnderStaffingHours =
                        new Dictionary<string, IList<string>> {{"UnderStaffingHours", new List<string>() {"10:00-10:15"}}}
                };
        }

        [Test]
        public void ShouldInitializePropertiesProperly()
        {
            Assert.IsNotNull(target.UnderStaffingDates["UnderStaffingDates"]);
            Assert.IsNotNull(target.UnderStaffingHours["UnderStaffingHours"]);
        }
    }
}
