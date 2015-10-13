using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRepositoryFactory : IRepositoryFactory
	{
		private readonly IActivityRepository _activityRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IScheduleTagRepository _scheduleTagRepository;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;

		public FakeRepositoryFactory(
			IActivityRepository activityRepository,
			IAbsenceRepository absenceRepository,
			IDayOffTemplateRepository dayOffTemplateRepository,
			IShiftCategoryRepository shiftCategoryRepository,
			IContractRepository contractRepository,
			IContractScheduleRepository contractScheduleRepository,
			IScheduleTagRepository scheduleTagRepository,
			IWorkflowControlSetRepository workflowControlSetRepository,
			IPersonAbsenceRepository personAbsenceRepository
			)
		{
			_activityRepository = activityRepository;
			_absenceRepository = absenceRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_shiftCategoryRepository = shiftCategoryRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_scheduleTagRepository = scheduleTagRepository;
			_workflowControlSetRepository = workflowControlSetRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		public IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork)
		{
			return _absenceRepository;
		}

		public IPersonAssignmentRepository CreatePersonAssignmentRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonAbsenceRepository CreatePersonAbsenceRepository(IUnitOfWork unitOfWork)
		{
			return _personAbsenceRepository;
		}

		public IPersonAbsenceAccountRepository CreatePersonAbsenceAccountRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IScenarioRepository CreateScenarioRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IApplicationFunctionRepository CreateApplicationFunctionRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IApplicationRoleRepository CreateApplicationRoleRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IAvailableDataRepository CreateAvailableDataRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IStatisticRepository CreateStatisticRepository()
		{
			throw new System.NotImplementedException();
		}

		public IAgentStateReadModelReader CreateRtaRepository()
		{
			throw new System.NotImplementedException();
		}

		public IBusinessUnitRepository CreateBusinessUnitRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISkillDayRepository CreateSkillDayRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IValidatedVolumeDayRepository CreateValidatedVolumeDayRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IScheduleRepository CreateScheduleRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IMultisiteDayRepository CreateMultisiteDayRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISkillRepository CreateSkillRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IWorkloadRepository CreateWorkloadRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISkillTypeRepository CreateSkillTypeRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IContractScheduleRepository CreateContractScheduleRepository(IUnitOfWork unitOfWork)
		{
			return _contractScheduleRepository;
		}

		public IContractRepository CreateContractRepository(IUnitOfWork unitOfWork)
		{
			return _contractRepository;
		}

		public IPersonRotationRepository CreatePersonRotationRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonAvailabilityRepository CreatePersonAvailabilityRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonRequestRepository CreatePersonRequestRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IShiftCategoryRepository CreateShiftCategoryRepository(IUnitOfWork unitOfWork)
		{
			return _shiftCategoryRepository;
		}

		public IDayOffTemplateRepository CreateDayOffRepository(IUnitOfWork unitOfWork)
		{
			return _dayOffTemplateRepository;
		}

		public IMeetingRepository CreateMeetingRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork)
		{
			return _activityRepository;
		}

		public IMultiplicatorDefinitionSetRepository CreateMultiplicatorDefinitionSetRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISiteRepository CreateSiteRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ITeamRepository CreateTeamRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPayrollExportRepository CreatePayrollExportRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IMultiplicatorRepository CreateMultiplicatorRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPushMessageRepository CreatePushMessageRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPushMessageDialogueRepository CreatePushMessageDialogueRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISettingDataRepository CreateGlobalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPreferenceDayRepository CreatePreferenceDayRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IStudentAvailabilityDayRepository CreateStudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IWorkShiftRuleSetRepository CreateWorkShiftRuleSetRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IRuleSetBagRepository CreateRuleSetBagRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IGroupPageRepository CreateGroupPageRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPartTimePercentageRepository CreatePartTimePercentageRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IOptionalColumnRepository CreateOptionalColumnRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IWorkflowControlSetRepository CreateWorkflowControlSetRepository(IUnitOfWork unitOfWork)
		{
			return _workflowControlSetRepository;
		}

		public IExtendedPreferenceTemplateRepository CreateExtendedPreferenceTemplateRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IRepository<IBudgetGroup> CreateBudgetGroupRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IMasterActivityRepository CreateMasterActivityRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public INoteRepository CreateNoteRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPublicNoteRepository CreatePublicNoteRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IAgentDayScheduleTagRepository CreateAgentDayScheduleTagRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IScheduleTagRepository CreateScheduleTagRepository(IUnitOfWork unitOfWork)
		{
			return _scheduleTagRepository;
		}

		public IApplicationRolePersonRepository CreateApplicationRolePersonRepository(IStatelessUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ILicenseStatusRepository CreateLicenseStatusRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ILicenseRepository CreateLicenseRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IRequestHistoryReadOnlyRepository CreateRequestHistoryReadOnlyRepository(IStatelessUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonScheduleDayReadModelFinder CreatePersonScheduleDayReadModelFinder(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IOvertimeAvailabilityRepository CreateOvertimeAvailabilityRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IGamificationSettingRepository CreateGamificationSettingRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ITeamGamificationSettingRepository CreateTeamGamificationSettingRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}
	}
}