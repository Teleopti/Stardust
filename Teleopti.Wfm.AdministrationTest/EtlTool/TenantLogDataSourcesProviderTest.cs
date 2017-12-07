using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.EtlTool;
using Teleopti.Wfm.AdministrationTest.FakeData;

namespace Teleopti.Wfm.AdministrationTest.EtlTool
{
	[AdministrationTest]
	public class TenantLogDataSourcesProviderTest :ISetup
	{
		public TenantLogDataSourcesProvider Target;
		public FakeTenants AllTenants;
		public FakeGeneralInfrastructure GeneralInfrastructure;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeGeneralInfrastructure>().For<IGeneralInfrastructure>();
		}

		[Test]
		public void ShouldReturnTenantLogDataSources()
		{
			const string connectionString = "Server=.;DataBase=a";
			AllTenants.HasWithAnalyticsConnectionsTring("Tenant", connectionString);
			GeneralInfrastructure.HasDataSources(new DataSourceEtl(3, "myDs", 1, "UTC", 15, false));

			var result = Target.Load("Tenant");
			result.Count.Should().Be(1);
			result.First().Id.Should().Be(3);
			result.First().Name.Should().Be("myDs");
		}
	}
}
