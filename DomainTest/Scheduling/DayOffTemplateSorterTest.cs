using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class DayOffTemplateSorterTest
    {
        private List<IDayOffTemplate> _dayOffs;

        [SetUp]
        public void Setup()
        {
            _dayOffs = new List<IDayOffTemplate>();
        }

        [Test]
        public void ShouldSort()
        {
            IDayOffTemplate dayOffA = new DayOffTemplate(new Description("A"));
            IDayOffTemplate dayOffB = new DayOffTemplate(new Description("B"));
            IDayOffTemplate dayOffC = new DayOffTemplate(new Description("C"));

            _dayOffs.Add(dayOffC);
            _dayOffs.Add(dayOffB);
            _dayOffs.Add(dayOffA);

            _dayOffs.Sort(new DayOffTemplateSorter());

            Assert.AreSame(dayOffA, _dayOffs[0]);
            Assert.AreSame(dayOffB, _dayOffs[1]);
            Assert.AreSame(dayOffC, _dayOffs[2]);
        }
    }
}
