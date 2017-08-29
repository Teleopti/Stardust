using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRepositoryFactory : IRepositoryFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IScheduleTagRepository _scheduleTagRepository;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IMeetingRepository _meetingRepository;
		private readonly IAgentDayScheduleTagRepository _agentDayScheduleTagRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly IOvertimeAvailabilityRepository _overtimeAvailabilityRepository;
		private readonly IPersonAvailabilityRepository _personAvailabilityRepository;
		private readonly IPersonRotationRepository _personRotationRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly INoteRepository _noteRepository;
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly IGroupPageRepository _groupPageRepository;

		public FakeRepositoryFactory(
			IPersonRepository personRepository,
			IBusinessUnitRepository businessUnitRepository,
			IActivityRepository activityRepository,
			IAbsenceRepository absenceRepository,
			IDayOffTemplateRepository dayOffTemplateRepository,
			IShiftCategoryRepository shiftCategoryRepository,
			IContractRepository contractRepository,
			IContractScheduleRepository contractScheduleRepository,
			IScheduleTagRepository scheduleTagRepository,
			IWorkflowControlSetRepository workflowControlSetRepository,
			IPersonAbsenceRepository personAbsenceRepository,
			IPersonAssignmentRepository personAssignmentRepository,
			IMeetingRepository meetingRepository,
			IAgentDayScheduleTagRepository agentDayScheduleTagRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IStudentAvailabilityDayRepository studentAvailabilityDayRepository,
			IOvertimeAvailabilityRepository overtimeAvailabilityRepository,
			IPersonAvailabilityRepository personAvailabilityRepository,
			IPersonRotationRepository personRotationRepository,
			IPartTimePercentageRepository partTimePercentageRepository,
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
			IRuleSetBagRepository ruleSetBagRepository, 
			INoteRepository noteRepository, 
			IPublicNoteRepository publicNoteRepository,
			IGroupPageRepository groupPageRepository)
		{
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
			_activityRepository = activityRepository;
			_absenceRepository = absenceRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_shiftCategoryRepository = shiftCategoryRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_scheduleTagRepository = scheduleTagRepository;
			_workflowControlSetRepository = workflowControlSetRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_meetingRepository = meetingRepository;
			_agentDayScheduleTagRepository = agentDayScheduleTagRepository;
			_preferenceDayRepository = preferenceDayRepository;
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_overtimeAvailabilityRepository = overtimeAvailabilityRepository;
			_personAvailabilityRepository = personAvailabilityRepository;
			_personRotationRepository = personRotationRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_noteRepository = noteRepository;
			_publicNoteRepository = publicNoteRepository;
			_groupPageRepository = groupPageRepository;
		}

		public IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork)
		{
			return _personRepository;
		}

		public IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork)
		{
			return _absenceRepository;
		}

		public IPersonAssignmentRepository CreatePersonAssignmentRepository(IUnitOfWork unitOfWork)
		{
			return _personAssignmentRepository;
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

		public IBusinessUnitRepository CreateBusinessUnitRepository(IUnitOfWork unitOfWork)
		{
			return _businessUnitRepository;
		}

		public ISkillDayRepository CreateSkillDayRepository(IUnitOfWork unitOfWork)
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
			return _personRotationRepository;
		}

		public IPersonAvailabilityRepository CreatePersonAvailabilityRepository(IUnitOfWork unitOfWork)
		{
			return _personAvailabilityRepository;
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
			return _meetingRepository;
		}

		public IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork)
		{
			return _activityRepository;
		}

		public IMultiplicatorDefinitionSetRepository CreateMultiplicatorDefinitionSetRepository(IUnitOfWork unitOfWork)
		{
			return _multiplicatorDefinitionSetRepository;
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
			return _preferenceDayRepository;
		}

		public IStudentAvailabilityDayRepository CreateStudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
		{
			return _studentAvailabilityDayRepository;
		}

		public IWorkShiftRuleSetRepository CreateWorkShiftRuleSetRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public IRuleSetBagRepository CreateRuleSetBagRepository(IUnitOfWork unitOfWork)
		{
			return _ruleSetBagRepository;
		}

		public IGroupPageRepository CreateGroupPageRepository(IUnitOfWork unitOfWork)
		{
			return _groupPageRepository;
		}

		public IPartTimePercentageRepository CreatePartTimePercentageRepository(IUnitOfWork unitOfWork)
		{
			return _partTimePercentageRepository;
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
			return _noteRepository;
		}

		public IPublicNoteRepository CreatePublicNoteRepository(IUnitOfWork unitOfWork)
		{
			return _publicNoteRepository;
		}

		public IAgentDayScheduleTagRepository CreateAgentDayScheduleTagRepository(IUnitOfWork unitOfWork)
		{
			return _agentDayScheduleTagRepository;
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
			return _overtimeAvailabilityRepository;
		}

		public IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}
	}
}