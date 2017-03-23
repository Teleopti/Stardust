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
		public void ShouldCheckAndUpdateWhenNoRecord()
		{
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.Any().Should().Be.True();
		}

		[Test]
		public void ShouldCheckAndUpdateWhenRecord()
		{
			Now.Is("2016-03-01 07:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.First().Should().Be.EqualTo(Now.UtcDateTime());
		}

		[Test]
		public void ShouldNotUpdateWhenRecentTimestamp()
		{
			Now.Is("2016-03-01 09:49");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.First().Should().Not.Be.EqualTo(Now.UtcDateTime());
			isUpdated.Should().Be.False();
		}

		[Test]
		public void ShouldUpdateWhenNoRecord()
		{
			Now.Is("2016-03-01 09:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.Any().Should().Be.True();
		}

		[Test]
		public void ShouldUpdateIfRecentTimestamp()
		{
			Now.Is("2016-03-01 09:49");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select StartTime from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime>();
			result.First().Should().Be.EqualTo(Now.UtcDateTime());
		}

		[Test, Ignore("WIP")]
		public void ShouldCheckAndUpdateAndSetTimeLockToNull()
		{
			Now.Is("2016-03-01 07:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.CheckAndUpdate(60);
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test, Ignore("WIP")]
		public void ShouldUpdateAndSetTimeLockToNull()
		{
			Now.Is("2016-03-01 07:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			var result = CurrentUnitOfWork.Current().FetchSession().CreateSQLQuery("select LockTimestamp from JobStartTime where BusinessUnit = :bu")
				.SetGuid("bu", CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.List<DateTime?>();
			result.First().Should().Be.Null();
		}

		[Test,Ignore("WIP")]
		public void ShouldUpdateIfLockTimestampIsNotValid()
		{
			Now.Is("2016-03-01 09:40");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Target.UpdateLockTimestamp();
			Now.Is("2016-03-01 09:50");
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.True();
		}

		[Test]
		public void ShouldNotUpdateIfLockTimestampIsValid()
		{
			Now.Is("2016-03-01 09:40");
			Target.Update(CurrentBusinessUnit.Current().Id.GetValueOrDefault());
			Now.Is("2016-03-01 09:50");
			Target.UpdateLockTimestamp();
			var isUpdated = Target.CheckAndUpdate(60);
			isUpdated.Should().Be.False();
		}

		//[Test]
		//public void ShouldPersist()
		//{
		//	var datetime = new DateTime(2016, 12, 16, 10, 0, 0);
		//	var buId = Guid.NewGuid();
		//	Target.Persist(buId, datetime);

		//	var result = Target.LoadAll();
		//	result.Keys.Count.Should().Be.EqualTo(1);
		//	result.Keys.FirstOrDefault().Should().Be.EqualTo(buId);
		//	result.Values.FirstOrDefault().Should().Be.EqualTo(datetime);
		//}

		//[Test]
		//public void ShouldDeleteAndPersist()
		//{
		//	var datetime = new DateTime(2016, 12, 16, 10, 0, 0);
		//	var buId = Guid.NewGuid();
		//	Target.Persist(buId, datetime);

		//	Target.Persist(buId, datetime.AddDays(1));

		//	var result = Target.LoadAll();
		//	result.Keys.Count.Should().Be.EqualTo(1);
		//	result.Keys.FirstOrDefault().Should().Be.EqualTo(buId);
		//	result.Values.FirstOrDefault().Should().Be.EqualTo(datetime.AddDays(1));
		//}
	}


}
