using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.Web.IntegrationTest.Areas.Reporting.Core
{
	[TestFixture]
	[InfrastructureTest]
	public class AnalyticsPermissionsUpdaterConcurrencyTest : ISetup
	{
		public IAnalyticsPermissionRepository AnalyticsPermissionRepository;
		public IAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public IAnalyticsPermissionExecutionRepository AnalyticsPermissionExecutionRepository;
		public IPermissionsConverter PermissionsConverter;
		public IDistributedLockAcquirer DistributedLockAcquirer;
		public ICurrentAnalyticsUnitOfWork CurrentAnalyticsUnitOfWork;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		public ConcurrencyRunner Run;
		public Database Database;
		public IPersonRepository PersonRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PermissionsConverter>().For<IPermissionsConverter>();
		}

		[Test]
		public void ShouldHandleConcurrency()
		{
			var businessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);
			analyticsDataFactory.Setup(businessUnit);
		
			Database.WithAgent("test1");
			var personId = Database.PersonIdFor("test1");
			IPerson person = null;
			Guid personPeriodId = Guid.Empty;

			WithUnitOfWork.Do(() =>
			{
				person = PersonRepository.Get(personId);
				personPeriodId = person.PersonPeriodCollection.FirstOrDefault().Id.Value;
			});

			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, 0, businessUnitId,
				false, timeZones.UtcTimeZoneId, personPeriodId));

			analyticsDataFactory.Persist();

			Run.InParallel(() =>
			{
				10.Times(i =>
				{
					WithAnalyticsUnitOfWork.Do(() =>
					{
						WithUnitOfWork.Do(() =>
						{
							var target = new AnalyticsPermissionsUpdater(AnalyticsPermissionRepository,
								AnalyticsBusinessUnitRepository,
								AnalyticsPermissionExecutionRepository,
								PermissionsConverter,
								DistributedLockAcquirer,
								CurrentAnalyticsUnitOfWork);

							target.Handle(personId, businessUnitId);
						});
					});
				});

			}).Times(10);

			Run.Wait();
		}
	}
}