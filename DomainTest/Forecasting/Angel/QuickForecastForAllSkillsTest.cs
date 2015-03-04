using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecastForAllSkillsTest
	{
		[Test]
		public void ShouldSumDifferenceForAllSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1, skill2});
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);

			quickForecaster.Stub(x => x.Execute(skill1, futurePeriod, historicalPeriod)).Return(2.3);
			quickForecaster.Stub(x => x.Execute(skill2, futurePeriod, historicalPeriod)).Return(3.4);
			
			var target = new QuickForecastForAllSkills(quickForecaster, skillRepository, now);
			var result = target.CreateForecast(futurePeriod);
			result.ToString().Should().Be.EqualTo((5.7/2).ToString());
		}
	}
}