using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[DatabaseTest]
	public class FindExternalApplicationAccessByHashTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistExternalApplicationAccess Persist;
		public IFindExternalApplicationAccessByHash Target;

		[Test]
		public void ShouldFindExternalApplicationAccessByHash()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var hash = Guid.NewGuid().ToString().Replace("-","");
				var externalApplicationAccess = new ExternalApplicationAccess(Guid.NewGuid(), name, hash);
				Persist.Persist(externalApplicationAccess);

				var actual = Target.Find(hash);

				actual.Name.Should().Be(name);
			}
		}

		[Test]
		public void ShouldReturnNullWhenNotFound()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var actual = Target.Find(RandomName.Make());

				actual.Should().Be.Null();
			}
		}
	}
}