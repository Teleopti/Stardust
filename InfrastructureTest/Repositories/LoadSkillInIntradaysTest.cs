using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class LoadSkillInIntradaysTest : DatabaseTest
	{
		[Test]
		public void ShouldLoadExistingIntradaySkillWithAtLeastOneQueue()
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
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

			var target = new LoadSkillInIntradays(CurrUnitOfWork, new SupportedSkillsInIntradayProvider(null, 
				new List<ISupportedSkillCheck>() { },
				new MultisiteSkillSupportedCheck()), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(1);
			skills.First().Name.Should().Be.EqualTo(skillWithQueue.Name);
		}

		[Test]
		public void ShouldNotLoadDuplicateSkills()
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
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

			var target = new LoadSkillInIntradays(CurrUnitOfWork, new SupportedSkillsInIntradayProvider(null, 
				new List<ISupportedSkillCheck>() { },
				new MultisiteSkillSupportedCheck()), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMarkSkillsForDisplay()
		{
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			ISkillTypeEmail skillTypeEmail = new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email);
			ISkillTypePhone skillTypeChat = new SkillTypePhone(new Description("SkillTypeChat"), ForecastSource.InboundTelephony);
			var phoneSkill = SkillFactory.CreateSkill("Phone", skillTypePhone, 15);
			var emailSkill = SkillFactory.CreateSkill("Email", skillTypeEmail, 60);
			var chatSkill = SkillFactory.CreateSkill("Chat", skillTypeChat, 60);
			var activity = new Activity("dummyActivity");
			phoneSkill.Activity = activity;
			emailSkill.Activity = activity;
			chatSkill.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			PersistAndRemoveFromUnitOfWork(skillTypePhone);
			PersistAndRemoveFromUnitOfWork(skillTypeEmail);
			PersistAndRemoveFromUnitOfWork(skillTypeChat);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(phoneSkill);
			PersistAndRemoveFromUnitOfWork(emailSkill);
			PersistAndRemoveFromUnitOfWork(chatSkill);

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

 			var workloadPhone = WorkloadFactory.CreateWorkload(phoneSkill);
 			var workloadEmail = WorkloadFactory.CreateWorkload(emailSkill);
 			var workloadChat = WorkloadFactory.CreateWorkload(chatSkill);
			workloadPhone.AddQueueSource(queueSourceHelpdesk);
			workloadEmail.AddQueueSource(queueSourceHelpdesk);
			workloadChat.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workloadPhone);
			PersistAndRemoveFromUnitOfWork(workloadEmail);
			PersistAndRemoveFromUnitOfWork(workloadChat);

			var target = new LoadSkillInIntradays(CurrUnitOfWork,
				new SupportedSkillsInIntradayProvider(null,
					new List<ISupportedSkillCheck>()
					{
						new InboundPhoneSkillSupported(),
						new OtherSkillsLikePhoneSupported()
					},
					new MultisiteSkillSupportedCheck()
				), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(3);
			skills.First().Name.Should().Be.EqualTo(chatSkill.Name);
			skills.First().DoDisplayData.Should().Be.EqualTo(true);
			skills.First().SkillType.Should().Be.EqualTo(skillTypeChat.Description.Name);
			skills[1].Name.Should().Be.EqualTo(emailSkill.Name);
			skills[1].DoDisplayData.Should().Be.EqualTo(false);
			skills[1].SkillType.Should().Be.EqualTo(skillTypeEmail.Description.Name);
			skills.Last().DoDisplayData.Should().Be.EqualTo(true);
			skills.Last().SkillType.Should().Be.EqualTo(skillTypePhone.Description.Name);
		}

		[Test]
		public void ShouldNotMarkMultisiteSkillForDisplay()
		{
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			var phoneSkill = SkillFactory.CreateSkill("Phone", skillTypePhone, 15);
			var multiSiteSkill = SkillFactory.CreateMultisiteSkill("Multisite", skillTypePhone, 15);
			var activity = new Activity("dummyActivity");
			phoneSkill.Activity = activity;
			multiSiteSkill.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			PersistAndRemoveFromUnitOfWork(skillTypePhone);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(phoneSkill);
			PersistAndRemoveFromUnitOfWork(multiSiteSkill);

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			var workloadPhone = WorkloadFactory.CreateWorkload(phoneSkill);
			var workloadEmail = WorkloadFactory.CreateWorkload(multiSiteSkill);
			workloadPhone.AddQueueSource(queueSourceHelpdesk);
			workloadEmail.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workloadPhone);
			PersistAndRemoveFromUnitOfWork(workloadEmail);

			var target = new LoadSkillInIntradays(CurrUnitOfWork,
				new SupportedSkillsInIntradayProvider(null,
					new List<ISupportedSkillCheck>()
					{
						new InboundPhoneSkillSupported()
					},
					new MultisiteSkillSupportedCheck()
				), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));

			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(2);
			skills.First().Name.Should().Be.EqualTo(multiSiteSkill.Name);
			skills.First().DoDisplayData.Should().Be.EqualTo(false);
			skills.Last().DoDisplayData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMarkMultisiteSkillForDisplay()
		{
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			var multiSiteSkill = SkillFactory.CreateMultisiteSkill("Multisite", skillTypePhone, 15);
			var activity = new Activity("dummyActivity");
			multiSiteSkill.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(skillTypePhone);
			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(multiSiteSkill);
			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			var workloadEmail = WorkloadFactory.CreateWorkload(multiSiteSkill);
			workloadEmail.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workloadEmail);

			var target = new LoadSkillInIntradays(CurrUnitOfWork,
				new SupportedSkillsInIntradayProvider(null,
					new List<ISupportedSkillCheck>()
					{
						new InboundPhoneSkillSupported()
					},
					new MultisiteSkillSupportedCheckAlwaysTrue()
				), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));
			var skills = target.Skills().ToList();
			skills.Count().Should().Be.EqualTo(1);
			skills.First().Name.Should().Be.EqualTo(multiSiteSkill.Name);
			skills.First().DoDisplayData.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMarkMultisiteSkillAsIsMulisiteSkill()
		{
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			var phoneSkill = SkillFactory.CreateSkill("Phone", skillTypePhone, 15);
			var multiSiteSkill = SkillFactory.CreateMultisiteSkill("Multisite", skillTypePhone, 15);
			var activity = new Activity("dummyActivity");
			phoneSkill.Activity = activity;
			multiSiteSkill.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			PersistAndRemoveFromUnitOfWork(skillTypePhone);

			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(phoneSkill);
			PersistAndRemoveFromUnitOfWork(multiSiteSkill);

			PersistAndRemoveFromUnitOfWork(queueSourceHelpdesk);

			var workloadPhone = WorkloadFactory.CreateWorkload(phoneSkill);
			var workloadEmail = WorkloadFactory.CreateWorkload(multiSiteSkill);
			workloadPhone.AddQueueSource(queueSourceHelpdesk);
			workloadEmail.AddQueueSource(queueSourceHelpdesk);
			PersistAndRemoveFromUnitOfWork(workloadPhone);
			PersistAndRemoveFromUnitOfWork(workloadEmail);

			var target = new LoadSkillInIntradays(CurrUnitOfWork,
				new SupportedSkillsInIntradayProvider(null,
					new List<ISupportedSkillCheck>()
					{
						new InboundPhoneSkillSupported()
					},
					new MultisiteSkillSupportedCheckAlwaysTrue()
				), new SkillTypeInfoProvider(new List<ISkillTypeInfo>()));
			var skills = target.Skills().ToList();
			
			skills.First().IsMultisiteSkill.Should().Be.True();
			skills.Last().IsMultisiteSkill.Should().Be.False();
		}

	}
}