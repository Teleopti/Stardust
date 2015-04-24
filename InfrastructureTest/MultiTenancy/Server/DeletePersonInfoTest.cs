using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class DeletePersonInfoTest
	{
		private TenantUnitOfWork _uow;
		private Tenant tenant;
		private PersonInfo person;

		[Test]
		public void ShouldDeletePerson()
		{
			var target = new DeletePersonInfo(_uow);
			target.Delete(person.Id);

			var session = _uow.CurrentSession();
			session.Flush();
			session.Get<PersonInfo>(person.Id)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldDoNothingIfPersonInfoDoesntExist()
		{
			var target = new DeletePersonInfo(_uow);
			Assert.DoesNotThrow(() =>
				target.Delete(Guid.NewGuid()));
		}


		[SetUp]
		public void Setup()
		{
			_uow = TenantUnitOfWork.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			var session = _uow.CurrentSession();
			tenant = new Tenant("som name");
			person = new PersonInfo(tenant, Guid.NewGuid());
			session.Save(tenant);
			session.Save(person);
			session.Flush();
		}

		[TearDown]
		public void TearDown()
		{
			_uow.Dispose();
		}
	}
}