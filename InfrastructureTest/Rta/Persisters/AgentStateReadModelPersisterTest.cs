using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStateReadModelPersisterTest
	{
		public IAgentStateReadModelPersister Target;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateReadModelForTest();

			Target.Persist(state);

			Target.Get(state.PersonId)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { BusinessUnitId = businessUnitId };

			Target.Persist(state);

			Target.Get(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { TeamId = teamId };

			Target.Persist(state);

			Target.Get(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { SiteId = siteId };

			Target.Persist(state);

			Target.Get(state.PersonId)
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				TeamId = null,
				SiteId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),

				StateCode = null,
				StateStartTime = null,
				StateName = null,

				Activity = null,
				NextActivity = null,
				NextActivityStartTime = null,

				RuleName = null,
				RuleStartTime = null,
				AlarmStartTime = null,
				StaffingEffect = null,
				RuleColor = null,
			});

			Target.Get(personId)
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateReadModelForTest { AlarmStartTime = "2015-12-11 08:00".Utc() };

			Target.Persist(state);

			Target.Get(state.PersonId)
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistIsAlarm()
		{
			var state = new AgentStateReadModelForTest { IsRuleAlarm = true };

			Target.Persist(state);

			Target.Get(state.PersonId)
				.IsRuleAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldPersistAlarmColor()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				AlarmColor = Color.Red.ToArgb()
			});

			Target.Get(personId)
				.AlarmColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldPersistShift()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						Color = Color.Green.ToArgb(),
						StartTime = "2016-06-01 10:00".Utc(),
						EndTime = "2016-06-01 11:00".Utc(),
						Name = "Phone"
					}
				}
			});

			var shift = Target.Get(personId).Shift.Single();
			shift.Color.Should().Be(Color.Green.ToArgb());
			shift.StartTime.Should().Be("2016-06-01 10:00".Utc());
			shift.EndTime.Should().Be("2016-06-01 11:00".Utc());
			shift.Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldUpdateShift()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Target.Persist(
				new AgentStateReadModelForTest
				{
					PersonId = personId,
					TeamId = teamId,
				});

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				TeamId = teamId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						Color = Color.Green.ToArgb()
					}
				}
			});

			Target.Get(personId)
				.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldUpdateLargeShift()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Target.Persist(
				new AgentStateReadModelForTest
				{
					PersonId = personId,
					TeamId = teamId,
				});

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				TeamId = teamId,
				Shift = Enumerable.Range(0, 100)
				.Select(_ => new AgentStateActivityReadModel
				{
					Name = new string('x', 100),
					Color = Color.Green.ToArgb()
				})
			});

			Target.Get(personId).Shift.Should().Have.Count.EqualTo(100);
		}

		[Test]
		public void ShouldPersistOutOfAdherences()
		{
			var state = new AgentStateReadModelForTest
			{
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 08:00".Utc(),
						EndTime = "2016-06-16 08:10".Utc()
					}
				}
			};

			Target.Persist(state);

			var outOfAdherence = Target.Get(state.PersonId)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}

		[Test]
		public void ShouldUpdateOutOfAdherences()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 08:00".Utc(),
						EndTime = "2016-06-16 08:10".Utc()
					}
				}
			});

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 08:00".Utc(),
						EndTime = "2016-06-16 08:10".Utc()
					},
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 08:20".Utc(),
						EndTime = null
					}
				}
			});

			var outOfAdherences = Target.Get(personId).OutOfAdherences;
			outOfAdherences.First().StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherences.First().EndTime.Should().Be("2016-06-16 08:10".Utc());
			outOfAdherences.Last().StartTime.Should().Be("2016-06-16 08:20".Utc());
			outOfAdherences.Last().EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldUpdateAlotOfOutOfAdherences()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				OutOfAdherences = Enumerable.Range(0, 59)
				.Select(m => new AgentStateOutOfAdherenceReadModel
				{
					StartTime = $"2016-06-16 08:{m:00}".Utc(),
					EndTime = $"2016-06-16 08:{m+1:00}".Utc()
				})
			});
			
			Target.Get(personId).OutOfAdherences.Should().Have.Count.EqualTo(59);
		}

		[Test]
		public void ShouldSetDeleted()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.Persist(model);

			Target.SetDeleted(personId, "2016-10-04 08:00".Utc());

			var result = Target.Get(personId);
			result.IsDeleted.Should().Be(true);
			result.ExpiresAt.Should().Be("2016-10-04 08:00".Utc());
		}
		
		[Test]
		public void ShouldUpdatePersonAssociation()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Target.Persist(new AgentStateReadModelForTest { PersonId = personId });

			Target.UpsertAssociation(personId, teamId, siteId, businessUnitId);

			var result = Target.Get(personId);
			result.TeamId.Should().Be(teamId);
			result.SiteId.Should().Be(siteId);
			result.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldInsertPersonAssociation()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();

			Target.UpsertAssociation(personId, teamId, siteId, businessUnitId);

			var result = Target.Get(personId);
			result.TeamId.Should().Be(teamId);
			result.SiteId.Should().Be(siteId);
			result.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistStateGroupId()
		{
			var personId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = stateGroupId
			});

			Target.Get(personId).StateGroupId.Should().Be(stateGroupId);
		}

		[Test]
		public void ShouldUpdateStateGroupId()
		{
			var personId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();
			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = null
			});

			Target.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = stateGroupId
			});

			Target.Get(personId).StateGroupId.Should().Be(stateGroupId);
		}

		[Test]
		public void ShouldRemoveOldRows()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.Persist(model);
			Target.SetDeleted(personId, "2016-10-04 08:30".Utc());
			
			Target.DeleteOldRows("2016-10-04 08:30".Utc());

			Target.Get(personId).Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveOlderRows()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.Persist(model);
			Target.SetDeleted(personId, "2016-10-04 08:30".Utc());

			Target.DeleteOldRows("2016-10-04 09:00".Utc());

			Target.Get(personId).Should().Be.Null();
		}
	}
}