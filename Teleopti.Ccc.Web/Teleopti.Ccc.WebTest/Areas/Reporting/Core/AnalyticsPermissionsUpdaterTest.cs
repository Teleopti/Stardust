using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsPermissionsUpdaterTest : ISetup
	{
		public IAnalyticsPermissionsUpdater Target;
		public IAnalyticsPermissionRepository AnalyticsPermissionRepository;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeAnalyticsPermissionExecutionRepository AnalyticsPermissionExecutionRepository;
		public FakePersonRepository PersonRepository;
		public FakeAnalyticsTeamRepository AnalyticsTeamRepository;
		public IPermissionsConverter PermissionsConverter;
		public MutableNow Now;
		private readonly Guid businessUnitId = Guid.NewGuid();
		private readonly Guid personId = Guid.NewGuid();
		private readonly Guid teamId = Guid.NewGuid();
		private readonly Guid reportId = Guid.NewGuid();
		private const int analyticsBusinessUnitId = 1;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsPermissionsUpdater>();
			system.AddService<PermissionsConverter>();
		}

		[Test]
		public void ShouldNotUpdatePermissionsIfRecentlyUpdated()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			PersonRepository.Has(PersonFactory.CreatePerson("_").WithId(personId));
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId,BusinessUnitCode = businessUnitId});
			Now.Is(DateTime.UtcNow);
			AnalyticsPermissionExecutionRepository.Has(Now);
			AnalyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);

			Target.Handle(personId, businessUnitId);

			AnalyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenNoPermissions()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			PersonRepository.Has(PersonFactory.CreatePerson("_").WithId(personId));
			Now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			AnalyticsPermissionExecutionRepository.Has(Now);
			AnalyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });

			Target.Handle(personId, businessUnitId);

			AnalyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldRemovePermissionsThatAreNotInApplicationDatabase()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			PersonRepository.Has(PersonFactory.CreatePerson("_").WithId(personId));
			Now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			AnalyticsPermissionExecutionRepository.Has(Now);
			AnalyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });

			var toBeRemoved = new AnalyticsPermission {BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = reportId, TeamId = 123};
			AnalyticsPermissionRepository.InsertPermissions(new[] { toBeRemoved });

			Target.Handle(personId, businessUnitId);

			AnalyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).Should().Be.Empty();
		}

		[Test]
		public void ShouldAddPermissionsThatAreInApplicationDatabase()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			var person = PersonFactory.CreatePerson("_").WithId(personId);
			var applicationRole = new ApplicationRole();
			var analyticTeam = new AnalyticTeam { TeamCode = teamId, TeamId = 123 };
			var team = new Team().WithId(teamId);

			applicationRole.AddApplicationFunction(new ApplicationFunction { ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix });
			applicationRole.AvailableData = new AvailableData
			{
				AvailableDataRange = AvailableDataRangeOption.None
			};
			applicationRole.AvailableData.AddAvailableTeam(team);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			AnalyticsTeamRepository.Has(analyticTeam);

			PersonRepository.Has(person);
			Now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			AnalyticsPermissionExecutionRepository.Has(Now);
			AnalyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			
			Target.Handle(personId, businessUnitId);

			AnalyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId)
				.First()
				.Should()
				.Be.EqualTo(new AnalyticsPermission
				{
					BusinessUnitId = analyticsBusinessUnitId,
					MyOwn = false,
					PersonCode = personId,
					ReportId = reportId,
					TeamId = analyticTeam.TeamId
				});
		}

		[Test]
		public void ShouldUpdateWithEmptySetWhenExistsInBoth()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			var person = PersonFactory.CreatePerson("_").WithId(personId);
			var applicationRole = new ApplicationRole();
			var analyticTeam = new AnalyticTeam { TeamCode = teamId, TeamId = 123 };
			var team = new Team().WithId(teamId);

			applicationRole.AddApplicationFunction(new ApplicationFunction { ForeignId = reportId.ToString(), ForeignSource = DefinedForeignSourceNames.SourceMatrix });
			applicationRole.AvailableData = new AvailableData
			{
				AvailableDataRange = AvailableDataRangeOption.None
			};
			applicationRole.AvailableData.AddAvailableTeam(team);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			AnalyticsTeamRepository.Has(analyticTeam);
			PersonRepository.Has(person);
			Now.Is(DateTime.UtcNow - TimeSpan.FromMinutes(16));
			AnalyticsPermissionExecutionRepository.Has(Now);
			AnalyticsPermissionExecutionRepository.Set(personId, analyticsBusinessUnitId);
			AnalyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit { BusinessUnitId = analyticsBusinessUnitId, BusinessUnitCode = businessUnitId });
			var existsInBoth = new AnalyticsPermission { BusinessUnitId = analyticsBusinessUnitId, DatasourceId = 1, DatasourceUpdateDate = DateTime.UtcNow, MyOwn = false, PersonCode = personId, ReportId = reportId, TeamId = analyticTeam.TeamId };

			AnalyticsPermissionRepository.InsertPermissions(new[] { existsInBoth });

			Target.Handle(personId, businessUnitId);

			AnalyticsPermissionRepository.GetPermissionsForPerson(personId, analyticsBusinessUnitId).First().Should().Be.EqualTo(existsInBoth);
		}
	}
}