using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	/// <summary>
	/// Tests for RepositoryFactory
	/// </summary>
	[TestFixture]
	[Category("LongRunning")]
	public class RepositoryFactoryTest
	{
		private MockRepository mocks;
		private IUnitOfWork uow;

		/// <summary>
		/// Runs once per test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			uow = mocks.StrictMock<IUnitOfWork>();
		}

		/// <summary>
		/// Runs after each test.
		/// </summary>
		[TearDown]
		public void Teardown()
		{
			mocks.VerifyAll();
		}

		/// <summary>
		/// Verifies that PersonRepository can be created.
		/// </summary>
		[Test]
		public void VerifyPersonRepositoryCanBeCreated()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonRepository(uow));
		}

		/// <summary>
		/// Verifies the skill type repository can be created.
		/// Don't really understand this factory, but anyway
		/// </summary>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-10-19
		/// </remarks>
		[Test]
		public void VerifySkillTypeRepositoryCanBeCreated()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateSkillTypeRepository(uow));
		}

		[Test]
		public void VerifyPersonAbsenceAccountRepositoryCanBeCreated()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonAbsenceAccountRepository(uow));
		}

		/// <summary>
		/// Verifies the skill repository can be created.
		/// </summary>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-10-19
		/// </remarks>
		[Test]
		public void VerifySkillRepositoryCanBeCreated()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateSkillRepository(uow));
		}

		[Test]
		public void VerifyCreateAbsenceRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateAbsenceRepository(uow));
		}

		[Test]
		public void VerifyCreateBudgetGroupRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateBudgetGroupRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonAssignmentRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonAssignmentRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonAbsenceRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonAbsenceRepository(uow));
		}

		[Test]
		public void VerifyCreateScenarioRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateScenarioRepository(uow));
		}

		[Test]
		public void VerifyCreateApplicationFunctionRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateApplicationFunctionRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkloadRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateWorkloadRepository(uow));
		}

		[Test]
		public void VerifyCreateSkillDayRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateSkillDayRepository(uow));
		}

		[Test]
		public void VerifyCreateValidatedVolumeDayRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateValidatedVolumeDayRepository(uow));
		}

		[Test]
		public void VerifyCreateMultisiteDayRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateMultisiteDayRepository(uow));
		}

		[Test]
		public void VerifyCreateSkillRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateSkillRepository(uow));
		}

		[Test]
		public void VerifyCreateScheduleRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateScheduleRepository(uow));
		}

		[Test]
		public void VerifyCreateContractScheduleRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateContractScheduleRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonRequestRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonRequestRepository(uow));
		}

		[Test]
		public void VerifyCreateShiftCategoryRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateShiftCategoryRepository(uow));
		}

		[Test]
		public void VerifyCreateApplicationRoleRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateApplicationRoleRepository(uow));
		}


		[Test]
		public void VerifyCreateSiteRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateSiteRepository(uow));
		}

		[Test]
		public void VerifyCreateTeamRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateTeamRepository(uow));
		}

		[Test]
		public void VerifyCreateMultiplicatorDefinitionSetRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateMultiplicatorDefinitionSetRepository(uow));
		}

		[Test]
		public void VerifyCreateAvailableDataRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateAvailableDataRepository(uow));
		}

		[Test]
		public void VerifyCreateStatisticRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateStatisticRepository());
		}

		[Test]
		public void VerifyCreateBusinessUnitRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateBusinessUnitRepository(uow));
		}

		[Test]
		public void VerifyCreateContractRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateContractRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonRotationRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonRotationRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonAvailabilityRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePersonAvailabilityRepository(uow));
		}

		[Test]
		public void VerifyCreateDayOffRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateDayOffRepository(uow));
		}

		[Test]
		public void VerifyCreateMeetingRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateMeetingRepository(uow));
		}

		[Test]
		public void VerifyCreateActivityRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateActivityRepository(uow));
		}

		[Test]
		public void VerifyCreatePayrollExportRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePayrollExportRepository(uow));
		}

		[Test]
		public void VerifyCreateMultiplicatorRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateMultiplicatorRepository(uow));
		}

		[Test]
		public void VerifyCreateConversationRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePushMessageRepository(uow));
		}

		[Test]
		public void VerifyCreateConversationDialogueRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePushMessageDialogueRepository(uow));
		}

		[Test]
		public void VerifyCreateGlobalSettingDataRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateGlobalSettingDataRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkShiftRuleSetRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateWorkShiftRuleSetRepository(uow));
		}

		[Test]
		public void VerifyCreateRuleSetBagRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateRuleSetBagRepository(uow));
		}

		[Test]
		public void VerifyCreateGroupPageRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateGroupPageRepository(uow));
		}

		[Test]
		public void VerifyCreateOptionalColumnRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateOptionalColumnRepository(uow));
		}

		[Test]
		public void VerifyCreatePartTimePercentageRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePartTimePercentageRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkflowControlSetRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateWorkflowControlSetRepository(uow));
		}

		[Test]
		public void VerifyCreateStudentAvailabilityRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateStudentAvailabilityDayRepository(uow));
		}

		[Test]
		public void VerifyCreatePreferenceDayRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePreferenceDayRepository(uow));
		}

		[Test]
		public void VerifyCreateExtendedPreferenceTemplateRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateExtendedPreferenceTemplateRepository(uow));
		}

		[Test]
		public void VerifyCreateNoteRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateNoteRepository(uow));
		}

		[Test]
		public void VerifyCreatePublicNoteRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreatePublicNoteRepository(uow));
		}

		[Test]
		public void VerifyCreateAgentDayScheduleTagRepository()
		{
			mocks.ReplayAll();
			Assert.IsNotNull(new RepositoryFactory().CreateAgentDayScheduleTagRepository(uow));
		}
	}

}