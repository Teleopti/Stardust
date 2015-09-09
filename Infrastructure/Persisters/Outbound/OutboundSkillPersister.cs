using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Outbound
{
	public interface IOutboundSkillPersister
	{
		void PersistSkill(ISkill skill);
		void RemoveSkill(ISkill skill);
	}

	public class OutboundSkillPersister : IOutboundSkillPersister
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;

		public OutboundSkillPersister(ISkillRepository skillRepository, IWorkloadRepository workloadRepository)
		{
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
		}

		public void PersistAll(ISkill skill)  //only for win
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_skillRepository.Add(skill);
				_workloadRepository.Add(skill.WorkloadCollection.First());
				uow.PersistAll();
			}
		}

		public void PersistSkill(ISkill skill)
		{
			_skillRepository.Add(skill);
			_workloadRepository.Add(skill.WorkloadCollection.First());
		}

		public void RemoveSkill(ISkill skill)
		{
			_workloadRepository.Remove(skill.WorkloadCollection.First());
			_skillRepository.Remove(skill);
		}
	}
}