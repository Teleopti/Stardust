using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class DailyStaffingMetricsViewModelFactoryTest
	{
		[Test]
		public void ShouldGetForecastedHours()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);
			skillDayRepository.Stub(x => x.FindRange(
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), skill, scenario)).Return(new Collection<ISkillDay>
				{
					skillDay
				});
			skillDay.Stub(x => x.ForecastedIncomingDemand).Return(TimeSpan.FromHours(2.5));

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, skillDayRepository, currentScenario);
			var result = factory.CreateViewModel(skillId, dateTime);

			result.ForecastedHours.Should().Be.EqualTo(2.5);
		}
	}
}