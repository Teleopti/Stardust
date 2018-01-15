using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
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
        public void VerifyAnalyze()
        {
			var period = MockRepository.GenerateMock<ISkillStaffPeriod>();
			period.Stub(x => x.FStaff).Return(1d);
            period.Stub(x => x.CalculatedResource).Return(11d);
            
            var target = new SkillStaffPeriodStatisticsForSkillIntraday(new List<ISkillStaffPeriod> { period });
            Assert.AreEqual(10d, target.StatisticsCalculator.AbsoluteDeviationSumma);
            Assert.AreEqual(10d, target.StatisticsCalculator.RelativeDeviationSumma);
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
