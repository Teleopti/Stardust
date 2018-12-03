using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsRequestRepositoryTest
	{
		public IAnalyticsRequestRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private UtcAndCetTimeZones _timeZones;
		private ExistingDatasources _datasource;
		private const int businessUnitId = 12;
		private const int personId = 10;
		private AnalyticsDataFactory analyticsDataFactory;


		[SetUp]
		public void Setup()
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			_timeZones = new UtcAndCetTimeZones();
			_datasource = new ExistingDatasources(_timeZones);

		}

		private void commonSetup()
		{
			analyticsDataFactory.Setup(new Person(personId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen",
				new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, businessUnitId, Guid.NewGuid(), _datasource, false, _timeZones.UtcTimeZoneId));

			setupForFactRequest();

			analyticsDataFactory.Persist();
		}

		private void setupForFactRequest()
		{
			analyticsDataFactory.Setup(Scenario.DefaultScenarioFor(10, Guid.NewGuid()));

			var absEmpty = new Absence(-1, Guid.NewGuid(), "Empty", Color.Black, _datasource, businessUnitId);
			var abs = new Absence(22, Guid.NewGuid(), "Freee", Color.LightGreen, _datasource, businessUnitId);

			analyticsDataFactory.Setup(abs);
			analyticsDataFactory.Setup(absEmpty);
			analyticsDataFactory.Setup(new CurrentWeekDates());
			analyticsDataFactory.Setup(new QuarterOfAnHourInterval());
		}

		[Test]
		public void ShouldAddAndUpdateRequest()
		{
			commonSetup();
			var analyticsRequest = new AnalyticsRequest
			{
				RequestStartDateId = 3,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00),
				ApplicationDatetime = new DateTime(2016, 06, 02, 12, 55, 00),
				RequestStartDate = new DateTime(2016, 06, 02),
				RequestStartTime = new DateTime(2016, 06, 02, 13, 00, 00),
				RequestEndDate = new DateTime(2016, 06, 02),
				RequestEndTime = new DateTime(2016, 06, 02, 14, 00, 00),
				RequestStartDateCount = 1,
				RequestedTimeMinutes = 60
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(analyticsRequest);
			});

			analyticsRequest.AbsenceId = 22;

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(analyticsRequest);
			});
		}

		[Test]
		public void ShouldAddAndUpdateRequestedDay()
		{
			commonSetup();
			var expected = new AnalyticsRequestedDay
			{
				RequestDateId = 3,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00)
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(expected);
			});

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAnalyticsRequestedDays(expected.RequestCode));

			result.Should().Not.Be.Empty();
			var actual = result.First();
			actual.RequestDayCount.Should().Be.EqualTo(expected.RequestDayCount);
			actual.BusinessUnitId.Should().Be.EqualTo(expected.BusinessUnitId);
			actual.DatasourceId.Should().Be.EqualTo(expected.DatasourceId);
			actual.PersonId.Should().Be.EqualTo(expected.PersonId);
			actual.RequestCode.Should().Be.EqualTo(expected.RequestCode);
			actual.RequestDateId.Should().Be.EqualTo(expected.RequestDateId);
			actual.RequestStatusId.Should().Be.EqualTo(expected.RequestStatusId);
			actual.RequestTypeId.Should().Be.EqualTo(expected.RequestTypeId);
			actual.DatasourceUpdateDate.Should().Be.EqualTo(expected.DatasourceUpdateDate);

			expected.AbsenceId = 22;
			expected.RequestTypeId = 1;
			expected.RequestDayCount = 2;
			expected.RequestStatusId = 1;
			expected.PersonId = personId;
			expected.DatasourceUpdateDate = new DateTime(2016, 06, 02, 13, 54, 00);


			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(expected);
			});

			result = WithAnalyticsUnitOfWork.Get(() => Target.GetAnalyticsRequestedDays(expected.RequestCode));

			result.Should().Not.Be.Empty();
			actual = result.First();
			actual.RequestDayCount.Should().Be.EqualTo(expected.RequestDayCount);
			actual.BusinessUnitId.Should().Be.EqualTo(expected.BusinessUnitId);
			actual.DatasourceId.Should().Be.EqualTo(expected.DatasourceId);
			actual.PersonId.Should().Be.EqualTo(expected.PersonId);
			actual.RequestCode.Should().Be.EqualTo(expected.RequestCode);
			actual.RequestDateId.Should().Be.EqualTo(expected.RequestDateId);
			actual.RequestStatusId.Should().Be.EqualTo(expected.RequestStatusId);
			actual.RequestTypeId.Should().Be.EqualTo(expected.RequestTypeId);
			actual.DatasourceUpdateDate.Should().Be.EqualTo(expected.DatasourceUpdateDate);

		}

		[Test]
		public void ShouldDeleteRequestedDay()
		{
			commonSetup();
			var expected = new AnalyticsRequestedDay
			{
				RequestDateId = 3,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00)
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(expected);
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Delete(new [] { expected });
			});
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAnalyticsRequestedDays(expected.RequestCode));

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteEverythingForARequest()
		{
			commonSetup();
			var analyticsRequest = new AnalyticsRequest
			{
				RequestStartDateId = 3,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00),
				ApplicationDatetime = new DateTime(2016, 06, 02, 12, 55, 00),
				RequestStartDate = new DateTime(2016, 06, 02),
				RequestStartTime = new DateTime(2016, 06, 02, 13, 00, 00),
				RequestEndDate = new DateTime(2016, 06, 02),
				RequestEndTime = new DateTime(2016, 06, 02, 14, 00, 00),
				RequestStartDateCount = 1,
				RequestedTimeMinutes = 60
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(analyticsRequest);
			});

			var analyticsRequestedDay = new AnalyticsRequestedDay
			{
				RequestDateId = 3,
				RequestCode = analyticsRequest.RequestCode,
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00)
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(analyticsRequestedDay);
			});

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Delete(analyticsRequest.RequestCode);
			});

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAnalyticsRequestedDays(analyticsRequest.RequestCode));

			result.Should().Be.Empty();
		}


		private void insertPerson(int personPeriodId, DateTime validFrom, DateTime validTo, bool toBeDeleted = false, int validateFromDateId = 0, int validateToDateId = -2)
		{
			analyticsDataFactory.Setup(new Person(personPeriodId, Guid.NewGuid(), Guid.NewGuid(), "Ashley", "Andeen", validFrom,
				validTo, validateFromDateId, validateToDateId, businessUnitId, Guid.NewGuid(), _datasource, toBeDeleted,
				_timeZones.UtcTimeZoneId));
		}

		private AnalyticsRequest getTestRequest(int dateId, int personPeriodId)
		{
			return new AnalyticsRequest
			{
				RequestStartDateId = dateId,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personPeriodId,
				BusinessUnitId = businessUnitId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00),
				ApplicationDatetime = new DateTime(2016, 06, 02, 12, 55, 00),
				RequestStartDate = new DateTime(2016, 06, 02),
				RequestStartTime = new DateTime(2016, 06, 02, 13, 00, 00),
				RequestEndDate = new DateTime(2016, 06, 02),
				RequestEndTime = new DateTime(2016, 06, 02, 14, 00, 00),
				RequestStartDateCount = 1,
				RequestedTimeMinutes = 60
			};
		}

		private AnalyticsRequestedDay getTestRequestedDay(int dateId, int personPeriodId)
		{
			return new AnalyticsRequestedDay
			{
				RequestDateId = dateId,
				RequestCode = Guid.NewGuid(),
				RequestTypeId = 0,
				RequestDayCount = 1,
				RequestStatusId = 0,
				DatasourceId = 1,
				PersonId = personPeriodId,
				AbsenceId = -1,
				DatasourceUpdateDate = new DateTime(2016, 06, 02, 12, 54, 00)
			};
		}

		[Test]
		public void ShouldUpdateFactRequestWithUnlinkedPersonidsWhenAddNewPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 20)),
				DateId = 20
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 26)),
				DateId = 26
			};
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24), false, 10, 24);
			insertPerson(personPeriod2, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate, false, 25, -2);
			setupForFactRequest();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(getTestRequest(specificDate1.DateId, personPeriod1));
				Target.AddOrUpdate(getTestRequest(specificDate2.DateId, personPeriod1));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdateFactRequestDaysWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 7)),
				DateId = 7
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 15)),
				DateId = 15
			};
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod2, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate, false, 5, -2);
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true, 10, -2);
			setupForFactRequest();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(getTestRequest(specificDate1.DateId, personPeriod1));
				Target.AddOrUpdate(getTestRequest(specificDate2.DateId, personPeriod2));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { personPeriod1, personPeriod2 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldUpdateFactRequestedDaysWithUnlinkedPersonidsWhenAddNewPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 20)),
				DateId = 20
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 26)),
				DateId = 26
			};
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), new DateTime(2015, 1, 24), false, 10, 24);
			insertPerson(personPeriod2, new DateTime(2015, 1, 25), AnalyticsDate.Eternity.DateDate, false, 25, -2);
			setupForFactRequest();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(getTestRequestedDay(specificDate1.DateId, personPeriod1));
				Target.AddOrUpdate(getTestRequestedDay(specificDate2.DateId, personPeriod1));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(2);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(0);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { 10, 11 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldUpdateFactRequestedWithUnlinkedPersonidsWhenDeleteExistingPersonPeriod()
		{
			var specificDate1 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 7)),
				DateId = 7
			};
			var specificDate2 = new SpecificDate
			{
				Date = new DateOnly(new DateTime(2015, 1, 15)),
				DateId = 15
			};
			analyticsDataFactory.Setup(specificDate1);
			analyticsDataFactory.Setup(specificDate2);

			const int personPeriod1 = 10;
			const int personPeriod2 = 11;
			insertPerson(personPeriod2, new DateTime(2015, 1, 5), AnalyticsDate.Eternity.DateDate, false, 5, -2);
			insertPerson(personPeriod1, new DateTime(2015, 1, 10), AnalyticsDate.Eternity.DateDate, true, 10, -2);
			setupForFactRequest();
			analyticsDataFactory.Persist();

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(getTestRequestedDay(specificDate1.DateId, personPeriod1));
				Target.AddOrUpdate(getTestRequestedDay(specificDate2.DateId, personPeriod2));
			});

			var rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(1);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(1);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateUnlinkedPersonids(new[] { personPeriod1, personPeriod2 }));
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod1));
			rowCount.Should().Be.EqualTo(0);
			rowCount = WithAnalyticsUnitOfWork.Get(() => Target.GetFactRequestedDaysRowCount(personPeriod2));
			rowCount.Should().Be.EqualTo(2);
		}
	}
}