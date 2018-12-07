using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
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
	}
}