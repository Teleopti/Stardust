using System.Collections.Generic;
using NHibernate.Properties;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
            return new PersonRepository(unitOfWork);
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
            return new AbsenceRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the agent assignment repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public IPersonAssignmentRepository CreatePersonAssignmentRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAssignmentRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the agent assignment repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public IPersonAbsenceRepository CreatePersonAbsenceRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAbsenceRepository(unitOfWork);
        }

        public IPersonAbsenceAccountRepository CreatePersonAbsenceAccountRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAbsenceAccountRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the scenario repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public IScenarioRepository CreateScenarioRepository(IUnitOfWork unitOfWork)
        {
            return new ScenarioRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the application function repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public IApplicationFunctionRepository CreateApplicationFunctionRepository(IUnitOfWork unitOfWork)
        {
            return new ApplicationFunctionRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the Skill repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public ISkillRepository CreateSkillRepository(IUnitOfWork unitOfWork)
        {
            return new SkillRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the SkillType repository.
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <returns></returns>
        public ISkillTypeRepository CreateSkillTypeRepository(IUnitOfWork unitOfWork)
        {
            return new SkillTypeRepository(unitOfWork);
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
            return new ApplicationRoleRepository(unitOfWork);
        }

        /// <summary>
        /// Creates the available data repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public IAvailableDataRepository CreateAvailableDataRepository(IUnitOfWork unitOfWork)
        {
            return new AvailableDataRepository(unitOfWork);
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
            return new BusinessUnitRepository(unitOfWork);
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

        /// <summary>
        /// Creates the validated volume day repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-26
        /// </remarks>
        public IValidatedVolumeDayRepository CreateValidatedVolumeDayRepository(IUnitOfWork unitOfWork)
        {
            return new ValidatedVolumeDayRepository(unitOfWork);
        }

        public IMultisiteDayRepository CreateMultisiteDayRepository(IUnitOfWork unitOfWork)
        {
            return new MultisiteDayRepository(unitOfWork);
        }

        public IScheduleRepository CreateScheduleRepository(IUnitOfWork unitOfWork)
        {
            return new ScheduleRepository(unitOfWork);
        }

        public IContractScheduleRepository CreateContractScheduleRepository(IUnitOfWork unitOfWork)
        {
            return new ContractScheduleRepository(unitOfWork);
        }

        public IContractRepository CreateContractRepository(IUnitOfWork unitOfWork)
        {
            return new ContractRepository(unitOfWork);
        }

        public IPersonRotationRepository CreatePersonRotationRepository(IUnitOfWork unitOfWork)
        {
            return new PersonRotationRepository(unitOfWork);
        }

        public IPersonAvailabilityRepository CreatePersonAvailabilityRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAvailabilityRepository(unitOfWork);
        }
        
        public IPreferenceDayRepository CreatePreferenceDayRepository(IUnitOfWork unitOfWork)
        {
            return new PreferenceDayRepository(unitOfWork);
        }

        public IStudentAvailabilityDayRepository CreateStudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
        {
            return new StudentAvailabilityDayRepository(unitOfWork);
        }

        public IOvertimeAvailabilityRepository CreateOvertimeAvailabilityRepository(IUnitOfWork unitOfWork)
        {
            return new OvertimeAvailabilityRepository(unitOfWork );
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
            return new DayOffTemplateRepository(unitOfWork);
        }

        public IMeetingRepository CreateMeetingRepository(IUnitOfWork unitOfWork)
        {
            return new MeetingRepository(unitOfWork);
        }

        public IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork)
        {
            return new ActivityRepository(unitOfWork);
        }

        public IMultiplicatorDefinitionSetRepository CreateMultiplicatorDefinitionSetRepository(IUnitOfWork unitOfWork)
        {
            return new MultiplicatorDefinitionSetRepository(unitOfWork);
        }

        public ITeamRepository CreateTeamRepository(IUnitOfWork unitOfWork)
        {
            return new TeamRepository(unitOfWork);
        }

        public ISiteRepository CreateSiteRepository(IUnitOfWork unitOfWork)
        {
            return new SiteRepository(unitOfWork);
        }

        public IPayrollExportRepository CreatePayrollExportRepository(IUnitOfWork unitOfWork )
        {
            return new PayrollExportRepository(unitOfWork);
        }

        public IMultiplicatorRepository CreateMultiplicatorRepository(IUnitOfWork unitOfWork)
        {
            return new MultiplicatorRepository(unitOfWork);
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
            return new GlobalSettingDataRepository(unitOfWork);
        }

        public IGroupPageRepository CreateGroupPageRepository(IUnitOfWork unitOfWork)
        {
            return new GroupPageRepository(unitOfWork);
        }

        public IPartTimePercentageRepository CreatePartTimePercentageRepository(IUnitOfWork unitOfWork)
        {
            return new PartTimePercentageRepository(unitOfWork);
        }

        public IOptionalColumnRepository CreateOptionalColumnRepository(IUnitOfWork unitOfWork)
        {
            return new OptionalColumnRepository(unitOfWork);
        }

        public IWorkflowControlSetRepository CreateWorkflowControlSetRepository(IUnitOfWork unitOfWork)
        {
            return new WorkflowControlSetRepository(unitOfWork);
        }

        public IUserDetailRepository CreateUserDetailRepository(IUnitOfWork unitOfWork)
        {
            return new UserDetailRepository(unitOfWork);
        }

        public IExtendedPreferenceTemplateRepository CreateExtendedPreferenceTemplateRepository(IUnitOfWork unitOfWork)
        {
            return new ExtendedPreferenceTemplateRepository(unitOfWork);
        }

        public IRepository<IBudgetGroup> CreateBudgetGroupRepository(IUnitOfWork unitOfWork)
        {
            return new BudgetGroupRepository(unitOfWork);
        }

        public IMasterActivityRepository CreateMasterActivityRepository(IUnitOfWork unitOfWork)
        {
            return new MasterActivityRepository(unitOfWork);
        }

        public INoteRepository CreateNoteRepository(IUnitOfWork unitOfWork)
        {
            return new NoteRepository(unitOfWork);
        }

        public IPublicNoteRepository CreatePublicNoteRepository(IUnitOfWork unitOfWork)
        {
            return new PublicNoteRepository(unitOfWork);
        }

        public IScheduleTagRepository CreateScheduleTagRepository(IUnitOfWork unitOfWork)
        {
            return new ScheduleTagRepository(unitOfWork);
        }

        public IApplicationRolePersonRepository CreateApplicationRolePersonRepository(IStatelessUnitOfWork unitOfWork)
        {
            return new ApplicationRolePersonRepository(unitOfWork);
        }

        public ILicenseStatusRepository CreateLicenseStatusRepository(IUnitOfWork unitOfWork)
        {
            return new LicenseStatusRepository(unitOfWork);
        }

        public ILicenseRepository CreateLicenseRepository(IUnitOfWork unitOfWork)
        {
            return new LicenseRepository(unitOfWork);
        }

        public IRequestHistoryReadOnlyRepository CreateRequestHistoryReadOnlyRepository(IStatelessUnitOfWork unitOfWork)
        {
            return new RequestHistoryReadOnlyRepository(unitOfWork);
        }

	    public IAgentDayScheduleTagRepository CreateAgentDayScheduleTagRepository(IUnitOfWork unitOfWork)
        {
            return new AgentDayScheduleTagRepository(unitOfWork);
        }

		public IPersonScheduleDayReadModelFinder CreatePersonScheduleDayReadModelFinder(IUnitOfWork unitOfWork)
		{
			return new PersonScheduleDayReadModelFinder(unitOfWork);
		}

	    public IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork)
	    {
			return new PersonalSettingDataRepository(unitOfWork);
	    }

}
}
