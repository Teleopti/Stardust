using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStateReadModelPersisterTest
	{
		public IAgentStateReadModelPersister Target;
		public IAgentStateReadModelReader Reader;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateReadModelForTest();

			Target.Persist(state);

			var result = Reader.Load(new[] { state.PersonId }).SingleOrDefault();
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { BusinessUnitId = businessUnitId };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { TeamId = teamId };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest { SiteId = siteId };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
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

			Reader.Load(new[] { personId }).Single()
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateReadModelForTest { AlarmStartTime = "2015-12-11 08:00".Utc() };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistIsAlarm()
		{
			var state = new AgentStateReadModelForTest { IsRuleAlarm = true };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
				.IsRuleAlarm.Should().Be(true);
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

			var outOfAdherence = Reader.Load(new[] {state.PersonId}).Single()
				.OutOfAdherences.Single();

			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}

		[Test]
		public void ShouldUpdateOutOfAdherences()
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

			state = new AgentStateReadModelForTest
			{
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
			};

			Target.Persist(state);

			var outOfAdherences = Reader.Load(new[] { state.PersonId }).Single().OutOfAdherences;

			outOfAdherences.First().StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherences.First().EndTime.Should().Be("2016-06-16 08:10".Utc());
			outOfAdherences.Last().StartTime.Should().Be("2016-06-16 08:20".Utc());
			outOfAdherences.Last().EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistAlarmColor()
		{
			var state = new AgentStateReadModelForTest { AlarmColor = Color.Red.ToArgb() };

			Target.Persist(state);

			Reader.Load(new[] { state.PersonId }).Single()
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

			var shift = Reader.Load(new[] {personId}).Single().Shift.Single();
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

			Reader.LoadForTeams(new[] { teamId })
				.Single()
				.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.Persist(model);

			Target.Delete(personId);

			Reader.Load(new[] { personId }).SingleOrDefault()
				.Should().Be.Null();
		}
	}
}