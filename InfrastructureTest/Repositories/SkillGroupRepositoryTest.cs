using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class SkillGroupRepositoryTest : RepositoryTest<SkillGroup>
	{
		private SkillType _skillType;
		private Activity _activity;
		private ISkill skill;

		protected override void ConcreteSetup()
		{
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();
			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			_skillType = SkillTypeFactory.CreateSkillTypePhone();
			PersistAndRemoveFromUnitOfWork(_skillType);
			_activity = new Activity("The test") { DisplayColor = Color.Honeydew };
			PersistAndRemoveFromUnitOfWork(_activity);
			skill = SkillFactory.CreateSkill("Skill - Name", _skillType, 15);
			skill.Activity = _activity;
			PersistAndRemoveFromUnitOfWork(skill);

			var workload = WorkloadFactory.CreateWorkload(skill);
			workload.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workload);
		}

		protected override SkillGroup CreateAggregateWithCorrectBusinessUnit()
		{
			var skills = new LoadSkillInIntradays(CurrUnitOfWork, new SupportedSkillsInIntradayProvider(null,
					new List<ISupportedSkillCheck>() { },
					new MultisiteSkillSupportedCheckAlwaysTrue()), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()))
				.Skills();

			return new SkillGroup
			{
				Name = "skill group 1 name",
				Skills = new[]
				{
					skills.First()
				}
			};
		}

		protected override void VerifyAggregateGraphProperties(SkillGroup loadedAggregateFromDatabase)
		{
			SkillGroup skillGroup = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(skillGroup.Name, loadedAggregateFromDatabase.Name);
			Assert.AreEqual(skillGroup.Skills.Count, loadedAggregateFromDatabase.Skills.Count);
			Assert.AreEqual(skillGroup.Skills.First().Name, loadedAggregateFromDatabase.Skills.First().Name);
		}

		protected override Repository<SkillGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SkillGroupRepository(currentUnitOfWork);
		}
	}
}