using System;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NUnit.Framework;
using SharpTestsEx;
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
	public class WorkbookHandlerTest : IIsolateSystem, IExtendSystem
	{
		public IWorkbookHandler Target;
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
			isolate.UseTestDouble<WorkbookHandler>().For<IWorkbookHandler>();

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

		[Test]
		public void ShouldValidateWorkbookWithCorrectColumnHeader()
		{
			var workbook = _agentFileTemplate.GetDefaultFileTemplate();
			var result = Target.Process(workbook);
			result.Feedback.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingHeaderForInvalidFile()
		{
			var workbook = _agentFileTemplate.GetDefaultFileTemplate();
			var row = workbook.GetSheetAt(0).GetRow(0);
			row.RemoveCell(row.GetCell(row.LastCellNum-1));
			var result = Target.Process(workbook);
			result.Feedback.ErrorMessages.Single().Should().Be(string.Format(Resources.MissingColumnX, _agentFileTemplate.ColumnHeaderNames.Last()));
		}

		[Test]
		public void ShouldValidateExcelFileWithCorrectColumnHeaderAndRedundantColumns()
		{
			var workbook = _agentFileTemplate.GetDefaultFileTemplate();
			workbook.GetSheetAt(0).GetRow(0).CreateCell(_agentFileTemplate.ColumnHeaderNames.Length).SetCellValue("redundant");
			var result = Target.Process(workbook);
			result.Feedback.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingHeaderForInvalidFileWithNontextHeader()
		{
			HSSFWorkbook hssfwb = new HSSFWorkbook();
			var sheet = hssfwb.CreateSheet("test");
			var row = sheet.CreateRow(0);
			row.CreateCell(0).SetCellValue(1234);
			var result = Target.Process(hssfwb);
			result.Feedback.ErrorMessages.Single().Should().Be(getMissingColumnsErrorMsg());
		}

		[Test]
		public void ShouldParseRawDataWithCorrectColumnInput()
		{
			var ms = _agentFileTemplate.GetFileTemplate(new RawAgent { Firstname = "test", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Raw.Firstname.Should().Be("test");
			result.RawResults.First().Raw.StartDate.Should().Be(new DateTime(2017, 3, 1));
		}

		[Test]
		public void ShouldParseRawValueIfExternalLogonColumnTypeNotMatch()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0).SetCellValue("ashley");
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue(new DateTime(2017, 3, 13));
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue(1009);

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue("partime");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);
			row.Cells[9].SetCellType(CellType.Numeric);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, nameof(RawAgent.ExternalLogon), string.Format(Resources.RequireXCellFormat, "text"))).Should().Be.True();
		}

		[Test]
		public void ShouldParseRawValueIfStartDateColumnTypeNotMatch()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0).SetCellValue("ashley");
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue("2017-09-23");
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue(1009);

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue("partime");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, nameof(RawAgent.StartDate), string.Format(Resources.RequireXCellFormat, "date"))).Should().Be.True();
		}

		[Test]
		public void ShouldParseRawValueIfPartimePercentageColumnTypeNotMatch()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
			row.CreateCell(0).SetCellValue("ashley");
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue("2017-09-23");
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue(1009);

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue(1);
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);
			row.Cells[12].SetCellType(CellType.Numeric);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, nameof(RawAgent.StartDate), string.Format(Resources.RequireXCellFormat, "date"))).Should().Be.True();
		}

		[Test]
		public void ShouldGiveCorrectErrorIfRequiredColumnCellIsEmpty()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0).SetCellValue("ashley");
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue(DateTime.Today);
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue("1002");

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue("100%");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			var errorMessage = string.Format(Resources.ColumnCanNotBeEmpty,
				_agentFileTemplate.GetColumnDisplayName(nameof(RawAgent.SchedulePeriodLength)));

			result.RawResults.First().Feedback.ErrorMessages.Contains(errorMessage).Should().Be.True();
		}

		[Test]
		public void ShouldNotGiveErrorIfNullableColumnCellIsEmpty()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0);
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue("2017-09-23");
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue(1009);

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue(1);
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15);
			row.Cells[12].SetCellType(CellType.Numeric);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, nameof(RawAgent.Firstname), string.Format(Resources.RequireXCellFormat, "text"))).Should().Be.False();
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySchedulePeriodType()
		{
			var rawAgent = setupProviderData();
			rawAgent.SchedulePeriodType = null;

			var defaultSchedulePeriodType = "Week";

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var sheet = workbook.GetSheetAt(0);
			sheet.GetRow(1).RemoveCell(sheet.GetRow(1).GetCell(14));

			var result = Target.Process(workbook, new ImportAgentDefaults { SchedulePeriodType = defaultSchedulePeriodType });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.SchedulePeriodType), defaultSchedulePeriodType));
			result.RawResults.Single().Agent.SchedulePeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidSchedulePeriodType()
		{
			var rawAgent = setupProviderData();
			rawAgent.SchedulePeriodType = "Invalid schedule period type";

			var defaultSchedulePeriodType = "Week";

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { SchedulePeriodType = defaultSchedulePeriodType });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.SchedulePeriodType), defaultSchedulePeriodType));
			result.RawResults.Single().Agent.SchedulePeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidExternalLogon()
		{
			var rawAgent = setupProviderData();
			rawAgent.ExternalLogon = "Invalid external logon";

			var externalLogon = ExternalLogOnFactory.CreateExternalLogOn().WithId();
			ExternalLogOnRepository.Add(externalLogon);

			var defaultExternalLogon = externalLogon.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { ExternalLogonId = defaultExternalLogon });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ExternalLogon), externalLogon.AcdLogOnName));
			result.RawResults.Single().Agent.ExternalLogons.Single().Should().Be.EqualTo(externalLogon);
		}

		[Test]
		public void WithEmptyDefaultsProvidedShouldBeAbleToFixInvalidExternalLogon()
		{
			var rawAgent = setupProviderData();
			rawAgent.ExternalLogon = "Invalid external logon";

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { ExternalLogonId = Guid.Empty.ToString() });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ExternalLogon), ""));
			result.RawResults.Single().Agent.ExternalLogons.Count.Should().Be(0);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidRuleSetBag()
		{
			var rawAgent = setupProviderData();
			rawAgent.ShiftBag = "Invalid shift bag";

			var ruleSetBag = new RuleSetBag().WithId();
			RuleSetBagRepository.Add(ruleSetBag);

			var defaultValue = ruleSetBag.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { ShiftBagId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ShiftBag), ruleSetBag.Description));
			result.RawResults.Single().Agent.RuleSetBag.Should().Be.EqualTo(ruleSetBag);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyRuleSetBag()
		{
			var rawAgent = setupProviderData();

			var ruleSetBag = new RuleSetBag().WithId();
			RuleSetBagRepository.Add(ruleSetBag);

			var defaultValue = ruleSetBag.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(13));

			var result = Target.Process(workbook, new ImportAgentDefaults { ShiftBagId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ShiftBag), ruleSetBag.Description));
			result.RawResults.Single().Agent.RuleSetBag.Should().Be.EqualTo(ruleSetBag);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyPartTimePercentage()
		{
			var rawAgent = setupProviderData();
			var defaultEntity = PartTimePercentageFactory.CreatePartTimePercentage("default").WithId();
			PartTimePercentageRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(12));
			row.CreateCell(12);

			var result = Target.Process(workbook, new ImportAgentDefaults { PartTimePercentageId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.PartTimePercentage), defaultEntity.Description.Name));
			result.RawResults.Single().Agent.PartTimePercentage.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidPartTimePercentage()
		{
			var rawAgent = setupProviderData();
			rawAgent.PartTimePercentage = "Invalid parttime percentage";

			var defaultEntity = PartTimePercentageFactory.CreatePartTimePercentage("default");
			PartTimePercentageRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { PartTimePercentageId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.PartTimePercentage), defaultEntity.Description.Name));
			result.RawResults.Single().Agent.PartTimePercentage.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyContractSchedule()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = ContractScheduleFactory.CreateContractSchedule("default").WithId();
			ContractScheduleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(11));
			row.CreateCell(11);

			var result = Target.Process(workbook, new ImportAgentDefaults { ContractScheduleId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ContractSchedule), defaultEntity.Description));
			result.RawResults.Single().Agent.ContractSchedule.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidContractSchedule()
		{
			var rawAgent = setupProviderData();
			rawAgent.ContractSchedule = "Invalid contract schedule";

			var defaultEntity = ContractScheduleFactory.CreateContractSchedule("default").WithId();
			ContractScheduleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { ContractScheduleId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.ContractSchedule), defaultEntity.Description));
			result.RawResults.Single().Agent.ContractSchedule.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyContract()
		{
			var rawAgent = setupProviderData();
			rawAgent.Contract = "Invalid contract";

			var defaultEntity = ContractFactory.CreateContract("default").WithId();
			ContractRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(10));

			var result = Target.Process(workbook, new ImportAgentDefaults { ContractId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Contract), defaultEntity.Description.Name));
			result.RawResults.Single().Agent.Contract.Should().Be.EqualTo(defaultEntity);
		}
		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidContract()
		{
			var rawAgent = setupProviderData();
			rawAgent.Contract = "Invalid contract";

			var defaultEntity = ContractFactory.CreateContract("default").WithId();
			ContractRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { ContractId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Contract), defaultEntity.Description));
			result.RawResults.Single().Agent.Contract.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyRole()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = ApplicationRoleFactory.CreateRole("default", "default role").WithId();
			RoleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(5));

			var result = Target.Process(workbook, new ImportAgentDefaults { RoleIds = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Role), defaultEntity.DescriptionText));
			result.RawResults.Single().Agent.Roles.Single().Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidRoles()
		{
			var rawAgent = setupProviderData();
			rawAgent.Role = "invalid role";

			var defaultEntity = ApplicationRoleFactory.CreateRole("default", "default role").WithId();
			RoleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { RoleIds = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Role), defaultEntity.DescriptionText));
			result.RawResults.Single().Agent.Roles.Single().Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyTeam()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = TeamFactory.CreateSimpleTeam("default").WithId();
			var site = SiteFactory.CreateSimpleSite("site default").WithId();
			defaultEntity.Site = site;
			TeamRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(7));

			var result = Target.Process(workbook, new ImportAgentDefaults { TeamId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Organization), defaultEntity.SiteAndTeam));
			result.RawResults.Single().Agent.Team.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidTeam()
		{
			var rawAgent = setupProviderData();
			rawAgent.Organization = "invalid site/invalid team";

			var defaultEntity = TeamFactory.CreateSimpleTeam("default").WithId();
			TeamRepository.Add(defaultEntity);
			var site = SiteFactory.CreateSimpleSite("site default").WithId();
			defaultEntity.Site = site;

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { TeamId = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Organization), defaultEntity.SiteAndTeam));
			result.RawResults.Single().Agent.Team.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySkill()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = SkillFactory.CreateSkill("default").WithId();
			SkillRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(8));

			var result = Target.Process(workbook, new ImportAgentDefaults { SkillIds = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Skill), defaultEntity.Name));
			result.RawResults.Single().Agent.Skills.Single().Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidSkill()
		{
			var rawAgent = setupProviderData();
			rawAgent.Skill = "invalid skill";

			var defaultEntity = SkillFactory.CreateSkill("default").WithId();
			SkillRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook, new ImportAgentDefaults { SkillIds = defaultValue });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.Skill), defaultEntity.Name));
			result.RawResults.Single().Agent.Skills.Single().Should().Be.EqualTo(defaultEntity);
		}


		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyStartDate()
		{
			var rawAgent = setupProviderData();

			var defaultValue = DateOnly.Today;

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(6));

			var result = Target.Process(workbook, new ImportAgentDefaults { StartDate = defaultValue.ToShortDateString() });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.StartDate), defaultValue.ToShortDateString()));
			result.RawResults.Single().Agent.StartDate.Should().Be.EqualTo(defaultValue);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySchedulePeriodLength()
		{
			var rawAgent = setupProviderData();

			var defaultValue = 4;

			var ms = _agentFileTemplate.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(15));

			var result = Target.Process(workbook, new ImportAgentDefaults { SchedulePeriodLength = "4" });

			result.RawResults.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.RawResults.Single().Feedback.WarningMessages.Single().Should().Be(getWarningMessage(nameof(RawAgent.SchedulePeriodLength), defaultValue));
			result.RawResults.Single().Agent.SchedulePeriodLength.Should().Be.EqualTo(defaultValue);
		}

		[Test]
		public void ShouldSkipEmptyRows()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row1 = sheet.CreateRow(2);
			row1.CreateCell(0).SetCellValue(string.Empty);
			row1.CreateCell(1).SetCellValue(string.Empty);
			sheet.CreateRow(3);
			var result = Target.Process(workbook);
			result.HasError.Should().Be.True();
			result.Feedback.ErrorMessages.Single().Should().Be(Resources.NoDataAvailable);
		}


		[Test]
		public void ShouldWriteErrorMsgForInvalidInput()
		{
			var ms = _agentFileTemplate.GetFileTemplate(new RawAgent { Firstname = "test", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Agent.Should().Be.Null();
			result.RawResults.First().Feedback.ErrorMessages.Count.Should().Be(10);
		}


		[Test]
		public void ShouldSpotUserWithEmptyFirstnameAndLastname()
		{
			var ms = _agentFileTemplate.GetFileTemplate(new RawAgent { Firstname = "", Lastname = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook);

			var errorMsg = result.RawResults.Single().Feedback.ErrorMessages;
			errorMsg.Contains(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldSpotUserWithEmptyApplicationLogonIdAndWindowsUser()
		{
			var ms = _agentFileTemplate.GetFileTemplate(new RawAgent { ApplicationUserId = "", WindowsUser = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.Process(workbook);

			var errorMsg = result.RawResults.Single().Feedback.ErrorMessages;
			errorMsg.Contains(Resources.NoLogonAccountErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldNotGiveErrorIfExternalLogonIsEmpty()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0);
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue(new DateTime(2017, 1, 1));
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9);

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue("100%");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);

			var result = Target.Process(workbook);

			result.RawResults.Count.Should().Be.EqualTo(1);
			result.RawResults.First().Feedback.ErrorMessages.Count(m => m.Contains("ExternalLogOn")).Should().Be(0);
		}

		[Test]
		public void ShouldValidateMultipleExternalLogon()
		{
			var workbook = _agentFileTemplate.GetTemplateWorkbook("testAgent");
			var row = workbook.GetSheetAt(0).CreateRow(1);
			row.CreateCell(0);
			row.CreateCell(1).SetCellValue("andeen");

			row.CreateCell(2).SetCellValue("");
			row.CreateCell(3).SetCellValue("aa");
			row.CreateCell(4).SetCellValue("aa");
			row.CreateCell(5).SetCellValue("agent");
			row.CreateCell(6).SetCellValue(new DateTime(2017, 1, 1));
			row.CreateCell(7).SetCellValue("london");
			row.CreateCell(8).SetCellValue("test");
			row.CreateCell(9).SetCellValue("0019,0018");

			row.CreateCell(10).SetCellValue("fix");
			row.CreateCell(11).SetCellValue("fix");
			row.CreateCell(12).SetCellValue("100%");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);

			var result = Target.Process(workbook);
			var externalLogonProName = _agentFileTemplate.GetColumnDisplayName(nameof(RawAgent.ExternalLogon));
			result.RawResults.Count.Should().Be.EqualTo(1);
			var errForExtLogon =
				result.RawResults.First().Feedback.ErrorMessages.Single(r => r.Contains(externalLogonProName));
			errForExtLogon.Should()
				.Be.EqualTo(string.Format(Resources.InvalidColumn, externalLogonProName, "0019, 0018"));
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

		private string getMissingColumnsErrorMsg(params string[] excepts)
		{
			var colsMsg = string.Join(", ", _agentFileTemplate.ColumnHeaderNames.Except(excepts.Select(ex => _agentFileTemplate.GetColumnDisplayName(ex))));
			return string.Format(Resources.MissingColumnX, colsMsg);
		}


		private string getWarningMessage(string column, object value)
		{
			return string.Format(Resources.ImportAgentsColumnFixedWithFallbackValue, _agentFileTemplate.GetColumnDisplayName(column), value);
		}
	}
}
