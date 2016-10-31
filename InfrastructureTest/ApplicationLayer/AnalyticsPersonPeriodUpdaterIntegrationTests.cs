using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[InfrastructureTest]
	[AnalyticsDatabaseTest]
	public class AnalyticsPersonPeriodUpdaterIntegrationTests : ISetup
	{
		public IPersonPeriodTransformer Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		private BusinessUnit businessUnit;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AnalyticsDateRepositoryWithCreation>().For<IAnalyticsDateRepository>();
		}

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);
			var quarterOfAnHourInterval = new QuarterOfAnHourInterval();

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(datasource);
			analyticsDataFactory.Setup(quarterOfAnHourInterval);

			analyticsDataFactory.Persist();
		}

		[Test]
		public void Test1()
		{
			AnalyticsPersonPeriod result = null;
			WithUnitOfWork.Do(() =>
			{
				WithAnalyticsUnitOfWork.Do(() =>
				{
					var personPeriodStartDate = new DateTime(2010, 01, 01);
					var team = TeamFactory.CreateSimpleTeam("Team1").WithId();
					var site = SiteFactory.CreateSimpleSite("Site1").WithId();
					site.AddTeam(team);

					var person = PersonFactory.CreatePerson("Test Person").WithId();
					var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate), team).WithId();
					person.AddPersonPeriod(personPeriod);

					List<AnalyticsSkill> t2;
					result = Target.Transform(person, personPeriod, out t2);
				});
			});
			result.Should().Not.Be.Null();
			result.ValidToDateId.Should().Be.EqualTo(-2); // The person period has not an end date.
			result.ValidToDateIdMaxDate.Should().Be.GreaterThanOrEqualTo(0);
			result.ValidToDateIdLocal.Should().Be.GreaterThanOrEqualTo(0);
		}
	}
}
