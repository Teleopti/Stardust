using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
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

			PersistAndRemoveFromUnitOfWork(skillType);
			var workload = WorkloadFactory.CreateWorkload(skillWithQueue);
			workload.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workload);

			var target = new LoadSkillInIntradays(CurrUnitOfWork);
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(1);
			skills.First().Name.Should().Be.EqualTo(skillWithQueue.Name);
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
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOnlyLoadInboundPhoneSkill()
		{
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			ISkillTypeEmail skillTypeEmail = new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email);
			var phoneSkill = SkillFactory.CreateSkill("Phone", skillTypePhone, 15);
			var emailSkill = SkillFactory.CreateSkill("Email", skillTypeEmail, 60);
			var activity = new Activity("dummyActivity");
			phoneSkill.Activity = activity;
			emailSkill.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			PersistAndRemoveFromUnitOfWork(skillTypePhone);
			PersistAndRemoveFromUnitOfWork(skillTypeEmail);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(phoneSkill);
			PersistAndRemoveFromUnitOfWork(emailSkill);

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

 			var workloadPhone = WorkloadFactory.CreateWorkload(phoneSkill);
 			var workloadEmail = WorkloadFactory.CreateWorkload(emailSkill);
			workloadPhone.AddQueueSource(queueSourceHelpdesk);
			workloadEmail.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workloadPhone);
			PersistAndRemoveFromUnitOfWork(workloadEmail);

			var target = new LoadSkillInIntradays(CurrUnitOfWork);
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(2);
			skills.First().Name.Should().Be.EqualTo(emailSkill.Name);
			skills.First().DoDisplayData.Should().Be.EqualTo(false);
			skills.Last().DoDisplayData.Should().Be.EqualTo(true);
		}

	}
}