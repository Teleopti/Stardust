using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Factory for repositories.
	/// </summary>
	/// <remarks>
	/// Just here for easier unit tests right now.
	/// Every repository this way later?
	/// </remarks>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class RepositoryFactory : IRepositoryFactory
	{
		/// <summary>
		/// Creates the person repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2007-11-29
		/// </remarks>
		public IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork)
		{
			return new PersonRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		/// <summary>
		/// Creates the absence repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// 
		/// Created date: 2007-10-29
		/// </remarks>
		public IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork)
		{
			return AbsenceRepository.DONT_USE_CTOR(unitOfWork);
		}
		
		/// <summary>
		/// Creates the scenario repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		public IScenarioRepository CreateScenarioRepository(IUnitOfWork unitOfWork)
		{
			return ScenarioRepository.DONT_USE_CTOR(unitOfWork);
		}

		/// <summary>
		/// Creates the application function repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		public IApplicationFunctionRepository CreateApplicationFunctionRepository(IUnitOfWork unitOfWork)
		{
			return ApplicationFunctionRepository.DONT_USE_CTOR(unitOfWork);
		}

		/// <summary>
		/// Creates the Skill repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		public ISkillRepository CreateSkillRepository(IUnitOfWork unitOfWork)
		{
			return SkillRepository.DONT_USE_CTOR(unitOfWork);
		}


		/// <summary>
		/// Creates the Workload repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		public IWorkloadRepository CreateWorkloadRepository(IUnitOfWork unitOfWork)
		{
			return new WorkloadRepository(unitOfWork);
		}

		/// <summary>
		/// Creates the application role repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		public IApplicationRoleRepository CreateApplicationRoleRepository(IUnitOfWork unitOfWork)
		{
			return ApplicationRoleRepository.DONT_USE_CTOR(unitOfWork);
		}

		/// <summary>
		/// Creates the available data repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		public IAvailableDataRepository CreateAvailableDataRepository(IUnitOfWork unitOfWork)
		{
			return AvailableDataRepository.DONT_USE_CTOR(unitOfWork);
		}

		/// <summary>
		/// Creates the statistic (matrix) repository.
		/// </summary>
		/// <returns></returns>
		public IStatisticRepository CreateStatisticRepository()
		{
			return StatisticRepositoryFactory.Create();
		}
		
		/// <summary>
		/// Creates the business unit repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		public IBusinessUnitRepository CreateBusinessUnitRepository(IUnitOfWork unitOfWork)
		{
			return BusinessUnitRepository.DONT_USE_CTOR(unitOfWork);
		}

		/// <summary>
		/// Creates the skill day repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-26
		/// </remarks>
		public ISkillDayRepository CreateSkillDayRepository(IUnitOfWork unitOfWork)
		{
			return new SkillDayRepository(unitOfWork);
		}

		public IMultisiteDayRepository CreateMultisiteDayRepository(IUnitOfWork unitOfWork)
		{
			return MultisiteDayRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IContractScheduleRepository CreateContractScheduleRepository(IUnitOfWork unitOfWork)
		{
			return ContractScheduleRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IContractRepository CreateContractRepository(IUnitOfWork unitOfWork)
		{
			return ContractRepository.DONT_USE_CTOR(unitOfWork);
		}
		
		public IPreferenceDayRepository CreatePreferenceDayRepository(IUnitOfWork unitOfWork)
		{
			return new PreferenceDayRepository(new ThisUnitOfWork(unitOfWork));
		}

		public IStudentAvailabilityDayRepository CreateStudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
		{
			return new StudentAvailabilityDayRepository(unitOfWork);
		}
		
		public IWorkShiftRuleSetRepository CreateWorkShiftRuleSetRepository(IUnitOfWork unitOfWork)
		{
			return new WorkShiftRuleSetRepository(unitOfWork);
		}

		public IRuleSetBagRepository CreateRuleSetBagRepository(IUnitOfWork unitOfWork)
		{
			return new RuleSetBagRepository(unitOfWork);
		}

		public IPersonRequestRepository CreatePersonRequestRepository(IUnitOfWork unitOfWork)
		{
			return new PersonRequestRepository(unitOfWork);
		}

		public IShiftCategoryRepository CreateShiftCategoryRepository(IUnitOfWork unitOfWork)
		{
			return new ShiftCategoryRepository(unitOfWork);
		}

		public IDayOffTemplateRepository CreateDayOffRepository(IUnitOfWork unitOfWork)
		{
			return DayOffTemplateRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IMeetingRepository CreateMeetingRepository(IUnitOfWork unitOfWork)
		{
			return new MeetingRepository(new ThisUnitOfWork(unitOfWork));
		}

		public IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork)
		{
			return ActivityRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IMultiplicatorDefinitionSetRepository CreateMultiplicatorDefinitionSetRepository(IUnitOfWork unitOfWork)
		{
			return MultiplicatorDefinitionSetRepository.DONT_USE_CTOR(unitOfWork);
		}

		public ITeamRepository CreateTeamRepository(IUnitOfWork unitOfWork)
		{
			return TeamRepository.DONT_USE_CTOR(unitOfWork);
		}

		public ISiteRepository CreateSiteRepository(IUnitOfWork unitOfWork)
		{
			return SiteRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IPayrollExportRepository CreatePayrollExportRepository(IUnitOfWork unitOfWork)
		{
			return new PayrollExportRepository(new ThisUnitOfWork(unitOfWork));
		}

		public IMultiplicatorRepository CreateMultiplicatorRepository(IUnitOfWork unitOfWork)
		{
			return MultiplicatorRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IPushMessageRepository CreatePushMessageRepository(IUnitOfWork unitOfWork)
		{
			return new PushMessageRepository(unitOfWork);
		}

		public IPushMessageDialogueRepository CreatePushMessageDialogueRepository(IUnitOfWork unitOfWork)
		{
			return new PushMessageDialogueRepository(unitOfWork);
		}

		public ISettingDataRepository CreateGlobalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			return GlobalSettingDataRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IGroupPageRepository CreateGroupPageRepository(IUnitOfWork unitOfWork)
		{
			return GroupPageRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IPartTimePercentageRepository CreatePartTimePercentageRepository(IUnitOfWork unitOfWork)
		{
			return PartTimePercentageRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IOptionalColumnRepository CreateOptionalColumnRepository(IUnitOfWork unitOfWork)
		{
			return OptionalColumnRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IWorkflowControlSetRepository CreateWorkflowControlSetRepository(IUnitOfWork unitOfWork)
		{
			return new WorkflowControlSetRepository(unitOfWork);
		}

		public IExtendedPreferenceTemplateRepository CreateExtendedPreferenceTemplateRepository(IUnitOfWork unitOfWork)
		{
			return new ExtendedPreferenceTemplateRepository(new ThisUnitOfWork(unitOfWork));
		}

		public IRepository<IBudgetGroup> CreateBudgetGroupRepository(IUnitOfWork unitOfWork)
		{
			return BudgetGroupRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IMasterActivityRepository CreateMasterActivityRepository(IUnitOfWork unitOfWork)
		{
			return MasterActivityRepository.DONT_USE_CTOR(unitOfWork);
		}
		
		public ILicenseStatusRepository CreateLicenseStatusRepository(IUnitOfWork unitOfWork)
		{
			return new LicenseStatusRepository(new ThisUnitOfWork(unitOfWork));
		}

		public ILicenseRepository CreateLicenseRepository(IUnitOfWork unitOfWork)
		{
			return LicenseRepository.DONT_USE_CTOR(unitOfWork);
		}

		public IRequestHistoryReadOnlyRepository CreateRequestHistoryReadOnlyRepository(IStatelessUnitOfWork unitOfWork)
		{
			return new RequestHistoryReadOnlyRepository(unitOfWork);
		}
		
		public IPersonScheduleDayReadModelFinder CreatePersonScheduleDayReadModelFinder(IUnitOfWork unitOfWork)
		{
			return new PersonScheduleDayReadModelFinder(unitOfWork);
		}

		public IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork)
		{
			return PersonalSettingDataRepository.DONT_USE_CTOR(unitOfWork);
		}

		public ISkillTypeRepository CreateSkillTypeRepository(IUnitOfWork unitOfWork)
		{
			return SkillTypeRepository.DONT_USE_CTOR(unitOfWork);
		}
	}
}
