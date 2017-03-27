using System;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
	public class ImportAgentEventHandlerTest : ISetup
	{
		public ImportAgentEventHandler Target;
		public FakeJobResultRepository JobResultRepo;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ImportAgentEventHandler>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			system.UseTestDouble<PasswordPolicyFake>().For<IPasswordPolicy>();
			system.UseTestDouble<FindLogonInfoFake>().For<IFindLogonInfo>();
			system.UseTestDouble<FindPersonInfoFake>().For<IFindPersonInfo>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
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
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls",null));

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
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls",null));
			updated.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, "test1.xls",null));

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
			message.Should().Be.EqualTo(string.Format(Resources.MissingColumnX, string.Join(", ",headers.Except(new[]{"Firstname"}))));
		}

		private IJobResult fakeJobResult()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult("WebImportAgent", DateOnly.Today.ToDateOnlyPeriod(), person, DateTime.UtcNow).WithId();
			JobResultRepo.Add(jobResult);
			var updated = JobResultRepo.Get(jobResult.Id.Value);
			return updated;
		}
	}
}
