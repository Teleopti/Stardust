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
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IMeetingRepository _meetingRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly IGroupPageRepository _groupPageRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IStatisticRepository _statisticRepository;

		public FakeRepositoryFactory(
			IPersonRepository personRepository,
			IBusinessUnitRepository businessUnitRepository,
			IActivityRepository activityRepository,
			IAbsenceRepository absenceRepository,
			IDayOffTemplateRepository dayOffTemplateRepository,
			IShiftCategoryRepository shiftCategoryRepository,
			IContractRepository contractRepository,
			IContractScheduleRepository contractScheduleRepository,
			IWorkflowControlSetRepository workflowControlSetRepository,
			IMeetingRepository meetingRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IStudentAvailabilityDayRepository studentAvailabilityDayRepository,
			IPartTimePercentageRepository partTimePercentageRepository,
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
			IRuleSetBagRepository ruleSetBagRepository, 
			IGroupPageRepository groupPageRepository,
			ISkillRepository skillRepository,
			ISkillTypeRepository skillTypeRepository,
			IStatisticRepository statisticRepository)
		{
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
			_activityRepository = activityRepository;
			_absenceRepository = absenceRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_shiftCategoryRepository = shiftCategoryRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_workflowControlSetRepository = workflowControlSetRepository;
			_meetingRepository = meetingRepository;
			_preferenceDayRepository = preferenceDayRepository;
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_groupPageRepository = groupPageRepository;
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
			_statisticRepository = statisticRepository;
		}

		public IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork)
		{
			return _personRepository;
		}

		public IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork)
		{
			return _absenceRepository;
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
			return _statisticRepository;
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
			return _skillRepository;
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
		
		public IPersonRequestRepository CreatePersonRequestRepository(IUnitOfWork unitOfWork)
		{
			return new FakePersonRequestRepository();
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
		
		public IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			throw new System.NotImplementedException();
		}

		public ISkillTypeRepository CreateSkillTypeRepository(IUnitOfWork unitOfWork)
		{
			return _skillTypeRepository;
		}
	}
}