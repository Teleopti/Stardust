using Autofac;
using Autofac.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class VirtualSkillGroupsKeepStateTest
	{
		public VirtualSkillGroupsResultProvider Target;
		public ILifetimeScope LifetimeScope;

		[Test]
		public void ShouldKeepResultInternally()
		{
			var date = new DateOnly(2000, 1, 1);
			Target.Fetch(date)
				.Should().Be.SameInstanceAs(Target.Fetch(date));
		}

		[Test]
		public void ShouldNotKeepResultBetweenDifferentLifetimeScopes()
		{
			var date = new DateOnly(2000, 1, 1);
			VirtualSkillGroupsCreatorResult result1;
			VirtualSkillGroupsCreatorResult result2;

			using (var newScope = LifetimeScope.BeginLifetimeScope())
			{
				var innerTarget = newScope.Resolve<VirtualSkillGroupsResultProvider>();
				result1 = innerTarget.Fetch(date);
			}
			using (var newScope = LifetimeScope.BeginLifetimeScope())
			{
				var innerTarget = newScope.Resolve<VirtualSkillGroupsResultProvider>();
				result2 = innerTarget.Fetch(date);
			}

			result1.Should().Not.Be.SameInstanceAs(result2);
		}
	}
}