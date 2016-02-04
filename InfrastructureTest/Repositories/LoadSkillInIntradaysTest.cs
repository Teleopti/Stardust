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
	public class LoadSkillInIntradaysTest : DatabaseTest
	{
		[Test]
		public void ShouldLoadExistingIntradaySkillWithAtLeastOneQueue()
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillWithQueue = SkillFactory.CreateSkill("dummy", skillType, 15);
			var skillWithoutQueue = SkillFactory.CreateSkill("dummy", skillType, 15);
			var activity = new Activity("dummyActivity");
			skillWithQueue.Activity = activity;
			skillWithoutQueue.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			PersistAndRemoveFromUnitOfWork(skillType);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(skillWithQueue);
			PersistAndRemoveFromUnitOfWork(skillWithoutQueue);

			var workload = WorkloadFactory.CreateWorkload(skillWithQueue);
			workload.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workload);

			var target = new LoadSkillInIntradays(CurrUnitOfWork);
			target.Skills().Count().Should().Be.EqualTo(1);
			target.Skills().First().Name.Should().Be.EqualTo(skillWithQueue.Name);
		}

		[Test]
		public void ShouldNotLoadDuplicateSkills()
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillWithQueues = SkillFactory.CreateSkill("dummy", skillType, 15);
			var activity = new Activity("dummyActivity");
			skillWithQueues.Activity = activity;
			var queue1 = QueueSourceFactory.CreateQueueSourceHelpdesk();
			var queue2 = QueueSourceFactory.CreateQueueSourceInrikes();

			PersistAndRemoveFromUnitOfWork(queue1);
			PersistAndRemoveFromUnitOfWork(queue2);

			PersistAndRemoveFromUnitOfWork(skillType);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(skillWithQueues);

			var workload = WorkloadFactory.CreateWorkload(skillWithQueues);
			workload.AddQueueSource(queue1);
			workload.AddQueueSource(queue2);
			PersistAndRemoveFromUnitOfWork(workload);

			var target = new LoadSkillInIntradays(CurrUnitOfWork);
			target.Skills().Count().Should().Be.EqualTo(1);
		}

	}
}