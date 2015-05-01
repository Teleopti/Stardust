using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Outbound
{
	public class OutboundSkillPersister
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;

		public OutboundSkillPersister(ISkillRepository skillRepository, IWorkloadRepository workloadRepository)
		{
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
		}

		public void PersistAll(ISkill skill)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_skillRepository.Add(skill);
				_workloadRepository.Add(skill.WorkloadCollection.First());
				uow.PersistAll();
			}
		}
	}
}