using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class SkillAreaRepositoryTest : RepositoryTest<SkillArea>
	{
		private SkillType _skillType;
		private Activity _activity;
		private ISkill skill;

		protected override void ConcreteSetup()
		{
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();
			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			_skillType = SkillTypeFactory.CreateSkillType();
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

		protected override SkillArea CreateAggregateWithCorrectBusinessUnit()
		{
			var skills = new LoadSkillInIntradays(CurrUnitOfWork, new SupportedSkillsInIntradayProvider(null)).Skills();

			return new SkillArea
			{
				Name = "skill area 1 name",
				Skills = new[]
				{
					skills.First()
				}
			};
		}

		protected override void VerifyAggregateGraphProperties(SkillArea loadedAggregateFromDatabase)
		{
			SkillArea skillArea = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(skillArea.Name, loadedAggregateFromDatabase.Name);
			Assert.AreEqual(skillArea.Skills.Count, loadedAggregateFromDatabase.Skills.Count);
			Assert.AreEqual(skillArea.Skills.First().Name, loadedAggregateFromDatabase.Skills.First().Name);
		}

		protected override Repository<SkillArea> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SkillAreaRepository(currentUnitOfWork);
		}
	}
}