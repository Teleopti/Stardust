using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	//LEGACY! DO NOT USE! Inject the repositories you need explicitly instead!
	public interface IRepositoryFactory
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
		IPersonRepository CreatePersonRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the absence repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2007-10-29
		/// </remarks>
		IAbsenceRepository CreateAbsenceRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the scenario repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		IScenarioRepository CreateScenarioRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the application function repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		IApplicationFunctionRepository CreateApplicationFunctionRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the application role repository.
		/// </summary>
		/// <param name="unitOfWork">The uow.</param>
		/// <returns></returns>
		IApplicationRoleRepository CreateApplicationRoleRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the available data repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		IAvailableDataRepository CreateAvailableDataRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the matrix repository.
		/// </summary>
		/// <returns></returns>
		IStatisticRepository CreateStatisticRepository();

		/// <summary>
		/// Creates the business unit repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		IBusinessUnitRepository CreateBusinessUnitRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the skill day repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-26
		/// </remarks>
		ISkillDayRepository CreateSkillDayRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the multisite day repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-24
		/// </remarks>
		IMultisiteDayRepository CreateMultisiteDayRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the skill repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-24
		/// </remarks>
		ISkillRepository CreateSkillRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the workload repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-24
		/// </remarks>
		IWorkloadRepository CreateWorkloadRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the contract schedule repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-10-22
		/// </remarks>
		IContractScheduleRepository CreateContractScheduleRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the contract repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-10-22
		/// </remarks>
		IContractRepository CreateContractRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the person request repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		IPersonRequestRepository CreatePersonRequestRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the shift category repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-11-11
		/// </remarks>
		IShiftCategoryRepository CreateShiftCategoryRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the day off repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-11-25
		/// </remarks>
		IDayOffTemplateRepository CreateDayOffRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the meeting repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-02-12
		/// </remarks>
		IMeetingRepository CreateMeetingRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the activity repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-02-12
		/// </remarks>
		IActivityRepository CreateActivityRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the multiplicator definition set repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-02-12
		/// </remarks>
		IMultiplicatorDefinitionSetRepository CreateMultiplicatorDefinitionSetRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates <see cref="ISiteRepository" /> repository instance.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns>A <see cref="ISiteRepository" /> instance.</returns>
		ISiteRepository CreateSiteRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates <see cref="ITeamRepository" /> repository instance.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns>A <see cref="ITeamRepository" /> instance.</returns>
		ITeamRepository CreateTeamRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the payroll export repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-04-15
		/// </remarks>
		IPayrollExportRepository CreatePayrollExportRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the multiplicator repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-04-15
		/// </remarks>
		IMultiplicatorRepository CreateMultiplicatorRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the conversation repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-05-18
		/// </remarks>
		IPushMessageRepository CreatePushMessageRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the conversation dialogue repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-05-18
		/// </remarks>
		IPushMessageDialogueRepository CreatePushMessageDialogueRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the global setting data repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-09-02
		/// </remarks>
		ISettingDataRepository CreateGlobalSettingDataRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the preference day repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		IPreferenceDayRepository CreatePreferenceDayRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the student availability day repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		IStudentAvailabilityDayRepository CreateStudentAvailabilityDayRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the work shift rule set repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-12-08
		/// </remarks>
		IWorkShiftRuleSetRepository CreateWorkShiftRuleSetRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the rule set bag repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-12-08
		/// </remarks>
		IRuleSetBagRepository CreateRuleSetBagRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the group page repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-01-07
		/// </remarks>
		IGroupPageRepository CreateGroupPageRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the part time percentage repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-01-07
		/// </remarks>
		IPartTimePercentageRepository CreatePartTimePercentageRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the optional column repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-01-07
		/// </remarks>
		IOptionalColumnRepository CreateOptionalColumnRepository(IUnitOfWork unitOfWork);

		/// <summary>
		/// Creates the workflow control set repository.
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-04-23
		/// </remarks>
		IWorkflowControlSetRepository CreateWorkflowControlSetRepository(IUnitOfWork unitOfWork);

		IExtendedPreferenceTemplateRepository CreateExtendedPreferenceTemplateRepository(IUnitOfWork unitOfWork);
		IRepository<IBudgetGroup> CreateBudgetGroupRepository(IUnitOfWork unitOfWork);

		IMasterActivityRepository CreateMasterActivityRepository(IUnitOfWork unitOfWork);
		ILicenseStatusRepository CreateLicenseStatusRepository(IUnitOfWork unitOfWork);
		ILicenseRepository CreateLicenseRepository(IUnitOfWork unitOfWork);
		IRequestHistoryReadOnlyRepository CreateRequestHistoryReadOnlyRepository(IStatelessUnitOfWork unitOfWork);
		IPersonScheduleDayReadModelFinder CreatePersonScheduleDayReadModelFinder(IUnitOfWork unitOfWork);
		IPersonalSettingDataRepository CreatePersonalSettingDataRepository(IUnitOfWork unitOfWork);
		ISkillTypeRepository CreateSkillTypeRepository(IUnitOfWork unitOfWork);
	}
}
