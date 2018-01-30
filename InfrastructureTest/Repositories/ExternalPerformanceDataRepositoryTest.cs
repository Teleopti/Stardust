using System;
using System.Collections.Generic;
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

			var externalPerformanceData = persistExternalPerformanceData(new DateOnly(2017, 12, 13), _person);

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

			var date1 = new DateOnly(2018, 1, 12);
			var date2 = new DateOnly(2018, 1, 13);
			var date3 = new DateOnly(2018, 1, 14);
			
			persistExternalPerformanceData(date1, _person);
			persistExternalPerformanceData(date2, _person);
			persistExternalPerformanceData(date3, _person);
			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.FindByPeriod(new DateOnlyPeriod(date1, date3));
				return data;
			});

			result.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotLoadDataWithinoutPeriod()
		{
			prepareRelatedData();

			var date1 = new DateOnly(2018, 1, 12);
			var date2 = new DateOnly(2018, 1, 13);
			var date3 = new DateOnly(2018, 1, 14);
			
			persistExternalPerformanceData(date1, _person);
			persistExternalPerformanceData(date2, _person);
			persistExternalPerformanceData(date3, _person);
			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.FindByPeriod(new DateOnlyPeriod(date2, date2));
				return data;
			});

			result.Count.Should().Be.EqualTo(1);
			result.ToList()[0].DateFrom.Should().Be.EqualTo(date2);
		}

		[Test]
		public void ShouldFindExternalPerformanceData()
		{
			var date = new DateOnly(2018, 01, 25);
			prepareRelatedData();
			persistExternalPerformanceData(date, _person);

			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.Find(date, new List<Guid>{_person.Id.Value}, 1);
				return data;
			});

			result.First().OriginalPersonId.Should().Be.EqualTo(_person.EmploymentNumber);
		}

		[Test]
		public void ShouldFindPersonWhoShouldGetBadge()
		{
			var date = new DateOnly(2018, 01, 25);
			var person2 = createPerson();
			prepareRelatedData();
			persistExternalPerformanceData(date, _person, 60);
			persistExternalPerformanceData(date, person2, 80);

			var result = WithUnitOfWork.Get(() =>
			{
				var data = Target.FindPersonsCouldGetBadgeOverThreshold(date, new List<Guid> { _person.Id.Value, person2.Id.Value }, 1, 70);
				return data;
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(person2.Id);
		}

		private IPerson createPerson()
		{
			var person = PersonFactory.CreatePerson();
			WithUnitOfWork.Do(() =>
			{
				PersonRepo.Add(person);
			});
			return person;
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

		private IExternalPerformanceData persistExternalPerformanceData(DateOnly date, IPerson person, double score = 0)
		{
			var externalPerformanceData = new ExternalPerformanceData
			{
				PersonId = person.Id.Value,
				ExternalPerformance = _externalPerformance,
				OriginalPersonId = person.EmploymentNumber,
				DateFrom = date,
				Score = score
			};

			WithUnitOfWork.Do(() => { Target.Add(externalPerformanceData); });

			return externalPerformanceData;
		}
	}
}