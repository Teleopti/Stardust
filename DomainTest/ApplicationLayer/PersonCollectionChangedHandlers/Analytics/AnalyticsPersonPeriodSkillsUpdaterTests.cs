using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsPersonPeriodSkillsUpdaterTests
	{
		[SetUp]
		public void Setup()
		{
			_analyticsSkillRepository = new FakeAnalyticsSkillRepository();

			_analyticsSkillRepository.AddAgentSkill(1, 1, true, 1);
			_analyticsSkillRepository.AddAgentSkill(1, 2, true, 1);
			_analyticsSkillRepository.AddAgentSkill(1, 3, false, 1);
			_analyticsSkillRepository.AddAgentSkill(1, 4, true, 1);

			_analyticsSkillRepository.AddAgentSkill(2, 1, true, 1);
			_analyticsSkillRepository.AddAgentSkill(2, 2, true, 1);


			_target = new AnalyticsPersonPeriodSkillsUpdater(_analyticsSkillRepository);
		}

		private AnalyticsPersonPeriodSkillsUpdater _target;
		private IAnalyticsSkillRepository _analyticsSkillRepository;


		[Test]
		public void ShouldReturnAgentSkills()
		{
			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => a.Active).Should().Be.EqualTo(3);
			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => !a.Active).Should().Be.EqualTo(1);

			_analyticsSkillRepository.GetFactAgentSkillsForPerson(2).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveChangedAgentSkills()
		{
			// When handling event
			_target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new[] { 1, 2 },
				AnalyticsInactiveSkillsId = new[] { 3 },
				AnalyticsPersonPeriodId = 1,
				AnalyticsBusinessUnitId = 1
			});

			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count.Should().Be.EqualTo(3);
			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => a.Active).Should().Be.EqualTo(2);
			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count(a => !a.Active).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnAnySkills()
		{
			_analyticsSkillRepository.GetFactAgentSkillsForPerson(123).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnAnySkillsAfterDelete()
		{
			// When handling event
			_target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new List<int>(),
				AnalyticsInactiveSkillsId = new List<int>(),
				AnalyticsPersonPeriodId = 1,
				AnalyticsBusinessUnitId = 1
			});

			_analyticsSkillRepository.GetFactAgentSkillsForPerson(1).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNewSkills()
		{
			_target.Handle(new AnalyticsPersonPeriodSkillsChangedEvent
			{
				AnalyticsActiveSkillsId = new[] { 1, 2 },
				AnalyticsInactiveSkillsId = new[] { 3 },
				AnalyticsPersonPeriodId = 3,
				AnalyticsBusinessUnitId = 1
			});

			_analyticsSkillRepository.GetFactAgentSkillsForPerson(3).Count.Should().Be.EqualTo(3);
		}
	}
}