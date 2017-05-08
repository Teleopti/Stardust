using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
		private TenantUnitOfWorkFake _tenantUnitOfWorkFake;
		private IDataSourceScope _dataSourceScope;
		private FakeBusinessUnitRepository _businessUnitRepository;
		private PublishStartupJobsTask _target;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_toggleManager = new FakeToggleManager();
			
			_eventPublisher = new LegacyFakeEventPublisher();
			_loadAllTenants = new FakeTenants();
			_tenantUnitOfWorkFake = new TenantUnitOfWorkFake();
			_dataSourceScope = MockRepository.GenerateMock<IDataSourceScope>();
			_businessUnitRepository = new FakeBusinessUnitRepository();
			_unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();

			_target = new PublishStartupJobsTask(_toggleManager, _eventPublisher, _loadAllTenants, _dataSourceScope, _tenantUnitOfWorkFake, uow => _businessUnitRepository, _unitOfWorkFactory);
		}

		[Test]
		public void ShouldNotPublishEventsWhenToggleIsFalse()
		{
			_toggleManager.Disable(Toggles.LastHandlers_ToHangfire_41203);

			_target.Execute(null);

			_eventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishEventWhenToggleIsTrue()
		{
			var tenantName = "TestTenant";
			_toggleManager.Enable(Toggles.LastHandlers_ToHangfire_41203);
			_loadAllTenants.Has(tenantName);
			_dataSourceScope.Stub(x => x.OnThisThreadUse(tenantName)).Return(new GenericDisposable(() => { }));
			_businessUnitRepository.Add(BusinessUnitFactory.CreateWithId("TestBu"));

			_target.Execute(null);

			_eventPublisher.PublishedEvents.OfType<InitialLoadScheduleProjectionEvent>().Should().Not.Be.Empty();
		}
	}
}