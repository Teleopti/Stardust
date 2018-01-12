using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	[TestFixture, Category("BucketB")]
	public class ExternalPerformanceDataRepositoryTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IExternalPerformanceDataRepository Target;
		public IPersonRepository PersonRepo;
		public IExternalPerformanceRepository ExternalPerfRepo;

		private IPerson _person;
		private IExternalPerformance _externalPerformance;

		[Test]
		public void ShouldPersistExternalPerformanceData()
		{
			prepareRelatedData();

			var externalPerformanceData = persistExternalPerformanceData(new DateTime(2017, 12, 13, 0, 0, 0, DateTimeKind.Utc));

			var externalPerformanceId = 0;
			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.Get(externalPerformanceData.Id.GetValueOrDefault());
				externalPerformanceId = data.ExternalPerformance.ExternalId;
				return data;
			});

			result.Should().Not.Be.Null();
			externalPerformanceId.Should().Be.Equals(1);
		}

		[Test]
		public void ShouldLoadDataWithinPeriod()
		{
			prepareRelatedData();

			var date1 = new DateTime(2018, 1, 12, 0, 0, 0, DateTimeKind.Utc);
			var date2 = new DateTime(2018, 1, 13, 0, 0, 0, DateTimeKind.Utc);
			var date3 = new DateTime(2018, 1, 14, 0, 0, 0, DateTimeKind.Utc);
			
			persistExternalPerformanceData(date1);
			persistExternalPerformanceData(date2);
			persistExternalPerformanceData(date3);
			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.FindByPeriod(new DateTimePeriod(date1, date3));
				return data;
			});

			result.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotLoadDataWithinoutPeriod()
		{
			prepareRelatedData();

			var date1 = new DateTime(2018, 1, 12, 0, 0, 0, DateTimeKind.Utc);
			var date2 = new DateTime(2018, 1, 13, 0, 0, 0, DateTimeKind.Utc);
			var date3 = new DateTime(2018, 1, 14, 0, 0, 0, DateTimeKind.Utc);
			
			persistExternalPerformanceData(date1);
			persistExternalPerformanceData(date2);
			persistExternalPerformanceData(date3);
			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.FindByPeriod(new DateTimePeriod(date2, date2));
				return data;
			});

			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].DateFrom.Should().Be.EqualTo(date2);
		}

		private void prepareRelatedData()
		{
			_person = PersonFactory.CreatePerson();
			_person.SetEmploymentNumber("employmentNo");
			_externalPerformance = new ExternalPerformance
			{
				ExternalId = 1,
				Name = "test",
				DataType = ExternalPerformanceDataType.Numeric
			};
			WithUnitOfWork.Do(() =>
			{
				PersonRepo.Add(_person);
				ExternalPerfRepo.Add(_externalPerformance);
			});
		}

		private IExternalPerformanceData persistExternalPerformanceData(DateTime date)
		{
			var externalPerformanceData = new ExternalPerformanceData
			{
				PersonId = _person.Id.Value,
				ExternalPerformance = _externalPerformance,
				OriginalPersonId = _person.EmploymentNumber,
				DateFrom = date
			};

			WithUnitOfWork.Do(() => { Target.Add(externalPerformanceData); });

			return externalPerformanceData;
		}
	}
}