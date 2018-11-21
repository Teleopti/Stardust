using System;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture, DomainTest]
	public class FileProcessorTest : IIsolateSystem, IExtendSystem
	{
		public FileProcessor Target;
		public FakeApplicationRoleRepository RoleRepository;
		public FakeContractRepository ContractRepository;
		public FakeContractScheduleRepository ContractScheduleRepository;
		public FakePartTimePercentageRepository PartTimePercentageRepository;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeExternalLogOnRepository ExternalLogOnRepository;
		public PersistPersonInfoFake PersonInfoPersister;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public FakeCurrentDatasource CurrentDatasource;
		public FakeTenants FindTenantByName;
		private AgentFileTemplate _agentFileTemplate;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FakeDatabase>();
			extend.AddService<FakeStorage>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FileProcessor>().For<IFileProcessor>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			isolate.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			isolate.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			isolate.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeExternalLogOnRepository>().For<IExternalLogOnRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();


			isolate.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			isolate.UseTestDouble<CheckPasswordStrengthFake>().For<ICheckPasswordStrength>();
			isolate.UseTestDouble<DeletePersonInfoFake>().For<IDeletePersonInfo>();
			isolate.UseTestDouble<ApplicationUserQueryFake>().For<IApplicationUserQuery>();
			isolate.UseTestDouble<IdentityUserQueryFake>().For<IIdentityUserQuery>();
			isolate.UseTestDouble<IdUserQueryFake>().For<IIdUserQuery>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			isolate.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			isolate.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			_agentFileTemplate = new AgentFileTemplate();
		}
		[Test, Ignore("file parsing")]
		public void ShouldParseWorkbookFromHttpContent()
		{
			var fileData = new ImportFileData();
			using (var file = new FileStream(@"C:\TeleoptiWFM\SourceCode\main\Teleopti\Logs\test.xls", FileMode.Open, FileAccess.Read))
			{
				var ms = new MemoryStream();
				file.CopyTo(ms);

				fileData.FileName = "test.xls";
				fileData.Data = ms.ToArray();
			}

			var result = Target.ParseFile(fileData);
			result.Should().Not.Be.Null();
		}
		[Test]
		public void ShouldReturnNullWithWrongFileType()
		{
			var ms = new MemoryStream();
			var fileData = new ImportFileData
			{
				FileName = "test.jpg",
				Data = ms.ToArray()
			};

			var result = Target.Process(fileData, TimeZoneInfo.Utc);
			result.Feedback.ErrorMessages.Single().Should().Be(Resources.InvalidInput);
		}
		[Test]
		public void ShouldPersistAgentWithValidData()
		{
			var rawAgent = setupProviderData();
			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, TimeZoneInfo.Utc);

			result.FailedCount.Should().Be(0);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldRollbackPersistedTenantUserWhenThereIsException()
		{

			var rawAgent = setupProviderData();
			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			Target.HasException();
			try
			{
				Target.Process(workbook, TimeZoneInfo.Utc);
			}
			catch (Exception)
			{
				PersonInfoPersister.RollBacked.Should().Be(true);
			}
			finally
			{
				Target.ResetException();
			}
		}

		[Test]
		public void ShouldCreateReturnedFileFromTheRawData()
		{
			var template = new AgentFileTemplate();
			var workbook = template.GetTemplateWorkbook("agents", true);
			var row = workbook.GetSheetAt(0).CreateRow(0);
			row.CreateCell(0).SetCellValue("aa");
			row.CreateCell(1).SetCellValue("bb");
			row.CreateCell(5).SetCellValue("agent");
			var agents = new []
			{
				new AgentExtractionResult
				{
					Feedback = { ErrorMessages = { "has error"}},
					Row = row
				}
			};

			var ms = Target.CreateFileForInvalidAgents(agents, true);

			ms.Should().Not.Be.Null();
		}

		private RawAgent setupProviderData()
		{
			CurrentDatasource.FakeName("test");
			FindTenantByName.Has(new Tenant("test"));
			var role = ApplicationRoleFactory.CreateRole("agent", "role description");

			RoleRepository.Add(role);
			var team = TeamFactory.CreateSimpleTeam("preference");
			var site = SiteFactory.CreateSimpleSite("London");
			SiteRepository.Add(site);
			team.Site = site;
			TeamRepository.Add(team);
			var skill = SkillFactory.CreateSkill("test skill");
			SkillRepository.Add(skill);
			var externalLogon = ExternalLogOnFactory.CreateExternalLogOn();
			ExternalLogOnRepository.Add(externalLogon);
			var contract = ContractFactory.CreateContract("full");
			ContractRepository.Add(contract);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("test schedule");
			ContractScheduleRepository.Add(contractSchedule);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("partTime").WithId();
			PartTimePercentageRepository.Add(partTimePercentage);
			var shiftBag = new RuleSetBag(WorkShiftRuleSetFactory.Create());
			shiftBag.Description = new Description("test");
			RuleSetBagRepository.Add(shiftBag);

			return new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = role.DescriptionText,
				StartDate = new DateTime(2017, 3, 1),
				Organization = team.SiteAndTeam,
				Skill = skill.Name,
				ExternalLogon = externalLogon.AcdLogOnName,
				Contract = contract.Description.Name,
				ContractSchedule = contractSchedule.Description.Name,
				PartTimePercentage = partTimePercentage.Description.Name,
				ShiftBag = shiftBag.Description.Name,
				SchedulePeriodType = "Week",
				SchedulePeriodLength = 4
			};
		}
	}
}

