using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.WebTest.Areas.People.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture, WebPeopleTest]
	public class FileProcessorTest:ISetup
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

		public TenantUnitOfWorkFake TenantUnitOfWork;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeDatabase>();
			system.AddService<FakeStorage>();
			system.UseTestDouble<FileProcessor>().For<IFileProcessor>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			system.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeExternalLogOnRepository>().For<IExternalLogOnRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();


			system.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			system.UseTestDouble<CheckPasswordStrengthFake>().For<ICheckPasswordStrength>();
			system.UseTestDouble<DeletePersonInfoFake>().For<IDeletePersonInfo>();
			system.UseTestDouble<ApplicationUserQueryFake>().For<IApplicationUserQuery>();
			system.UseTestDouble<IdentityUserQueryFake>().For<IIdentityUserQuery>();
			system.UseTestDouble<IdUserQueryFake>().For<IIdUserQuery>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			system.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			system.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<LogLogonAttemptFake>().For<ILogLogonAttempt>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
		}

		[Test, Ignore("file parsing")]
		public void ShouldParseWorkbookFromHttpContent()
		{
			HttpContent content;
			using (FileStream file = new FileStream(@"C:\TeleoptiWFM\SourceCode\main\Teleopti\Logs\test.xls", FileMode.Open, FileAccess.Read))
			{
				var ms = new MemoryStream();
				file.CopyTo(ms);
				content = new ByteArrayContent(ms.ToArray());
				content.Headers.ContentDisposition = new ContentDispositionHeaderValue("file") {FileName = "\"test.xls\""};
			}
			

			var result = Target.ParseFiles(content);

			result.Should().Not.Be.Null();
			result.GetType().Should().Be<HSSFWorkbook>();
		}

		[Test]
		public void ShouldReturnNullWithWrongFileType()
		{
			var ms = new MemoryStream();
			HttpContent content = new ByteArrayContent(ms.ToArray());
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("file") {FileName = "\"test.img\""};


			var result = Target.ParseFiles(content);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldValidateWorkbookWithCorrectColumnHeader()
		{
			var fileTemplate = new AgentFileTemplate();
			var workbook = fileTemplate.GetTemplateWorkbook("test");
			

			var result = Target.ValidateWorkbook(workbook);

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldValidateExcelFileWithCorrectColumnHeaderAndRedundantColumns()
		{
			var fileTemplate = new AgentFileTemplate();
			var workbook = fileTemplate.GetTemplateWorkbook("test");
			workbook.GetSheetAt(0).GetRow(0).CreateCell(fileTemplate.ColumnHeaderNames.Length).SetCellValue("redundant");

			var result = Target.ValidateWorkbook(workbook);

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnMissingHeaderForInvalidFile()
		{
			HSSFWorkbook hssfwb = new HSSFWorkbook();

			var result = Target.ValidateWorkbook(hssfwb);

			result.Count.Should().Be.EqualTo(16);
		}

		[Test]
		public void ShouldParseRawDataWithCorrectColumnInput()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent {Firstname = "test", StartDate = new DateTime(2017, 3, 1)});
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			result.Count.Should().Be.EqualTo(1);
			result.First().Raw.Firstname.Should().Be("test");
			result.First().Raw.StartDate.Should().Be(new DateTime(2017, 3, 1));
		}

		[Test]
		public void ShouldWriteErrorMsgForInvalidInput()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent {Firstname = "test", StartDate = new DateTime(2017, 3, 1)});
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			result.Count.Should().Be.EqualTo(1);
			result.First().Agent.Should().Be.Null();
			result.First().ErrorMessages.Count.Should().Be(9);
		}


		[Test]
		public void ShouldSpotUserWithEmptyFirstnameAndLastname()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { Firstname = "", Lastname = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			var errorMsg = result.Single().ErrorMessages;
			errorMsg.Contains(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldSpotUserWithEmptyApplicationLogonIdAndWindowsUser()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { ApplicationUserId = "", WindowsUser = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			var errorMsg = result.Single().ErrorMessages;
			errorMsg.Contains(Resources.NoLogonAccountErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldPersistAgentWithValidData()
		{
			var rawAgent = setupProviderData();
			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			result.Count.Should().Be(0);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		private RawAgent setupProviderData()
		{
			var role = ApplicationRoleFactory.CreateRole("agent", "test");

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
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("partTime");
			PartTimePercentageRepository.Add(partTimePercentage);
			var shiftBag = new RuleSetBag(WorkShiftRuleSetFactory.Create());
			RuleSetBagRepository.Add(shiftBag);

			return new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = role.Name,
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
