﻿using System;
using System.IO;
using System.Linq;
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

		private static string warningMessage(string column)
		{
			return string.Format(Resources.ImportAgentsColumnFixedWithFallbackValue, column);
		}

		[Test, Ignore("file parsing")]
		public void ShouldParseWorkbookFromHttpContent()
		{
			var fileData = new FileData();						
			using (var file = new FileStream(@"C:\TeleoptiWFM\SourceCode\main\Teleopti\Logs\test.xls", FileMode.Open, FileAccess.Read))
			{
				var ms = new MemoryStream();
				file.CopyTo(ms);
				
				fileData.FileName = "test.xls";
				fileData.Data = ms.ToArray();
			}
						
			var result = Target.ParseFile(fileData);

			result.Should().Not.Be.Null();
			result.GetType().Should().Be<HSSFWorkbook>();
		}

		[Test]
		public void ShouldReturnNullWithWrongFileType()
		{
			var ms = new MemoryStream();
			var fileData = new FileData
			{
				FileName = "test.jpg",
				Data = ms.ToArray()
			};

			var result = Target.ParseFile(fileData);

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
			result.First().Feedback.ErrorMessages.Count.Should().Be(11);
		}


		[Test]
		public void ShouldSpotUserWithEmptyFirstnameAndLastname()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { Firstname = "", Lastname = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			var errorMsg = result.Single().Feedback.ErrorMessages;
			errorMsg.Contains(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldSpotUserWithEmptyApplicationLogonIdAndWindowsUser()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { ApplicationUserId = "", WindowsUser = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook);

			var errorMsg = result.Single().Feedback.ErrorMessages;
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

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidSchedulePeriodType()
		{
			var rawAgent = setupProviderData();
			rawAgent.SchedulePeriodType = "Invalid schedule period type";

			var defaultSchedulePeriodType = "Week";

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook, new ImportAgentFormData { SchedulePeriodType = defaultSchedulePeriodType});

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("SchedulePeriodType"));
			result.Single().Agent.SchedulePeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidExternalLogon()
		{
			var rawAgent = setupProviderData();
			rawAgent.ExternalLogon = "Invalid external logon";

			var externalLogon = ExternalLogOnFactory.CreateExternalLogOn().WithId();
			ExternalLogOnRepository.Add(externalLogon);

			var defaultExternalLogon = externalLogon.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { ExternalLogonId = defaultExternalLogon });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ExternalLogon"));
			result.Single().Agent.ExternalLogon.Should().Be.EqualTo(externalLogon);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidRuleSetBag()
		{
			var rawAgent = setupProviderData();
			rawAgent.ShiftBag = "Invalid shift bag";

			var ruleSetBag = new RuleSetBag().WithId();			
			RuleSetBagRepository.Add(ruleSetBag);

			var defaultValue = ruleSetBag.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { ShiftBagId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ShiftBag"));
			result.Single().Agent.RuleSetBag.Should().Be.EqualTo(ruleSetBag);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidPartTimePercentage()
		{
			var rawAgent = setupProviderData();
			rawAgent.PartTimePercentage = "Invalid parttime percentage";

			var defaultEntity = PartTimePercentageFactory.CreatePartTimePercentage("default");
			PartTimePercentageRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { PartTimePercentageId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("PartTimePercentage"));
			result.Single().Agent.PartTimePercentage.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidContractSchedule()
		{
			var rawAgent = setupProviderData();
			rawAgent.ContractSchedule = "Invalid contract schedule";

			var defaultEntity = ContractScheduleFactory.CreateContractSchedule("default").WithId();
			ContractScheduleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { ContractScheduleId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ContractSchedule"));
			result.Single().Agent.ContractSchedule.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidContract()
		{
			var rawAgent = setupProviderData();
			rawAgent.Contract = "Invalid contract";

			var defaultEntity = ContractFactory.CreateContract("default").WithId();
			ContractRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { ContractId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("Contract"));
			result.Single().Agent.Contract.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidRoles()
		{
			var rawAgent = setupProviderData();
			rawAgent.Role = "invalid role";

			var defaultEntity = ApplicationRoleFactory.CreateRole("default", "default role").WithId();
			RoleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { RoleIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("Roles"));
			result.Single().Agent.Roles.Single().Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidTeam()
		{
			var rawAgent = setupProviderData();
			rawAgent.Organization = "invalid site/invalid team";

			var defaultEntity = TeamFactory.CreateSimpleTeam("default").WithId();
			TeamRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { TeamId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("Team"));
			result.Single().Agent.Team.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidSkill()
		{
			var rawAgent = setupProviderData();
			rawAgent.Skill = "invalid skill";

			var defaultEntity = SkillFactory.CreateSkill("default").WithId();
			SkillRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessWorkbook(workbook,new ImportAgentFormData { SkillIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("Skills"));
			result.Single().Agent.Skills.Single().Should().Be.EqualTo(defaultEntity);
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
