using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SkillStaffPeriodStatisticsForSkillIntradayTest
    {
        [Test]
        public void VerifyConstructor()
        {
			var target = new SkillStaffPeriodStatisticsForSkillIntraday(Enumerable.Empty<ISkillStaffPeriod>());
            Assert.IsNotNull(target);
        }
		
		
        [Test]
        public void VerifyStatisticsCalculator()
		{
			var target = new SkillStaffPeriodStatisticsForSkillIntraday(Enumerable.Empty<ISkillStaffPeriod>());
			var getValue = target.StatisticsCalculator;

			Assert.IsNotNull(getValue);
		}
    }
}
