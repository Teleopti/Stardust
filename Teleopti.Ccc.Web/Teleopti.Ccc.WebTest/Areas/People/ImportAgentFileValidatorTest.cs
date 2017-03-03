using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class ImportAgentFileValidatorTest
	{
		private FakeApplicationRoleRepository _applicationRoleRepository;
		private FakeContractRepository _contractRepository;
		private FakeContractScheduleRepository _contractScheduleRepository;
		private FakePartTimePercentageRepository _partTimePercentageRepository;
		private FakeRuleSetBagRepository _ruleSetBagRepository;
		private FakeExternalLogOnRepository _externalLogonRepo;
		private FakeSkillRepository _skillRepository;
		private FakeSiteRepository _siteRepository;
		private FakeTeamRepository _teamRepository;
		private ImportAgentDataProvider _provider;
		private AgentFileTemplate _template;

		private RawAgent rawAgent;

		private ImportAgentFileValidator target;

		[SetUp]
		public void SetUp()
		{
			_applicationRoleRepository = new FakeApplicationRoleRepository();
			_contractRepository = new FakeContractRepository();
			_contractScheduleRepository = new FakeContractScheduleRepository();
			_partTimePercentageRepository = new FakePartTimePercentageRepository();
			_ruleSetBagRepository = new FakeRuleSetBagRepository();
			_skillRepository = new FakeSkillRepository();
			_siteRepository = new FakeSiteRepository();
			_teamRepository = new FakeTeamRepository();
			_externalLogonRepo = new FakeExternalLogOnRepository();
			

			_provider = new ImportAgentDataProvider(_applicationRoleRepository,_contractRepository,_contractScheduleRepository, 
				_partTimePercentageRepository, _ruleSetBagRepository, _skillRepository, _siteRepository, _teamRepository, _externalLogonRepo, null);

			_template = new AgentFileTemplate();

			target = new ImportAgentFileValidator(_provider);
		}


		[Test]
		public void ShouldValidateColumnNames()
		{
			var extracted = new List<string>
			{
				"Firstname",
				"Lastname",
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
				"SchedulePeriodLength"
			};
			target.ValidateColumnNames(extracted).Should().Be.Empty();
		}

		[Test]
		public void ShouldFailIfColumnsAreNotTheSameAsExpected()
		{
			var extracted = new List<string>
			{
				"Lastname",
				"Firstname",				
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
			};
			target.ValidateColumnNames(extracted).Should().Have.SameSequenceAs(new[]
			{
				"Firstname",
				"Lastname",
				"SchedulePeriodLength"
			});
		}
	

		[Test]
		public void ShouldTolerateAdditionalColumns()
		{
			var extracted = new List<string>
			{
				"Firstname",
				"Lastname",
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
				"SchedulePeriodLength",
				"AdditionalColumn"
			};
			target.ValidateColumnNames(extracted).Should().Be.Empty();
		}

		[Test]
		public void ShouldValidateTemplateFileHeader()
		{
			var templ = _template.GetFileTemplateWithDemoData();
			var workbook = new HSSFWorkbook(templ);
			var header = target.ExtractColumnNames(workbook);
			target.ValidateColumnNames(header).Should().Be.Empty();
		}

		[Test]
		public void ShouldValidateTemplateFileValuesWithValidInput()
		{
			setupProviderData();
			var templ = _template.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(templ);
			var values = target.ExtractAgentInfoValues(workbook);
			var extractedAgent = values.Single().Agent;
			extractedAgent.Firstname.Should().Be.EqualTo(rawAgent.Firstname);
			extractedAgent.StartDate.Should().Be.EqualTo(new DateOnly(rawAgent.StartDate.Value));
			extractedAgent.Contract.Description.Name.Should().Be.EqualTo(rawAgent.Contract);
		}

		[Test]
		public void ShouldValidateTemplateFileValuesWithInvalidInput()
		{
			setupProviderData();
			rawAgent.Contract = "not exist contract";

			var templ = _template.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(templ);
			var values = target.ExtractAgentInfoValues(workbook);
			var extractedAgent = values.Single().Agent;
			extractedAgent.Should().Be.Null();
			var errorMsg = values.Single().ErrorMessages;
			errorMsg.Count.Should().Be.EqualTo(1);
			errorMsg.Single().Should().Be.EqualTo(string.Format(Resources.InvalidColumn, Resources.Contract, rawAgent.Contract));
		}


		[Test]
		public void ShouldSpotUserWithEmptyFirstnameAndLastname()
		{
			setupProviderData();
			rawAgent.Firstname = "";
			rawAgent.Lastname = "";

			var templ = _template.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(templ);
			var values = target.ExtractAgentInfoValues(workbook);
			var extractedAgent = values.Single().Agent;
			extractedAgent.Should().Be.Null();
			var errorMsg = values.Single().ErrorMessages;
			errorMsg.Count.Should().Be.EqualTo(1);
			errorMsg.Single().Should().Be.EqualTo(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon);
		}


		[Test]
		public void ShouldSpotUserWithEmptyApplicationLogonIdAndWindowsUser()
		{
			setupProviderData();
			rawAgent.ApplicationUserId = "";
			rawAgent.WindowsUser = "";

			var templ = _template.GetFileTemplate(rawAgent);
			var workbook = new HSSFWorkbook(templ);
			var values = target.ExtractAgentInfoValues(workbook);
			var extractedAgent = values.Single().Agent;
			extractedAgent.Should().Be.Null();
			var errorMsg = values.Single().ErrorMessages;
			errorMsg.Count.Should().Be.EqualTo(1);
			errorMsg.Single().Should().Be.EqualTo(Resources.NoLogonAccountErrorMsgSemicolon);
		}

		private void setupProviderData()
		{
			var role = ApplicationRoleFactory.CreateRole("agent","test");

			_applicationRoleRepository.Add(role);
			var team = TeamFactory.CreateSimpleTeam("preference");
			var site = SiteFactory.CreateSimpleSite("London");
			_siteRepository.Add(site);
			team.Site = site;
			_teamRepository.Add(team);
			var skill = SkillFactory.CreateSkill("test skill");
			_skillRepository.Add(skill);
			var externalLogon = ExternalLogOnFactory.CreateExternalLogOn();
			_externalLogonRepo.Add(externalLogon);
			var contract = ContractFactory.CreateContract("full");
			_contractRepository.Add(contract);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("test schedule");
			_contractScheduleRepository.Add(contractSchedule);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("partTime");
			_partTimePercentageRepository.Add(partTimePercentage);
			var shiftBag = new RuleSetBag(WorkShiftRuleSetFactory.Create());
			_ruleSetBagRepository.Add(shiftBag);

			
			rawAgent = new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = role.Name,
				StartDate = new DateTime(2017,3,1),
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

		[Test]
		[Ignore("For manual testing only")]
		public void TryOutputTemplateFile()
		{
			var templ = _template.GetFileTemplate(_template.GetDefaultAgent());
			using (var outputFile = File.Create("C:\\TeleoptiV7\\Logs\\templateFile.xls"))
			{
				templ.Seek(0,SeekOrigin.Begin);
				templ.CopyTo(outputFile);
			}

		}
	}
}
