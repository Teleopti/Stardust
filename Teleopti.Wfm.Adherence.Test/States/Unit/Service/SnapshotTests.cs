using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[RtaTest]
	[TestFixture]
	public class SnapshotTests
	{
		public FakeAgentStatePersister Persister;
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldLogOutPersonsNotInSnapshot()
		{
			var personId = Guid.NewGuid();
			var loggedout = Guid.NewGuid();
			Database
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", personId)
				.WithStateGroup(loggedout, "loggedout", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup(null, "state")
				.WithStateCode("statecode")
				;

			Now.Is("2014-10-20 10:00");
			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc()
			});

			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Database.StoredStateFor(personId)
				.StateGroupId.Should().Be(loggedout);
		}

		[Test]
		public void ShouldOnlyLogOutPersonsNotInSnapshotFromSameSource()
		{
			var user2 = Guid.NewGuid();
			var state = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithDataSource("source2")
				.WithAgent("usercode2", user2)
				.WithStateGroup(Guid.NewGuid(), "loggedout", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup(state, "state")
				.WithStateCode("statecode")
				;
			Now.Is("2014-10-20 10:00");
			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:00".Utc()
			});
			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc(),
				SourceId = "source2",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source2",
				SnapshotId = "2014-10-20 10:00".Utc()
			});

			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Database.StoredStateFor(user2)
				.StateGroupId.Should().Be(state);
		}

		[Test]
		public void ShouldLogOutPersonNotInSnapshotWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			var loggedout = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", personId)
				.WithStateGroup(loggedout, "loggedout", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup(null, "state1")
				.WithStateCode("statecode1")
				.WithStateGroup(null, "state2")
				.WithStateCode("statecode2")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode2",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode2"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Database.StoredStateFor(personId)
				.StateGroupId.Should().Be(loggedout);
		}

		[Test]
		public void ShouldOnlyLogOutPersonsInSnapshotFromSameSourceWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			var state1 = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithDataSource("source2")
				.WithAgent("usercode2", personId)
				.WithStateGroup(Guid.NewGuid(), "loggedout", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup(state1, "state1")
				.WithStateCode("statecode1")
				.WithStateGroup(null, "state2")
				.WithStateCode("statecode2")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode2",
				SourceId = "source2",
				StateCode = "statecode1"
			});

			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode2"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Database.StoredStateFor(personId)
				.StateGroupId.Should().Be(state1);
		}

		[Test]
		public void ShouldNotLogOutAlreadyLoggedOutAgents()
		{
			var user2 = Guid.NewGuid();
			var loggedout = Guid.NewGuid();
			Database
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", user2)
				.WithStateGroup(loggedout, "loggedout", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup(null, "state")
				.WithStateCode("statecode")
				;
			Now.Is("2014-10-20 10:00");
			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = Rta.LogOutBySnapshot
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc()
			});

			Now.Is("2014-10-20 10:05");
			Target.Process(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Database.StoredStateFor(user2)
				.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldThrowIfNoLoggedOutStateGroups()
		{
			Assert.Throws<NoLoggedOutStateGroupsException>(() =>
			{
				Target.CloseSnapshot(new CloseSnapshotForTest
				{
					SnapshotId = Now.UtcDateTime()
				});
			});
		}

		[Test]
		public void ShouldThrowIfNoLoggedOutStateGroups2()
		{
			Database
				.WithAgent("user")
				.WithStateGroup(null, "state")
				.WithStateCode("statecode")
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "user",
				StateCode = "statecode"
			});

			Assert.Throws<NoLoggedOutStateGroupsException>(() =>
			{
				Target.CloseSnapshot(new CloseSnapshotForTest
				{
					SnapshotId = Now.UtcDateTime()
				});
			});
		}
	}
}