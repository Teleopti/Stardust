using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

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

		[Test]
		public void ShouldPersistExternalPerformanceData()
		{
			var person = PersonFactory.CreatePerson();
			person.SetEmploymentNumber("employmentNo");
			var externalPerformance = new ExternalPerformance
			{
				ExternalId = 1,
				Name = "test",
				DataType = ExternalPerformanceDataType.Numeric
			};
			WithUnitOfWork.Do(() =>
			{
				PersonRepo.Add(person);
				ExternalPerfRepo.Add(externalPerformance);
			});

			var externalPerformanceData = new ExternalPerformanceData
			{
				PersonId = person.Id.Value,
				ExternalPerformance = externalPerformance,
				OriginalPersonId = person.EmploymentNumber,
				DateFrom = new DateTime(2017, 12, 13, 0, 0, 0, DateTimeKind.Utc)
			};

			WithUnitOfWork.Do(() =>
			{
				Target.Add(externalPerformanceData);
			});

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
	}
}