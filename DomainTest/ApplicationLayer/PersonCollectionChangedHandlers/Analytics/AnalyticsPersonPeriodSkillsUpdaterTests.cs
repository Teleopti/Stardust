using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsPersonPeriodSkillsUpdaterTests : ISetup
	{
		public AnalyticsPersonPeriodSkillsUpdater Target;
		public IAnalyticsSkillRepository AnalyticsSkillRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsPersonPeriodSkillsUpdater>();
		}

		private void createSkills()
		{
			AnalyticsSkillRepository.AddAgentSkill(1, 1, true, 1);
			AnalyticsSkillRepository.AddAgentSkill(1, 2, true, 1);
			AnalyticsSkillRepository.AddAgentSkill(1, 3, false, 1);
			AnalyticsSkillRepository.AddAgentSkill(1, 4, true, 1);

			AnalyticsSkillRepository.AddAgentSkill(2, 1, true, 1);
			AnalyticsSkillRepository.AddAgentSkill(2, 2, true, 1);
		}

		[Test]
		public void ShouldReturnAgentSkills()
		{
			createSkills();

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => a.Active).Should().Be.EqualTo(3);
			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => !a.Active).Should().Be.EqualTo(1);

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(2).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveChangedAgentSkills()
		{
			createSkills();

			// When handling event
			Target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new[] { 1, 2 },
				AnalyticsInactiveSkillsId = new[] { 3 },
				AnalyticsPersonPeriodId = 1,
				AnalyticsBusinessUnitId = 1
			});

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count.Should().Be.EqualTo(3);
			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => a.Active).Should().Be.EqualTo(2);
			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => !a.Active).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnAnySkills()
		{
			createSkills();

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(123).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnAnySkillsAfterDelete()
		{
			createSkills();

			// When handling event
			Target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new List<int>(),
				AnalyticsInactiveSkillsId = new List<int>(),
				AnalyticsPersonPeriodId = 1,
				AnalyticsBusinessUnitId = 1
			});

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNewSkills()
		{
			createSkills();

			Target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new[] { 1, 2 },
				AnalyticsInactiveSkillsId = new[] { 3 },
				AnalyticsPersonPeriodId = 3,
				AnalyticsBusinessUnitId = 1
			});

			AnalyticsSkillRepository.GetFactAgentSkillsForPerson(3).Count.Should().Be.EqualTo(3);
		}
	}
}