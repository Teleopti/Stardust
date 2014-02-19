using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ResourceCalculationPrerequisitesLoader : IResourceCalculationPrerequisitesLoader
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWork;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IAbsenceRepository _absenceRepository;

		public ResourceCalculationPrerequisitesLoader(ICurrentUnitOfWorkFactory currentUnitOfWork, IContractScheduleRepository contractScheduleRepository, IActivityRepository activityRepository, IAbsenceRepository absenceRepository)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_contractScheduleRepository = contractScheduleRepository;
			_activityRepository = activityRepository;
			_absenceRepository = absenceRepository;
		}

		public void Execute()
		{
			using (_currentUnitOfWork.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork().DisableFilter(QueryFilter.Deleted))
			{
				_contractScheduleRepository.LoadAllAggregate();
				_activityRepository.LoadAll();
				_absenceRepository.LoadAll();
			}
		}
	}
}