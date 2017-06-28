using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon.Toggle;
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
		private FakeToggleManager _toggleManager;
		private LegacyFakeEventPublisher _eventPublisher;
		private FakeTenants _loadAllTenants;
		private FakeBusinessUnitRepository _businessUnitRepository;
		private PublishStartupJobsTask _target;
		private TenantUnitOfWorkFake _tenantUnitOfWorkFake;

		[SetUp]
		public void Setup()
		{
			_toggleManager = new FakeToggleManager();
			
			_eventPublisher = new LegacyFakeEventPublisher();
			_loadAllTenants = new FakeTenants();
			_businessUnitRepository = new FakeBusinessUnitRepository();
			_tenantUnitOfWorkFake = new TenantUnitOfWorkFake();

			_target = new PublishStartupJobsTask(_toggleManager, _eventPublisher, _loadAllTenants, _tenantUnitOfWorkFake);
		}

		[Test]
		public void ShouldNotPublishEventsWhenToggleIsFalse()
		{
			_toggleManager.Disable(Toggles.LastHandlers_ToHangfire_41203);

			var task = _target.Execute(null);
			task.Should().Be.Null();

			_eventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		[Ignore("This works, but is on a 20 seconds delay so ignore for now")]
		public void ShouldPublishEventWhenToggleIsTrue()
		{
			var tenantName = "TestTenant";
			_toggleManager.Enable(Toggles.LastHandlers_ToHangfire_41203);
			_loadAllTenants.Has(tenantName);
			_businessUnitRepository.Add(BusinessUnitFactory.CreateWithId("TestBu"));

			var task = _target.Execute(null);
			task.Should().Not.Be.Null();
			task.Wait();

			_eventPublisher.PublishedEvents.OfType<PublishInitializeReadModelEvent>().Should().Not.Be.Empty();
		}
	}
}