using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[AnalyticsDatabaseTest]
	public class AnalyticsPersonPeriodUpdaterIntegrationTests
	{
		public IPersonPeriodTransformer Target;
		public IAnalyticsDateRepository DateRepository;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public WithUnitOfWork WithUnitOfWork;
		private TestCommon.TestData.Analytics.BusinessUnit businessUnit;

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			businessUnit = new TestCommon.TestData.Analytics.BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);
			var quarterOfAnHourInterval = new QuarterOfAnHourInterval();
			var dates = new DatesFromPeriod(new DateTime(2009, 12, 01), new DateTime(2009, 12, 31));

			analyticsDataFactory.Setup(new SysConfiguration("TimeZoneCode", "UTC"));
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(datasource);
			analyticsDataFactory.Setup(quarterOfAnHourInterval);
			analyticsDataFactory.Setup(dates);

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldCreatePersonPeriodWithStartDate_CorrectValidToDates()
		{
			AnalyticsPersonPeriod result = null;
			var personPeriodStartDate = new DateTime(2010, 01, 01);
			var team = TeamFactory.CreateSimpleTeam("Team1").WithId();
			var site = SiteFactory.CreateSimpleSite("Site1").WithId();
			site.AddTeam(team);

			var person = PersonFactory.CreatePerson("Test Person").WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate), team).WithId();
			person.AddPersonPeriod(personPeriod);

			List<AnalyticsSkill> t2;
			WithUnitOfWork.Do(() =>
			{
				WithAnalyticsUnitOfWork.Do(() =>
				{
					result = Target.Transform(person, personPeriod, out t2);
				});
			});
			result.Should().Not.Be.Null();
			result.ValidToDateId.Should().Be.EqualTo(AnalyticsDate.Eternity.DateId);
			result.ValidToDateIdMaxDate.Should().Be.GreaterThanOrEqualTo(0);
			result.ValidToDateIdLocal.Should().Be.GreaterThanOrEqualTo(0);
		}

		[Test]
		public void ShouldCreateTwoPersonPeriodWithStartAndEndDate_CorrectValidToDates()
		{
			AnalyticsPersonPeriod result1 = null;
			AnalyticsPersonPeriod result2 = null;
			var personPeriodStartDate = new DateTime(2010, 01, 01);
			var team = TeamFactory.CreateSimpleTeam("Team1").WithId();
			var site = SiteFactory.CreateSimpleSite("Site1").WithId();
			site.AddTeam(team);

			var person = PersonFactory.CreatePerson("Test Person").WithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate), team).WithId();
			var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate.AddYears(1)), team).WithId();
			person.AddPersonPeriod(personPeriod1);
			person.AddPersonPeriod(personPeriod2);

			List<AnalyticsSkill> t1;
			List<AnalyticsSkill> t2;

			WithUnitOfWork.Do(() =>
			{
				WithAnalyticsUnitOfWork.Do(() =>
				{
					result1 = Target.Transform(person, personPeriod1, out t1);
					result2 = Target.Transform(person, personPeriod2, out t2);
				});
			});
			result1.Should().Not.Be.Null();
			result1.ValidToDate.Should().Be.EqualTo(personPeriodStartDate.AddYears(1));
			result1.ValidToDateId.Should().Not.Be.EqualTo(AnalyticsDate.Eternity.DateId);
			result1.ValidToDateIdMaxDate.Should().Be.GreaterThanOrEqualTo(0);
			result1.ValidToDateIdLocal.Should().Be.GreaterThanOrEqualTo(0);

			result2.ValidFromDate.Should().Be.EqualTo(personPeriodStartDate.AddYears(1));
			result2.ValidToDate.Should().Be.EqualTo(AnalyticsDate.Eternity.DateDate);
			result2.ValidToDateId.Should().Be.EqualTo(AnalyticsDate.Eternity.DateId);
		}

		[Test]
		public void ShouldCreateOnePersonPeriodWithStartAndLeavingDateDate_CorrectValidToDates()
		{
			AnalyticsPersonPeriod result1 = null;
			var personPeriodStartDate = new DateTime(2010, 01, 01);
			var leavingDate = personPeriodStartDate.AddYears(2);
			var team = TeamFactory.CreateSimpleTeam("Team1").WithId();
			var site = SiteFactory.CreateSimpleSite("Site1").WithId();
			site.AddTeam(team);

			var person = PersonFactory.CreatePerson("Test Person").WithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate), team).WithId();
			person.AddPersonPeriod(personPeriod1);

			person.TerminatePerson(new DateOnly(leavingDate), new PersonAccountUpdaterDummy());

			List<AnalyticsSkill> t1;
			WithUnitOfWork.Do(() =>
			{
				WithAnalyticsUnitOfWork.Do(() =>
				{
					result1 = Target.Transform(person, personPeriod1, out t1);
				});
			});
			result1.Should().Not.Be.Null();
			result1.ValidToDate.Should().Be.EqualTo(leavingDate.AddDays(1)); // One extra day because of no laps between person periods.
			result1.ValidToDateId.Should().Not.Be.EqualTo(AnalyticsDate.Eternity.DateId);
			result1.ValidToDateIdMaxDate.Should().Be.EqualTo(result1.ValidToDateId);
			result1.ValidToDateIdLocal.Should().Be.GreaterThanOrEqualTo(0);
			result1.ValidToDateLocal.Should().Be.EqualTo(leavingDate);
		}
		
		[TestCase("UTC", 0)]
		[TestCase("Dateline Standard Time", +12)]
		[TestCase("Tonga Standard Time", -13)]
		public void ShouldCreatePersonPeriodWith_CorrectValidToDates(string timeZone, int hoursToGetToUtcSummerTime)
		{
			AnalyticsPersonPeriod result1 = null;
			var personPeriodStartDate = new DateTime(2010, 05, 10);
			var leavingDate = new DateTime(2010, 08, 10);
			var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
			var team = TeamFactory.CreateSimpleTeam("Team1").WithId();
			var site = SiteFactory.CreateSimpleSite("Site1").WithId();
			site.AddTeam(team);

			var person = PersonFactory.CreatePerson(new Name("Test", "Lastname"), timezone).WithId();
			var personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(personPeriodStartDate), team).WithId();
			person.AddPersonPeriod(personPeriod1);

			person.TerminatePerson(new DateOnly(leavingDate), new PersonAccountUpdaterDummy());

			List<AnalyticsSkill> t1;

			WithUnitOfWork.Do(() =>
			{
				WithAnalyticsUnitOfWork.Do(() =>
				{
					result1 = Target.Transform(person, personPeriod1, out t1);
				});
			});

			// Check dates
			result1.ValidFromDateLocal.Should().Be.EqualTo(personPeriodStartDate);
			result1.ValidToDateLocal.Should().Be.EqualTo(leavingDate);

			result1.ValidFromDate.Should().Be.EqualTo(personPeriodStartDate.AddHours(hoursToGetToUtcSummerTime));
			result1.ValidToDate.Should().Be.EqualTo(leavingDate.AddDays(1).AddHours(hoursToGetToUtcSummerTime)); // One extra day because of no laps between person periods.

			// Check date ids
			var dateList = WithAnalyticsUnitOfWork.Get(() => DateRepository.GetAllPartial());
			result1.ValidToDateId.Should().Be.EqualTo(dateList.First(d => d.DateDate.Date == result1.ValidToDate.Date).DateId);
			result1.ValidToDateIdLocal.Should().Be.EqualTo(dateList.First(d => d.DateDate.Date == result1.ValidToDateLocal.Date).DateId);
			result1.ValidToDateIdMaxDate.Should().Be.EqualTo(dateList.Last(d => d != AnalyticsDate.Eternity).DateId - 42);
			result1.ValidFromDateId.Should().Be.EqualTo(dateList.First(d => d.DateDate.Date == result1.ValidFromDate.Date).DateId);
			result1.ValidFromDateIdLocal.Should().Be.EqualTo(dateList.First(d => d.DateDate.Date == result1.ValidFromDateLocal.Date).DateId);
		}
	}
}

