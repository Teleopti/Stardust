using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecasterSkillTest
	{
		[Test]
		public void ShouldForecastAllWorkloadsOnSkill()
		{
			var skill = SkillFactory.CreateSkill("test");
			var wl1 = new Workload(skill);
			var wl2 = new Workload(skill);
			var historicalPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			var futurePeriod = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2);
			var quickForecasterWorkload = MockRepository.GenerateStub<IQuickForecasterWorkload>();
			
			var target = new QuickForecasterSkill(quickForecasterWorkload);
			target.Execute(skill, historicalPeriod, futurePeriod);

			quickForecasterWorkload.AssertWasCalled(x => x.Execute(wl1, historicalPeriod, futurePeriod));
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(wl2, historicalPeriod, futurePeriod));
		}
	}
}