using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class UpdateAgentStateReadModelSnapshotTests
	{
		public FakeAgentStateReadModelPersister Persister;
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldLogOutPersonsNotInSnapshot()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", personId)
				.WithMappedRule("statecode", Guid.Empty, null, "A State")
				.WithStateGroup(null, "Logged Out", false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, Guid.Empty, null, "Logged Out")
				;

			Now.Is("2014-10-20 10:00");
			Target.SaveStateBatch(new BatchForTest
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

			Target.SaveStateBatch(new BatchForTest
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

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateName.Should().Be("Logged Out");
		}

		[Test]
		public void ShouldOnlyLogOutPersonsNotInSnapshotFromSameSource()
		{
			var personId = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithDataSource("source2")
				.WithAgent("usercode2", personId)
				.WithMappedRule("statecode", Guid.Empty, null, "A State")
				.WithStateGroup(null, "Logged Out", false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, Guid.Empty, null, "Logged Out")
				;
			Now.Is("2014-10-20 10:00");
			Target.SaveStateBatch(new BatchForTest
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
				SnapshotId = "2014-10-20 10:00".Utc(),
				SourceId = "source1"
			});

			Target.SaveStateBatch(new BatchForTest
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
				SnapshotId = "2014-10-20 10:00".Utc(),
				SourceId = "source2"
			});

			Target.SaveStateBatch(new BatchForTest
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
				SnapshotId = "2014-10-20 10:05".Utc(),
				SourceId = "source1"
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateName.Should().Be("A State");
		}

		[Test]
		public void ShouldLogOutPersonNotInSnapshotWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", personId)
				.WithMappedRule("statecode1", Guid.Empty, null, "A State")
				.WithStateGroup(null, "Logged Out", false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, Guid.Empty, null, "Logged Out")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode2",
				SourceId = "source1",
				StateCode = "statecode1",
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode1"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateName.Should().Be("Logged Out");
		}

		[Test]
		public void ShouldOnlyLogOutPersonsInSnapshotFromSameSourceWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			Database
				.WithDataSource("source1")
				.WithAgent("usercode1", Guid.NewGuid())
				.WithDataSource("source2")
				.WithAgent("usercode2", personId)
				.WithMappedRule("statecode1", Guid.Empty, null, "A State")
				.WithStateGroup(null, "Logged Out", false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, Guid.Empty, null, "Logged Out")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode2",
				SourceId = "source2",
				StateCode = "statecode1",
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2014-10-20 10:05".Utc(),
				SourceId = "source1",
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "statecode1"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SourceId = "source1",
				SnapshotId = "2014-10-20 10:05".Utc()
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateName.Should().Be("A State");
		}

		[Test]
		public void ShouldNotLogOutAlreadyLoggedOutAgents()
		{
			var user2 = Guid.NewGuid();
			Database
				.WithAgent("usercode1", Guid.NewGuid())
				.WithAgent("usercode2", user2)
				.WithStateGroup(null, "state", true, false)
				.WithStateCode("statecode")
				.WithStateGroup(null, "loggedout", false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				;
			Now.Is("2014-10-20 10:00");
			Target.SaveStateBatch(new BatchForTest
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
						StateCode = Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2014-10-20 10:00".Utc()
			});

			Now.Is("2014-10-20 10:05");
			Target.SaveStateBatch(new BatchForTest
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

			Persister.Models
				.Single(x => x.PersonId == user2)
				.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}
		
	}
}