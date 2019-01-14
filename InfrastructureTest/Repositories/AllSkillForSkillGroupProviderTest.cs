using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class AllSkillForSkillGroupProviderTest
	{
		public IQueueSourceRepository QueueSourceRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public ISkillRepository SkillRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IAllSkillForSkillGroupProvider Target;
		public IWorkloadRepository WorkloadRepository;
		
		[Test]
		public void ShouldLoadAllSkills()
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skillWithQueue = SkillFactory.CreateSkill("dummy1", skillType, 15);
			var skillWithoutQueue = SkillFactory.CreateSkill("dummy2", skillType, 15);
			var activity = new Activity("dummyActivity");
			skillWithQueue.Activity = activity;
			skillWithoutQueue.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				QueueSourceRepository.Add(queueSourceHelpdesk);
				SkillTypeRepository.Add(skillType);
				ActivityRepository.Add(activity);
				SkillRepository.Add(skillWithQueue);
				SkillRepository.Add(skillWithoutQueue);
				uow.PersistAll();
			}
			
			var workload = WorkloadFactory.CreateWorkload(skillWithQueue);
			workload.AddQueueSource(queueSourceHelpdesk);
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				WorkloadRepository.Add(workload);
				uow.PersistAll();
			}
			
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var skills = Target.AllExceptSubSkills().ToList();
				skills.Count().Should().Be.EqualTo(2);
				skills.First().Name.Should().Be.EqualTo(skillWithQueue.Name);
				skills.Second().Name.Should().Be.EqualTo(skillWithoutQueue.Name);
			}
			
		}

		[Test]
		public void ShouldNotShowSubSkills()
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skillWithQueue = SkillFactory.CreateSkill("dummy1", skillType, 15);
			var skillMultiSite = SkillFactory.CreateMultisiteSkill("multisite", skillType, 15);

			var activity = new Activity("dummyActivity2");
			skillWithQueue.Activity = activity;
			skillMultiSite.Activity = activity;
			var queueSourceHelpdesk = QueueSourceFactory.CreateQueueSourceHelpdesk();
			
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				QueueSourceRepository.Add(queueSourceHelpdesk);
				SkillTypeRepository.Add(skillType);
				ActivityRepository.Add(activity);
				SkillRepository.Add(skillWithQueue);
				SkillRepository.Add(skillMultiSite);
				uow.PersistAll();
			}

			IChildSkill childSkill = SkillFactory.CreateChildSkill("childskill", skillMultiSite);
			skillMultiSite.AddChildSkill(childSkill);
			childSkill.Activity = activity;

			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				SkillRepository.Add(childSkill);
				uow.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var skills = Target.AllExceptSubSkills().ToList();
				skills.Count().Should().Be.EqualTo(2);
				skills.First().Name.Should().Be.EqualTo(skillWithQueue.Name);
				skills.Second().Name.Should().Be.EqualTo(skillMultiSite.Name);
			}

		}

	}
}