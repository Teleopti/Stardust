using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.IntegrationTest.Areas.Reporting.Core
{
	[TestFixture]
	[InfrastructureTest]
	public class AnalyticsPermissionsUpdaterConcurrencyTest : IExtendSystem
	{
		public WithUnitOfWork WithUnitOfWork;
		public ConcurrencyRunner Run;
		public Database Database;
		public IPersonRepository PersonRepository;
		public IAnalyticsPermissionsUpdater Target;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PermissionsConverter>();
			extend.AddService<AnalyticsPermissionsUpdater>();
		}
		
		[Test]
		public void ShouldHandleConcurrency()
		{
			var businessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);
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
					Target.Handle(personId, businessUnitId);
				});

			}).Times(10);

			Run.Wait();
		}
	}
}