using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[DatabaseTest]
	public class FindExternalApplicationAccessTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public IPersistExternalApplicationAccess Persist;
		public IFindExternalApplicationAccess Target;

		[Test]
		public void ShouldFindExternalApplicationAccessByHash()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var hash = Guid.NewGuid().ToString().Replace("-","");
				var externalApplicationAccess = new ExternalApplicationAccess(Guid.NewGuid(), name, hash);
				Persist.Persist(externalApplicationAccess);

				var actual = Target.FindByTokenHash(hash);

				actual.Name.Should().Be(name);
			}
		}

		[Test]
		public void ShouldNotFindDeletedExternalApplicationAccess()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var hash = Guid.NewGuid().ToString().Replace("-", "");
				var personId = Guid.NewGuid();
				var externalApplicationAccess = new ExternalApplicationAccess(personId, name, hash);
				Persist.Persist(externalApplicationAccess);
				Persist.Remove(externalApplicationAccess.Id,personId);

				Target.FindByPerson(personId).Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldFindExternalApplicationAccessByPerson()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var hash = Guid.NewGuid().ToString().Replace("-", "");
				var personId = Guid.NewGuid();
				var externalApplicationAccess = new ExternalApplicationAccess(personId, name, hash);
				Persist.Persist(externalApplicationAccess);

				var actual = Target.FindByPerson(personId).Single();

				actual.Name.Should().Be(name);
			}
		}

		[Test]
		public void ShouldFindExternalApplicationAccessByPersonOnlyForGivenPerson()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var hash = Guid.NewGuid().ToString().Replace("-", "");
				var personId = Guid.NewGuid();
				var externalApplicationAccess = new ExternalApplicationAccess(personId, name, hash);
				Persist.Persist(externalApplicationAccess);

				Target.FindByPerson(Guid.NewGuid()).Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldReturnNullWhenNotFound()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var actual = Target.FindByTokenHash(RandomName.Make());

				actual.Should().Be.Null();
			}
		}
	}
}