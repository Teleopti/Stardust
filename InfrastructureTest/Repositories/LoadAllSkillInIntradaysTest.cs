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

			var target = new LoadAllSkillInIntradays(CurrUnitOfWork);
			target.Skills().Count().Should().Be.EqualTo(1);
			target.Skills().First().Name.Should().Be.EqualTo(skillWithQueue.Name);
		}

	}
}