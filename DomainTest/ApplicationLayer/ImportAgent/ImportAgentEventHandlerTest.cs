using System;
using System.Collections.Generic;
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

using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using System.Globalization;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture, DomainTest]
	public class ImportAgentEventHandlerTest : IIsolateSystem, IExtendSystem
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
		public IFileProcessor FileProcessor;

		public TenantUnitOfWorkFake TenantUnitOfWork;
		public FakeCurrentDatasource CurrentDatasource;
		public FakeTenants FindTenantByName;
		public PersistPersonInfoFake PersonInfoPersister;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<FakeDatabase>();
			extend.AddService<FakeStorage>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			isolate.UseTestDouble<ImportAgentEventHandler>().For<IHandleEvent<ImportAgentEvent>>();
			isolate.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			isolate.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			isolate.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<PersistPersonInfoFake>().For<IPersistPersonInfo>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();


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
			isolate.UseTestDouble<FileProcessor>().For<IFileProcessor>();
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
				JobResultId = updated.Id.Value
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
				JobResultId = updated.Id.Value
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
			sheet.CreateRow(0).CreateCell(0).SetCellValue(nameof(RawAgent.Firstname));
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
			message.Should().Be.EqualTo(string.Format(Resources.MissingColumnX, string.Join(", ", headers.Except(new[] { nameof(RawAgent.Firstname) }))));
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
				JobResultId = jobResult.Id.Value,
				LogOnDatasource = CurrentDatasource.CurrentName()
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:1, failed count:0, warning count:0");
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputError).Should().Be(0);
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Should().Be(0);
		}

		[Test]
		public void ShouldWriteErrorArtifactWhenHavingFailureWithValidInputArtifact()
		{
			var jobResult = fakeJobResult();
			List<RawAgent> agents = new List<RawAgent>();
			for (int i = 0; i < 100; i++)
			{
				var rawAgent = setupProviderData();
				rawAgent.Role = "notExistRole";
				agents.Add(rawAgent);
			}
			var ms = new AgentFileTemplate().GetFileTemplate(agents.ToArray());
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:0, failed count:100, warning count:0");

			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputError).Should().Be(1);
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Should().Be(0);

			var failedArtifact = jobResult.Artifacts.Single(ar => ar.Category == JobResultArtifactCategory.OutputError);
			var wookbook = FileProcessor.ParseFile(new ImportFileData
			{
				FileName = failedArtifact.Name,
				Data = failedArtifact.Content
			});
			wookbook.GetSheet("Agents").LastRowNum.Should().Be(100);
			wookbook.GetSheet("Agents").GetRow(1).GetCell(6).CellStyle.DataFormat.Should().Equals(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
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
				},
				LogOnDatasource = "test"
			});
			var message = jobResult.Details.Single().Message;
			message.Should().Be.EqualTo("success count:0, failed count:0, warning count:1");
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputError).Should().Be(0);
			jobResult.Artifacts.Count(ar => ar.Category == JobResultArtifactCategory.OutputWarning).Should().Be(1);
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

		[Test]
		public void ShouldJobDetailLevelBeInfoIfJobExecuteSucceed()
		{
			var jobResult = fakeJobResult();
			var ms = new AgentFileTemplate().GetFileTemplate(setupProviderData());
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});

			jobResult.Details.Single().DetailLevel.Should().Be.EqualTo(DetailLevel.Info);

		}

		[Test]
		public void ShouldJobDetailLevelBeErrorIfInputAgentsHasErrorMessages()
		{
			var jobResult = fakeJobResult();
			var agent = setupProviderData();
			agent.Role = "Invalid Role";
			var ms = new AgentFileTemplate().GetFileTemplate(agent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));

			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value
			});
			jobResult.Details.Single().DetailLevel.Should().Be.EqualTo(DetailLevel.Error);

		}

		[Test]
		public void ShouldJobDetailLevelBeWarningIfFixedAgentsWithDefaults()
		{
			var jobResult = fakeJobResult();
			var agent = setupProviderData();
			agent.Role = "";
			var ms = new AgentFileTemplate().GetFileTemplate(agent);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", ms.ToArray()));
			var defaultRole = ApplicationRoleFactory.CreateRole("default", "role description").WithId();
			RoleRepository.Add(defaultRole);
			Target.Handle(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.Value,
				Defaults = new ImportAgentDefaults
				{
					RoleIds = defaultRole.Id.ToString()
				}
			});
			jobResult.Details.Single().DetailLevel.Should().Be.EqualTo(DetailLevel.Warning);

		}

		private IJobResult fakeJobResult()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult("WebImportAgent", DateOnly.Today.ToDateOnlyPeriod(), person, DateTime.UtcNow).WithId();
			jobResult.SetVersion(1);
			JobResultRepo.Add(jobResult);
			var updated = JobResultRepo.Get(jobResult.Id.Value);
			return updated;
		}

		private RawAgent setupProviderData()
		{
			CurrentDatasource.FakeName("test");
			FindTenantByName.Has(new Tenant("test"));
			var role = ApplicationRoleFactory.CreateRole("agent", "role description").WithId();
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
