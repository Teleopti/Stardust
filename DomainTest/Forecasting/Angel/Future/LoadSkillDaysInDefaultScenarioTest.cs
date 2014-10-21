using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Future
{
	public class LoadSkillDaysInDefaultScenarioTest
	{
		[Test]
		public void ShouldReturnSkillDay()
		{
			var expected = new List<ISkillDay>();

			var period = new DateOnlyPeriod();
			var skill = SkillFactory.CreateSkill("_");
			var skillDayRepository = MockRepository.GenerateStub<ISkillDayRepository>();
			var fakeCurrentScenario = new FakeCurrentScenario();
			var target = new LoadSkillDaysInDefaultScenario(skillDayRepository, fakeCurrentScenario);
			skillDayRepository.Stub(x => x.FindRange(period, skill, fakeCurrentScenario.Current())).Return(expected);

			var result = target.FindRange(period, skill);

			result.Should().Be.SameInstanceAs(expected);
		}
	}
}