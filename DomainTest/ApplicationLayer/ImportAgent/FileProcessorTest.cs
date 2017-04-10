using System;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture, DomainTest]
	public class FileProcessorTest : ISetup
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
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
		}

		private static string warningMessage(string column, object value)
		{
			return string.Format(Resources.ImportAgentsColumnFixedWithFallbackValue, column, value);
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


			var result = Target.ValidateSheetColumnHeader(workbook);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldValidateExcelFileWithCorrectColumnHeaderAndRedundantColumns()
		{
			var fileTemplate = new AgentFileTemplate();
			var workbook = fileTemplate.GetTemplateWorkbook("test");
			workbook.GetSheetAt(0).GetRow(0).CreateCell(fileTemplate.ColumnHeaderNames.Length).SetCellValue("redundant");

			var result = Target.ValidateSheetColumnHeader(workbook);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMissingHeaderForInvalidFile()
		{
			HSSFWorkbook hssfwb = new HSSFWorkbook();

			var result = Target.ValidateSheetColumnHeader(hssfwb);

			result.Should().Be.EqualTo(getMissingColumnsErrorMsg());
		}

		[Test]
		public void ShouldReturnMissingHeaderForInvalidFileWithNontextHeader()
		{
			HSSFWorkbook hssfwb = new HSSFWorkbook();
			var sheet = hssfwb.CreateSheet("test");
			var row = sheet.CreateRow(0);
			row.CreateCell(0).SetCellValue(1234);


			var result = Target.ValidateSheetColumnHeader(hssfwb);

			result.Should().Be.EqualTo(getMissingColumnsErrorMsg());
		}

		[Test]
		public void ShouldParseRawDataWithCorrectColumnInput()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { Firstname = "test", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Raw.Firstname.Should().Be("test");
			result.First().Raw.StartDate.Should().Be(new DateTime(2017, 3, 1));
		}

		[Test]
		public void ShouldParseRawValueIfExternalLogonColumnTypeNotMatch()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, "ExternalLogon", string.Format(Resources.RequireXCellFormat, "text"))).Should().Be.True();
		}

		[Test]
		public void ShouldParseRawValueIfStartDateColumnTypeNotMatch()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
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
			row.CreateCell(12).SetCellValue("partime");
			row.CreateCell(13).SetCellValue("early");
			row.CreateCell(14).SetCellValue("week");
			row.CreateCell(15).SetCellValue(4);

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, "StartDate", string.Format(Resources.RequireXCellFormat, "date"))).Should().Be.True();
		}

		[Test]
		public void ShouldParseRawValueIfPartimePercentageColumnTypeNotMatch()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, "StartDate", string.Format(Resources.RequireXCellFormat, "date"))).Should().Be.True();
		}

		[Test]
		public void ShouldGiveCorrectErrorIfRequiredColumnCellIsEmpty()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Contains(string.Format(Resources.ColumnCanNotBeEmpty, "SchedulePeriodLength")).Should().Be.True();
		}

		[Test]
		public void ShouldNotGiveErrorIfNullableColumnCellIsEmpty()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Contains(string.Format(Resources.InvalidColumn, "Firstname", string.Format(Resources.RequireXCellFormat, "text"))).Should().Be.False();
		}

		[Test]
		public void ShouldSkipEmptyRows()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			sheet.CreateRow(1);
			sheet.CreateRow(2);

			var result = Target.GetNumberOfRecordsInSheet(sheet);
			result.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldWriteErrorMsgForInvalidInput()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { Firstname = "test", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Agent.Should().Be.Null();
			result.First().Feedback.ErrorMessages.Count.Should().Be(10);
		}


		[Test]
		public void ShouldSpotUserWithEmptyFirstnameAndLastname()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { Firstname = "", Lastname = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc);

			var errorMsg = result.Single().Feedback.ErrorMessages;
			errorMsg.Contains(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldSpotUserWithEmptyApplicationLogonIdAndWindowsUser()
		{
			var ms = new AgentFileTemplate().GetFileTemplate(new RawAgent { ApplicationUserId = "", WindowsUser = "", StartDate = new DateTime(2017, 3, 1) });
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc);

			var errorMsg = result.Single().Feedback.ErrorMessages;
			errorMsg.Contains(Resources.NoLogonAccountErrorMsgSemicolon).Should().Be.True();
		}

		[Test]
		public void ShouldNotGiveErrorIfExternalLogonIsEmpty()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);

			result.Count.Should().Be.EqualTo(1);
			result.First().Feedback.ErrorMessages.Count(m => m.Contains("ExternalLogOn")).Should().Be(0);
		}

		[Test]
		public void ShouldValidateMultipleExternalLogon()
		{
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("testAgent");
			var sheet = workbook.GetSheetAt(0);
			var row = sheet.CreateRow(1);
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

			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);
			var externalLogonProName = nameof(RawAgent.ExternalLogon);
			result.Count.Should().Be.EqualTo(1);
			var errForExtLogon =
				result.First().Feedback.ErrorMessages.Where(r => r.Contains(externalLogonProName)).Single();
			errForExtLogon.Should()
				.Be.EqualTo(string.Format(Resources.InvalidColumn, externalLogonProName, "0019,0018"));
		}

		[Test]
		public void ShouldPersistAgentWithValidData()
		{
			var rawAgent = setupProviderData();
			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc);

			result.Count.Should().Be(0);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySchedulePeriodType()
		{
			var rawAgent = setupProviderData();
			rawAgent.SchedulePeriodType = null;

			var defaultSchedulePeriodType = "Week";

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var sheet = workbook.GetSheetAt(0);
			sheet.GetRow(1).RemoveCell(sheet.GetRow(1).GetCell(14));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { SchedulePeriodType = defaultSchedulePeriodType });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("SchedulePeriodType", defaultSchedulePeriodType));
			result.Single().Agent.SchedulePeriodType.Should().Be.EqualTo(SchedulePeriodType.Week);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixInvalidSchedulePeriodType()
		{
			var rawAgent = setupProviderData();
			rawAgent.SchedulePeriodType = "Invalid schedule period type";

			var defaultSchedulePeriodType = "Week";

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { SchedulePeriodType = defaultSchedulePeriodType });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("SchedulePeriodType", defaultSchedulePeriodType));
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ExternalLogonId = defaultExternalLogon });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ExternalLogon", externalLogon.AcdLogOnName));
			result.Single().Agent.ExternalLogons.Single().Should().Be.EqualTo(externalLogon);
		}

		[Test]
		public void WithEmptyDefaultsProvidedShouldBeAbleToFixInvalidExternalLogon()
		{
			var rawAgent = setupProviderData();
			rawAgent.ExternalLogon = "Invalid external logon";

			var externalLogon = ExternalLogOnFactory.CreateExternalLogOn().WithId();
			ExternalLogOnRepository.Add(externalLogon);

			var defaultExternalLogon = Guid.Empty.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ExternalLogonId = defaultExternalLogon });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ExternalLogon", ""));
			result.Single().Agent.ExternalLogons.Count.Should().Be(0);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ShiftBagId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ShiftBag", ruleSetBag.Description));
			result.Single().Agent.RuleSetBag.Should().Be.EqualTo(ruleSetBag);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyRuleSetBag()
		{
			var rawAgent = setupProviderData();

			var ruleSetBag = new RuleSetBag().WithId();
			RuleSetBagRepository.Add(ruleSetBag);

			var defaultValue = ruleSetBag.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(13));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ShiftBagId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ShiftBag", ruleSetBag.Description));
			result.Single().Agent.RuleSetBag.Should().Be.EqualTo(ruleSetBag);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyPartTimePercentage()
		{
			var rawAgent = setupProviderData();
			var defaultEntity = PartTimePercentageFactory.CreatePartTimePercentage("default").WithId();
			PartTimePercentageRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(12));
			row.CreateCell(12);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { PartTimePercentageId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("PartTimePercentage", defaultEntity.Description.Name));
			result.Single().Agent.PartTimePercentage.Should().Be.EqualTo(defaultEntity);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { PartTimePercentageId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("PartTimePercentage", defaultEntity.Description.Name));
			result.Single().Agent.PartTimePercentage.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyContractSchedule()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = ContractScheduleFactory.CreateContractSchedule("default").WithId();
			ContractScheduleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(11));
			row.CreateCell(11);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ContractScheduleId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ContractSchedule", defaultEntity.Description));
			result.Single().Agent.ContractSchedule.Should().Be.EqualTo(defaultEntity);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ContractScheduleId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("ContractSchedule", defaultEntity.Description));
			result.Single().Agent.ContractSchedule.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyContract()
		{
			var rawAgent = setupProviderData();
			rawAgent.Contract = "Invalid contract";

			var defaultEntity = ContractFactory.CreateContract("default").WithId();
			ContractRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(10));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ContractId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Contract), defaultEntity.Description.Name));
			result.Single().Agent.Contract.Should().Be.EqualTo(defaultEntity);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { ContractId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage("Contract", defaultEntity.Description));
			result.Single().Agent.Contract.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyRole()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = ApplicationRoleFactory.CreateRole("default", "default role").WithId();
			RoleRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(5));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { RoleIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Role), defaultEntity.Name));
			result.Single().Agent.Roles.Single().Should().Be.EqualTo(defaultEntity);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { RoleIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Role), defaultEntity.Name));
			result.Single().Agent.Roles.Single().Should().Be.EqualTo(defaultEntity);
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

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(7));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { TeamId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Organization), defaultEntity.SiteAndTeam));
			result.Single().Agent.Team.Should().Be.EqualTo(defaultEntity);
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

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { TeamId = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Organization), defaultEntity.SiteAndTeam));
			result.Single().Agent.Team.Should().Be.EqualTo(defaultEntity);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySkill()
		{
			var rawAgent = setupProviderData();

			var defaultEntity = SkillFactory.CreateSkill("default").WithId();
			SkillRepository.Add(defaultEntity);

			var defaultValue = defaultEntity.Id.Value.ToString();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(8));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { SkillIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Skill), defaultEntity.Name));
			result.Single().Agent.Skills.Single().Should().Be.EqualTo(defaultEntity);
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

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { SkillIds = defaultValue });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.Skill), defaultEntity.Name));
			result.Single().Agent.Skills.Single().Should().Be.EqualTo(defaultEntity);
		}


		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptyStartDate()
		{
			var rawAgent = setupProviderData();

			var defaultValue = DateOnly.Today;

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(6));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { StartDate = defaultValue.ToShortDateString() });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.StartDate), defaultValue.ToShortDateString()));
			result.Single().Agent.StartDate.Should().Be.EqualTo(defaultValue);
		}

		[Test]
		public void WithDefaultsProvidedShouldBeAbleToFixEmptySchedulePeriodLength()
		{
			var rawAgent = setupProviderData();

			var defaultValue = 4;

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(ms);
			var row = workbook.GetSheetAt(0).GetRow(1);
			workbook.GetSheetAt(0).GetRow(1).RemoveCell(row.GetCell(15));

			var result = Target.ProcessSheet(workbook.GetSheetAt(0), TimeZoneInfo.Utc, new ImportAgentDefaults { SchedulePeriodLength = "4" });

			result.Single().Feedback.ErrorMessages.Should().Be.Empty();
			result.Single().Feedback.WarningMessages.Single().Should().Be(warningMessage(nameof(RawAgent.SchedulePeriodLength), defaultValue));
			result.Single().Agent.SchedulePeriodLength.Should().Be.EqualTo(defaultValue);
		}

		[Test]
		public void ShouldProcessSheetResultWithoutBlankRows()
		{
			var workbook = new HSSFWorkbook();
			var sheet = workbook.CreateSheet("Test sheet");
			for (int i = 0; i < 4; i++)
			{
				var blankRow = sheet.CreateRow(i);
				for (int j = 0; j < 4; j++)
				{
					blankRow.CreateCell(j);
				}
			}
			var row = sheet.CreateRow(4);
			for (int j = 0; j < 4; j++)
			{
				row.CreateCell(j).SetCellValue(j);
			}
			var result = Target.ProcessSheet(sheet,TimeZoneInfo.Utc);
			Assert.AreEqual(result.Count, 1);
		}

		private RawAgent setupProviderData()
		{
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

			var colsMsg = string.Join(", ", new AgentFileTemplate().ColumnHeaderNames.Except(excepts));
			return string.Format(Resources.MissingColumnX, colsMsg);

		}
	}
}
