using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class RepositoryFactoryFake : IRepositoryFactory
	{
		public IPersonRepository PersonRepository { get; set; }
		public IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork)
		{
			return PersonRepository;
		}

		public IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonAssignmentRepository CreatePersonAssignmentRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IPersonAbsenceRepository CreatePersonAbsenceRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public IContractRepository CreateContractRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public IDayOffTemplateRepository CreateDayOffRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IMeetingRepository CreateMeetingRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public IUserDetailRepository CreateUserDetailRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
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

		public IAgentBadgeSettingsRepository CreateAgentBadgeSettingsRepository(IUnitOfWork unitOfWork)
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