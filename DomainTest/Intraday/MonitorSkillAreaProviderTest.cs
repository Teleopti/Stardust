using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class MonitorSkillAreaProviderTest
	{
		public MonitorSkillAreaProvider Target;
		public FakeSkillAreaRepository SkillAreaRepository;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;

		[Test]
		public void ShouldProvideMonitorData()
		{
			var skillAreaId = Guid.NewGuid();
			var today = DateOnly.Today;
			var skills = new Collection<SkillInIntraday>
			{
				new SkillInIntraday
				{
					Id = Guid.NewGuid()
				}
			};
			var existingSkillArea = new SkillArea
			{
				Skills = skills
			};
			existingSkillArea.SetId(skillAreaId);

			SkillAreaRepository.Has(existingSkillArea);
			IntradayMonitorDataLoader.Has(10, 25, new DateTime(today.Year, today.Month, today.Day, 8, 0, 0));

			var result = Target.Load(skillAreaId);

			result.ForecastedCalls.Should().Be.EqualTo(10);
			result.OfferedCalls.Should().Be.EqualTo(25);
			result.LatestStatsTime.Should().Be.EqualTo(new DateTime(today.Year, today.Month, today.Day, 8, 0, 0));
		}
	}
}