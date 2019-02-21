using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class DeletePersonInfoTest
	{
		private TenantUnitOfWorkManager _uow;

		[Test]
		public void ShouldDeletePerson()
		{
			var session = _uow.CurrentSession();
			var tenant = new Tenant("som name");
			var person = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(tenant);
			session.Save(person);

			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenant);

			var target = new DeletePersonInfo(_uow, currentTenant);
			target.Delete(person.Id);

			session.Get<PersonInfo>(person.Id)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldNotDeletePersonInOtherTenant()
		{
			var session = _uow.CurrentSession();
			var tenant = new Tenant("some name");
			var person = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(tenant);
			session.Save(person);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(new Tenant("some other"));

			var target = new DeletePersonInfo(_uow, currentTenant);
			target.Delete(person.Id);

			session.Get<PersonInfo>(person.Id)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDoNothingIfPersonInfoDoesntExist()
		{
			var target = new DeletePersonInfo(_uow, new CurrentTenantFake());
			Assert.DoesNotThrow(() =>
				target.Delete(Guid.NewGuid()));
		}


		[SetUp]
		public void Setup()
		{
			_uow = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
			_uow.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void TearDown()
		{
			_uow.Dispose();
		}
	}
}