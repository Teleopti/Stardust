using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[DatabaseTest]
	public class RtaEventStoreUpgradeTest : IIsolateSystem
	{
		public IRtaEventStore Events;
		public IRtaEventStoreTester EventsT;
		public IRtaEventStoreUpgrader Target;
		public WithUnitOfWork WithUnitOfWork;
		public Database Database;
		public ILogOnOffContext Context;
		public ICurrentDataSource DataSource;
		public IDataSourceScope DataSourceScope;

		[Test]
		public void ShouldUpdateDate()
		{
			var dataSource = DataSource.Current();
			var person = Guid.NewGuid();
			WithUnitOfWork.Do(() => { Events.Add(new PersonStateChangedEvent {PersonId = person, Timestamp = "2018-10-30 08:00".Utc()}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate); });
			Context.Logout();

			using (DataSourceScope.OnThisThreadUse(dataSource))
			{
				Target.Upgrade();

				WithUnitOfWork.Get(() => EventsT.LoadAllForTest())
					.OfType<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-30".Date());
			}
		}

		[Test]
		public void ShouldUpgradeDateFromSchedule()
		{
			var dataSource = DataSource.Current();
			Database
				.WithActivity("phone")
				.WithAgent()
				.WithAssignment("2018-10-31")
				.WithAssignedActivity("phone", "2018-10-31 23:00", "2018-11-01 06:00")
				;
			var personId = Database.CurrentPersonId();
			WithUnitOfWork.Do(() =>
			{
				Events.Add(new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = "2018-11-01 01:00".Utc()
				}, DeadLockVictim.No, RtaEventStoreVersion.WithoutBelongsToDate);
			});
			Context.Logout();

			using (DataSourceScope.OnThisThreadUse(dataSource))
			{
				Target.Upgrade();

				WithUnitOfWork.Get(() => EventsT.LoadAllForTest())
					.OfType<PersonStateChangedEvent>().Single().BelongsToDate.Should().Be("2018-10-31".Date());
			}
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}