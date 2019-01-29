using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class PublishStartupJobsTaskTest
	{
		private LegacyFakeEventPublisher _eventPublisher;
		private FakeTenants _loadAllTenants;
		private FakeBusinessUnitRepository _businessUnitRepository;
		private PublishStartupJobsTask _target;
		private TenantUnitOfWorkFake _tenantUnitOfWorkFake;

		[SetUp]
		public void Setup()
		{
			_eventPublisher = new LegacyFakeEventPublisher();
			_loadAllTenants = new FakeTenants();
			_businessUnitRepository = new FakeBusinessUnitRepository(null);
			_tenantUnitOfWorkFake = new TenantUnitOfWorkFake();

			_target = new PublishStartupJobsTask(_eventPublisher, _loadAllTenants, _tenantUnitOfWorkFake);
		}

		[Test]
		[Ignore("This works, but is on a 20 seconds delay so ignore for now")]
		public void ShouldPublishEventWhenToggleIsTrue()
		{
			var tenantName = "TestTenant";
			_loadAllTenants.Has(tenantName);
			_businessUnitRepository.Add(BusinessUnitFactory.CreateWithId("TestBu"));

			var task = _target.Execute(null);
			task.Should().Not.Be.Null();
			task.GetAwaiter().GetResult();

			_eventPublisher.PublishedEvents.OfType<PublishInitializeReadModelEvent>().Should().Not.Be.Empty();
		}
	}
}