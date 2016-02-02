using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class LoadAllSkillInIntradaysTest : DatabaseTest
	{
		[Test]
		public void ShouldFindExistingSkillInIntraday()
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(skillType);
			var activity = new Activity("The test") { DisplayColor = Color.Honeydew };
			PersistAndRemoveFromUnitOfWork(activity);
			var skill = SkillFactory.CreateSkill("Skill - Name", skillType, 15);
			skill.Activity = activity;
			PersistAndRemoveFromUnitOfWork(skill);

			var target = new LoadAllSkillInIntradays(CurrUnitOfWork);
			target.Skills().Count().Should().Be.EqualTo(1);
			target.Skills().First().Name.Should().Be.EqualTo(skill.Name);
		}

	}
}