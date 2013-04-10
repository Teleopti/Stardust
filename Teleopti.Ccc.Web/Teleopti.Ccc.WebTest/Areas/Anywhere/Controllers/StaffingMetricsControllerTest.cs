using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class StaffingMetricsControllerTest
	{
		[Test]
		public void ShouldGetSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var target = new StaffingMetricsController(skillRepository);
			var expected = SkillFactory.CreateSkill("skill1");
			expected.SetId(Guid.NewGuid());
			var date = DateOnly.Today;
			skillRepository.Stub(x => x.FindAllWithSkillDays(new DateOnlyPeriod(date, date))).Return(new[] {expected});
			dynamic result = target.AvailableSkills(date).Data;
			dynamic skill = result.Skills[0];
			((object)skill.Id).Should().Be.EqualTo(expected.Id);
			((object)skill.Name).Should().Be.EqualTo(expected.Name);
		}
	}
}