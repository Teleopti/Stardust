using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	[DatabaseTest]
	public class FindTenantByNameWithEnsuredTransactionTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistTenant Persist;
		public IFindTenantByNameWithEnsuredTransaction Target;

		[Test]
		public void ShouldFindTenantByNameWithExplicitTransaction()
		{
			var name = RandomName.Make();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant(name);
				Persist.Persist(tenant);
			}

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var actual = Target.Find(name);

				actual.Name.Should().Be(name);
			}
		}

		[Test]
		public void ShouldFindTenantByNameWithImplicitTransaction()
		{
			var name = RandomName.Make();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant(name);
				Persist.Persist(tenant);
			}

			var actual = Target.Find(name);

			actual.Name.Should().Be(name);
		}

		[Test]
		public void ShouldReturnNullWhenNotFoundWithExplicitTransaction()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				ShouldReturnNullWhenNotFoundWithImplicitTransaction();
			}
		}

		[Test]
		public void ShouldReturnNullWhenNotFoundWithImplicitTransaction()
		{
			Target.Find(RandomName.Make())
				.Should().Be.Null();
		}
	}
}