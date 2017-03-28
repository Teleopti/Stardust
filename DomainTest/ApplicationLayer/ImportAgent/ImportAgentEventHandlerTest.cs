using System;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture, DomainTest]
	public class ImportAgentEventHandlerTest : ISetup
	{
		public ImportAgentEventHandler Target;
		public FakeJobResultRepository JobResultRepo;
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
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			system.UseTestDouble<ImportAgentEventHandler>().For<IHandleEvent<ImportAgentEvent>>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			system.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			system.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			system.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();


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
		}

		[Test]
		public void ShouldResolveHandler()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHandleEventWithNoAtifact()
		{
			var updated = fakeJobResult();

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = updated.Id.Value
			});

			updated.Details.Count().Should().Be(1);
			updated.FinishedOk.Should().Be.True();
		}

		[Test]
		public void ShouldHandleEventWhenInvalidArtifact()
		{
			var updated = fakeJobResult();
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", null));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = updated.Id.Value,
			});

			updated.Details.Count().Should().Be(1);
			updated.FinishedOk.Should().Be.True();
			updated.Details.Single().Message.Should().Be(Resources.InvalidInput);
		}

		[Test]
		public void ShouldHandleEventWhenThereAreMultipleInputArtifacts()
		{
			var updated = fakeJobResult();
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", null));
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test1.xls", null));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = updated.Id.Value,
			});

			updated.Details.Count().Should().Be(1);
			updated.FinishedOk.Should().Be.True();
			updated.Details.Single().Message.Should().Be(Resources.InvalidInput);
		}

		[Test]
		public void ShouldHandleEventWithEmptyInputArtifact()
		{
			var jobResult = fakeJobResult();
			var workbook = new HSSFWorkbook();
			workbook.CreateSheet();
			using (var stream = new MemoryStream())
			{
				workbook.Write(stream);
				jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", stream.ToArray()));
			}

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});

			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo(Resources.EmptyFile);
		}

		[Test]
		public void ShouldHandleEventWithMissingColumnInputArtifact()
		{
			var jobResult = fakeJobResult();
			var workbook = new HSSFWorkbook();
			var sheet = workbook.CreateSheet();
			sheet.CreateRow(0).CreateCell(0).SetCellValue("Firstname");
			using (var stream = new MemoryStream())
			{
				workbook.Write(stream);
				jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", stream.ToArray()));
			}

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			var headers = new AgentFileTemplate().ColumnHeaderNames;
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo(string.Format(Resources.MissingColumnX, string.Join(", ", headers.Except(new[] { "Firstname" }))));
		}

		[Test]
		public void ShouldHandleEventWhenInputArtifactHasNoAgents()
		{
			var jobResult = fakeJobResult();
			var workbook = new AgentFileTemplate().GetTemplateWorkbook("agent");
			using (var stream = new MemoryStream())
			{
				workbook.Write(stream);
				jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", stream.ToArray()));
			}

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo(Resources.NoDataAvailable);
		}

		[Test]
		public void ShouldOnlyWriteSummaryWhenAllSucceededWithValidInputArtifact()
		{
			var jobResult = fakeJobResult();
			var rawAgent = setupProviderData();
			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:1, failed count:0, warning count:0");
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputError).Count().Should().Be(0);
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Count().Should().Be(0);
		}

		[Test]
		public void ShouldWriteErrorArtifactWhenHavingFailureWithValidInputArtifact()
		{
			var jobResult = fakeJobResult();
			var rawAgent = setupProviderData();
			rawAgent.Role = "notExistRole";
			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:0, failed count:1, warning count:0");
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputError).Count().Should().Be(1);
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Count().Should().Be(0);
		}


		[Test]
		public void ShouldWriteWarningArtifactWhenHavingWarningWithValidInputArtifact()
		{
			var jobResult = fakeJobResult();
			var rawAgent = setupProviderData();
			var validPeriodType = rawAgent.SchedulePeriodType;
			rawAgent.SchedulePeriodType = null;

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value,
				Defaults = new ImportAgentDefaults
				{
					SchedulePeriodType = validPeriodType
				}
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:0, failed count:0, warning count:1");
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputError).Count().Should().Be(0);
			jobResult.Artifacts.Where(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Count().Should().Be(1);
		}


		[Test]
		public void ShouldRejectJobWhenJobHadHandled()
		{
			var jobResult = fakeJobResult();
			var rawAgent = setupProviderData();

			var ms = new AgentFileTemplate().GetFileTemplate(rawAgent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));
			
			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Version.GetValueOrDefault().Should().Be(2);
			jobResult.Details.Count().Should().Be(1);
		}

		private IJobResult fakeJobResult()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult("WebImportAgent", DateOnly.Today.ToDateOnlyPeriod(), person, DateTime.UtcNow).WithId();
			JobResultRepo.Add(jobResult);
			var updated = JobResultRepo.Get(jobResult.Id.Value);
			return updated;
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
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("partTime");
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
				SchedulePeriodType = SchedulePeriodType.Week.ToString(),
				SchedulePeriodLength = 4
			};
		}

	}
}
