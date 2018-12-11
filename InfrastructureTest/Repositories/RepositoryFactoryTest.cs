using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	/// <summary>
	/// Tests for RepositoryFactory
	/// </summary>
	[TestFixture]
	[Category("BucketB")]
	public class RepositoryFactoryTest
	{
		private IUnitOfWork uow;

		/// <summary>
		/// Runs once per test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			uow = new FakeUnitOfWork(new FakeStorage());
		}

		/// <summary>
		/// Verifies that PersonRepository can be created.
		/// </summary>
		[Test]
		public void VerifyPersonRepositoryCanBeCreated()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePersonRepository(uow));
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
			Assert.IsNotNull(new RepositoryFactory().CreateSkillRepository(uow));
		}

		[Test]
		public void VerifyCreateAbsenceRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateAbsenceRepository(uow));
		}

		[Test]
		public void VerifyCreateBudgetGroupRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateBudgetGroupRepository(uow));
		}
		
		[Test]
		public void VerifyCreateScenarioRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateScenarioRepository(uow));
		}

		[Test]
		public void VerifyCreateApplicationFunctionRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateApplicationFunctionRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkloadRepository()
		{
			
			Assert.IsNotNull(new RepositoryFactory().CreateWorkloadRepository(uow));
		}

		[Test]
		public void VerifyCreateSkillDayRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateSkillDayRepository(uow));
		}

		[Test]
		public void VerifyCreateMultisiteDayRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateMultisiteDayRepository(uow));
		}

		[Test]
		public void VerifyCreateSkillRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateSkillRepository(uow));
		}

		[Test]
		public void VerifyCreateContractScheduleRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateContractScheduleRepository(uow));
		}

		[Test]
		public void VerifyCreatePersonRequestRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePersonRequestRepository(uow));
		}

		[Test]
		public void VerifyCreateShiftCategoryRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateShiftCategoryRepository(uow));
		}

		[Test]
		public void VerifyCreateApplicationRoleRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateApplicationRoleRepository(uow));
		}

		[Test]
		public void VerifyCreateSiteRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateSiteRepository(uow));
		}

		[Test]
		public void VerifyCreateTeamRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateTeamRepository(uow));
		}

		[Test]
		public void VerifyCreateMultiplicatorDefinitionSetRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateMultiplicatorDefinitionSetRepository(uow));
		}

		[Test]
		public void VerifyCreateAvailableDataRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateAvailableDataRepository(uow));
		}

		[Test]
		public void VerifyCreateStatisticRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateStatisticRepository());
		}

		[Test]
		public void VerifyCreateBusinessUnitRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateBusinessUnitRepository(uow));
		}

		[Test]
		public void VerifyCreateContractRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateContractRepository(uow));
		}
		
		[Test]
		public void VerifyCreateDayOffRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateDayOffRepository(uow));
		}

		[Test]
		public void VerifyCreateMeetingRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateMeetingRepository(uow));
		}

		[Test]
		public void VerifyCreateActivityRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateActivityRepository(uow));
		}

		[Test]
		public void VerifyCreatePayrollExportRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePayrollExportRepository(uow));
		}

		[Test]
		public void VerifyCreateMultiplicatorRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateMultiplicatorRepository(uow));
		}

		[Test]
		public void VerifyCreateConversationRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePushMessageRepository(uow));
		}

		[Test]
		public void VerifyCreateConversationDialogueRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePushMessageDialogueRepository(uow));
		}

		[Test]
		public void VerifyCreateGlobalSettingDataRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateGlobalSettingDataRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkShiftRuleSetRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateWorkShiftRuleSetRepository(uow));
		}

		[Test]
		public void VerifyCreateRuleSetBagRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateRuleSetBagRepository(uow));
		}

		[Test]
		public void VerifyCreateGroupPageRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateGroupPageRepository(uow));
		}

		[Test]
		public void VerifyCreateOptionalColumnRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateOptionalColumnRepository(uow));
		}

		[Test]
		public void VerifyCreatePartTimePercentageRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePartTimePercentageRepository(uow));
		}

		[Test]
		public void VerifyCreateWorkflowControlSetRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateWorkflowControlSetRepository(uow));
		}

		[Test]
		public void VerifyCreateStudentAvailabilityRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateStudentAvailabilityDayRepository(uow));
		}

		[Test]
		public void VerifyCreatePreferenceDayRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreatePreferenceDayRepository(uow));
		}

		[Test]
		public void VerifyCreateExtendedPreferenceTemplateRepository()
		{
			Assert.IsNotNull(new RepositoryFactory().CreateExtendedPreferenceTemplateRepository(uow));
		}
	}
}