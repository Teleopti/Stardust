using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillForecastJobStartTimeRepositoryTest
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IMutateNow Now;
		public ISkillForecastJobStartTimeRepository Target;
		public IBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldSavejobStartTime()
		{
	
			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			var result = readData(bu.Id.GetValueOrDefault());

			result.Any().Should().Be.True();
			result.FirstOrDefault().StartedAt.Should().Be.EqualTo(now);
			result.FirstOrDefault().Locked.Should().Be.EqualTo(now.AddMinutes(60));
		}

		[Test]
		public void ShouldUpdateJobStartTimeForExistingJobs()
		{

			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			var newNow = now.AddDays(1);
			Now.Is(newNow);
			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());
			var result = readData(bu.Id.GetValueOrDefault());
			result.Count.Should().Be.EqualTo(1);
			result.FirstOrDefault().StartedAt.Should().Be.EqualTo(newNow);
			result.FirstOrDefault().Locked.Should().Be.EqualTo(newNow.AddMinutes(60));
		}

		[Test]
		public void ShouldReturnTrueIfLockedTimestampIsStillValid()
		{

			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			now = new DateTime(2019, 2, 17, 16, 30, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.IsLockTimeValid(bu.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfLockedTimestampIsNotValid()
		{

			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			now = new DateTime(2019, 2, 17, 17, 01, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.IsLockTimeValid(bu.Id.GetValueOrDefault()).Should().Be.False();
		}

		[Test]
		public void ShouldResetLockTimestamp()
		{

			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			now = new DateTime(2019, 2, 17, 17, 01, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.ResetLock(bu.Id.GetValueOrDefault());

			var result = readData(bu.Id.GetValueOrDefault());
			result.Count.Should().Be.EqualTo(1);
			result.FirstOrDefault().Locked.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfLockedTimestampIsNotSet()
		{

			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			BusinessUnitRepository.Add(bu);
			CurrentUnitOfWork.Current().PersistAll();

			var now = new DateTime(2019, 2, 17, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.UpdateJobStartTime(bu.Id.GetValueOrDefault());

			now = new DateTime(2019, 2, 17, 17, 01, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.ResetLock(bu.Id.GetValueOrDefault());

			Target.IsLockTimeValid(bu.Id.GetValueOrDefault()).Should().Be.True();
		}

		private List<FakeStartTimeModel> readData(Guid businessUnitId)
		{
			var result = new List<FakeStartTimeModel>();
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var selectCommand = new SqlCommand(@"select StartTime, LockTimestamp from [SkillForecastJobStartTime] where BusinessUnit = @bu", connection, transaction))
					{
						selectCommand.Parameters.AddWithValue("@bu", businessUnitId);
						using (var reader = selectCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								var item = new FakeStartTimeModel
								{
									BusinessUnit = businessUnitId,
									StartedAt = reader.GetDateTime(0)

								};
								if (!reader.IsDBNull(1))
									item.Locked = reader.GetDateTime(1);
								result.Add(item);
							}
						}
					}
					
				}
			}

			return result;
		}
	}
}