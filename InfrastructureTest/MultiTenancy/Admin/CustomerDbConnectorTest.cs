using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class CustomerDbConnectorTest : DatabaseTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		protected override void SetupForRepositoryTest()
		{
			base.SetupForRepositoryTest();
			
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}
		
		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldReturnEmailAddressFromCustomerDatabase()
		{
			var personToTest = PersonFactory.CreatePerson("Kalle", "Anka");
			personToTest.Email = "kalle.anka@ankeborg.com";
			PersistAndRemoveFromUnitOfWork(personToTest);

			var tenantSession = tenantUnitOfWorkManager.CurrentSession();
			var tenant = tenantSession.Query<Tenant>().First();
			var pi = new PersonInfo(tenant, personToTest.Id.GetValueOrDefault());
			pi.ApplicationLogonInfo.SetLogonName("kallea");
			tenantSession.SaveOrUpdate(pi);
			tenantSession.Flush();

			var target = new CustomerDbConnector();
			target.TryGetEmailAddress(pi, out var userEmail);
			userEmail.Should().Be.EqualTo(personToTest.Email);
		}
	}
}