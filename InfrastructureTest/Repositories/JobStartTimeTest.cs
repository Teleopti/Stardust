using System;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	public class JobStartTimeRepositoryTest
	{
		public IJobStartTimeRepository Target;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public ICurrentUnitOfWork CurrentUnitOfWork;

		[Test]
		public void ShouldPersistStartTime()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60, businessUnitId);
			
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", businessUnitId)
				.List<DateTime>();
			result.Any().Should().Be.True();
		}

		[Test]
		public void ShouldPersistTimelockAsNull()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60, businessUnitId);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", businessUnitId)
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test]
		public void ShouldCheckAndUpdateWhenNoRecord()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60, businessUnitId);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldCheckAndUpdateWhenOldRecord()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 07:50");
			Target.CheckAndUpdate(60, businessUnitId);
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60, businessUnitId);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldCheckAndUpdateIfLockTimestampIsNotValid()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId);
			Target.UpdateLockTimestamp(businessUnitId);
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60, businessUnitId);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateTimestampTo1MinuteFromNow()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId); 
			Now.Is("2016-03-01 09:50");
			Target.UpdateLockTimestamp(businessUnitId);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", businessUnitId)
				.List<DateTime?>();
			result.First().Should().Be.EqualTo(Now.UtcDateTime().AddMinutes(1));
		}

		[Test]
		public void ShouldResetTimestamp()  
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId);
			Target.UpdateLockTimestamp(businessUnitId);
			Now.Is("2016-03-01 09:50");
			Target.ResetLockTimestamp(businessUnitId);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", businessUnitId)
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test]
		public void ShouldNotUpdateIfLockTimestampIsValid()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId);
			Now.Is("2016-03-01 09:50");
			Target.UpdateLockTimestamp(businessUnitId);
			var isUpdated = Target.CheckAndUpdate(60, businessUnitId);
			isUpdated.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateIfLockTimestampIsNullAndStartTimeIsNotOld()
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId);
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60, businessUnitId);
			isUpdated.Should().Be.False();
		}

		[Test]
		public void ShouldResetLockTimeStampIfInvalid() 
		{
			var businessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault();
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60, businessUnitId);
			Target.UpdateLockTimestamp(businessUnitId);
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60, businessUnitId);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", businessUnitId)
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}
	}


}
