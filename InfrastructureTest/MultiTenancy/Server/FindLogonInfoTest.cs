using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class FindLogonInfoTest
	{
		private IFindLogonInfo target;
		private Tenant tenantPresentInDatabase;
		private TenantUnitOfWork tenantUnitOfWork;

		[Test]
		public void ShouldGetLogonInfo()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make());
			info.SetIdentity(RandomName.Make());
			tenantUnitOfWork.CurrentSession().Save(info);

			var result = target.GetForIds(new[] {info.Id}).Single();
			result.LogonName.Should().Be.EqualTo(info.ApplicationLogonName);
			result.Identity.Should().Be.EqualTo(info.Identity);
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[Test]
		public void ShouldSilentlyIgnoreNonExistingIds()
		{
			target.GetForIds(new[] {Guid.NewGuid(), Guid.NewGuid()})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnNullForNonExistingColumns()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			tenantUnitOfWork.CurrentSession().Save(info);

			var result = target.GetForIds(new[] { info.Id }).Single();
			result.LogonName.Should().Be.Null();
			result.Identity.Should().Be.Null();
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[SetUp]
		public void InsertPreState()
		{
			tenantUnitOfWork = TenantUnitOfWork.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new FindLogonInfo(tenantUnitOfWork);

			tenantPresentInDatabase = new Tenant(RandomName.Make());
			tenantUnitOfWork.CurrentSession().Save(tenantPresentInDatabase);
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWork.Dispose();
		}
	}
}