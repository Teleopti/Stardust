using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.WebTest.TestHelper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class StaffingMetricsControllerTest
	{
		[Test]
		public void ShouldGetSkills()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var target = new StaffingMetricsController(skillRepository, null);
			var expected = SkillFactory.CreateSkill("skill1");
			expected.SetId(Guid.NewGuid());
			var date = DateOnly.Today;
			skillRepository.Stub(x => x.FindAllWithSkillDays(new DateOnlyPeriod(date, date))).Return(new[] {expected});
			var result = target.AvailableSkills(date.Date).Result<AvailableSkillsModel>();
			var skill = result.Skills.First();
			skill.Id.Should().Be.EqualTo(expected.Id);
			skill.Name.Should().Be.EqualTo(expected.Name);
		}

		[Test]
		public void ShouldGetStaffingMetric()
		{
			var dailyStaffingMetricsViewModelFactory = MockRepository.GenerateMock<IDailyStaffingMetricsViewModelFactory>();
			var skillId = Guid.NewGuid();
			var dateTime = DateTime.Today;
			var dailyStaffingMetricsViewModel = new DailyStaffingMetricsViewModel();
			dailyStaffingMetricsViewModelFactory.Stub(x => x.CreateViewModel(skillId, dateTime)).Return(dailyStaffingMetricsViewModel);
			var target = new StaffingMetricsController(null, dailyStaffingMetricsViewModelFactory);
			var vm = target.DailyStaffingMetrics(skillId, dateTime).Result<DailyStaffingMetricsViewModel>();

			vm.Should().Be.SameInstanceAs(dailyStaffingMetricsViewModel);
		}
	}
}