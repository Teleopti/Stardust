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
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.Any().Should().Be.True();
		}

		[Test]
		public void ShouldPersistTimelockAsNull()
		{
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test]
		public void ShouldCheckAndUpdateWhenNoRecord()
		{
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldCheckAndUpdateWhenOldRecord()
		{
			Now.Is("2016-03-01 07:50");
			Target.CheckAndUpdate(60);
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldCheckAndUpdateIfLockTimestampIsNotValid()
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60);
			Target.UpdateLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldUpdateTimestampTo1MinuteFromNow()
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60); 
			Now.Is("2016-03-01 09:50");
			Target.UpdateLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.EqualTo(Now.UtcDateTime().AddMinutes(1));
		}

		[Test]
		public void ShouldResetTimestamp()  
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60);
			Target.UpdateLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.ResetLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test]
		public void ShouldNotUpdateIfLockTimestampIsValid()
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60);
			Now.Is("2016-03-01 09:50");
			Target.UpdateLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.False();
		}

		[Test]
		public void ShouldNotUpdateIfLockTimestampIsNullAndStartTimeIsNotOld()
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60);
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.False();
		}

		[Test]
		public void ShouldResetLockTimeStampIfInvalid() 
		{
			Now.Is("2016-03-01 09:40");
			Target.CheckAndUpdate(60);
			Target.UpdateLockTimestamp(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}
	}


}
