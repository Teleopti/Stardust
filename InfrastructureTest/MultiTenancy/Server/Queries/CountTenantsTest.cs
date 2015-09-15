using System;
using Castle.Core.Internal;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[TestFixture]
	[DatabaseTest]
	public class CountTenantsTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistTenant Persist;
		public ICountTenants Target;
		public DeleteTenant Delete;
		public ILoadAllTenants LoadAll;

		[Test]
		public void ShouldCountTenants([Values(1, 3, 7)] int expected)
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				LoadAll.Tenants().ForEach(Delete.Delete);
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				expected.Times(() => Persist.Persist(new Tenant(RandomName.Make())));

				var actual = Target.Count();

				actual.Should().Be(expected);
			}
		}

	}
}