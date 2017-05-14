using System;
using System.Drawing;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
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

			Target.PersistWithAssociation(state);

			Target.Load(state.PersonId)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithProperties()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				TeamId = teamId,
				IsRuleAlarm = true,
				AlarmStartTime = "2015-12-11 08:00".Utc(),
				AlarmColor = Color.Red.ToArgb(),
			};

			Target.PersistWithAssociation(state);

			var model = Target.Load(personId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.TeamId.Should().Be(teamId);
			model.SiteId.Should().Be(siteId);
			model.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
			model.IsRuleAlarm.Should().Be(true);
			model.AlarmColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldPersistShift()
		{
			var personId = Guid.NewGuid();

			Target.PersistWithAssociation(new AgentStateReadModelForTest
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

			var shift = Target.Load(personId).Shift.Single();
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
			Target.PersistWithAssociation(
				new AgentStateReadModelForTest
				{
					PersonId = personId,
					TeamId = teamId,
				});

			Target.PersistWithAssociation(new AgentStateReadModelForTest
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

			Target.Load(personId)
				.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldUpdateLargeShift()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Target.PersistWithAssociation(
				new AgentStateReadModelForTest
				{
					PersonId = personId,
					TeamId = teamId,
				});

			Target.PersistWithAssociation(new AgentStateReadModelForTest
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

			Target.Load(personId).Shift.Should().Have.Count.EqualTo(100);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.PersistWithAssociation(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				SiteId = null,
				SiteName = null,
				TeamId = null,
				TeamName = null,

				ReceivedTime = "2015-01-02 10:00".Utc(),
				
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

			Target.Load(personId)
				.Should().Not.Be.Null();
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

			Target.PersistWithAssociation(state);

			var outOfAdherence = Target.Load(state.PersonId)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}

		[Test]
		public void ShouldUpdateOutOfAdherences()
		{
			var personId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest
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

			Target.PersistWithAssociation(new AgentStateReadModelForTest
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

			var outOfAdherences = Target.Load(personId).OutOfAdherences.ToArray();
			outOfAdherences.First().StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherences.First().EndTime.Should().Be("2016-06-16 08:10".Utc());
			outOfAdherences.Last().StartTime.Should().Be("2016-06-16 08:20".Utc());
			outOfAdherences.Last().EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldUpdateAlotOfOutOfAdherences()
		{
			var personId = Guid.NewGuid();

			Target.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = personId,
				OutOfAdherences = Enumerable.Range(0, 59)
				.Select(m => new AgentStateOutOfAdherenceReadModel
				{
					StartTime = $"2016-06-16 08:{m:00}".Utc(),
					EndTime = $"2016-06-16 08:{m+1:00}".Utc()
				})
			});
			
			Target.Load(personId).OutOfAdherences.Should().Have.Count.EqualTo(59);
		}

		[Test]
		public void ShouldPersistStateGroupId()
		{
			var personId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();

			Target.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = stateGroupId
			});

			Target.Load(personId).StateGroupId.Should().Be(stateGroupId);
		}

		[Test]
		public void ShouldUpdateStateGroupId()
		{
			var personId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = null
			});

			Target.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = personId,
				StateGroupId = stateGroupId
			});

			Target.Load(personId).StateGroupId.Should().Be(stateGroupId);
		}

		[Test]
		public void ShouldUpdatePersonAssociation()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest { PersonId = personId });

			Target.UpsertAssociation(new AssociationInfo()
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				SiteName = "site",
				TeamId = teamId,
				TeamName = "team"
			});

			var result = Target.Load(personId);
			result.SiteId.Should().Be(siteId);
			result.SiteName.Should().Be("site");
			result.TeamId.Should().Be(teamId);
			result.TeamName.Should().Be("team");
			result.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldInsertPersonAssociation()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();

			Target.UpsertAssociation(new AssociationInfo()
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				SiteName = "site",
				TeamId = teamId,
				TeamName = "team"
			});

			var result = Target.Load(personId);
			result.SiteId.Should().Be(siteId);
			result.SiteName.Should().Be("site");
			result.TeamId.Should().Be(teamId);
			result.TeamName.Should().Be("team");
			result.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldUnSoftDeleteWhenUpdatingPersonAssociation()
		{
			var personId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest { PersonId = personId });
			Target.UpsertDeleted(personId, "2016-10-05 08:00".Utc());

			Target.UpsertAssociation(new AssociationInfo()
			{
				PersonId = personId
			});

			var result = Target.Load(personId);
			result.IsDeleted.Should().Be.False();
			result.ExpiresAt.Should().Be(null);
		}

		[Test]
		public void ShouldUpdateDeleted()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.PersistWithAssociation(model);

			Target.UpsertDeleted(personId, "2016-10-04 08:00".Utc());

			var result = Target.Load(personId);
			result.IsDeleted.Should().Be(true);
			result.ExpiresAt.Should().Be("2016-10-04 08:00".Utc());
		}

		[Test]
		public void ShouldInsertDeleted()
		{
			var personId = Guid.NewGuid();

			Target.UpsertDeleted(personId, "2016-10-04 08:00".Utc());

			var result = Target.Load(personId);
			result.IsDeleted.Should().Be(true);
			result.ExpiresAt.Should().Be("2016-10-04 08:00".Utc());
		}


		[Test]
		public void ShouldUpdateEmploymentNumber()
		{
			var personId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest() { PersonId = personId, IsDeleted = false });

			Target.UpsertEmploymentNumber(personId, "abc", "2017-02-14 08:00".Utc());

			Target.Load(personId).EmploymentNumber.Should().Be("abc");
			Target.Load(personId).IsDeleted.Should().Be(false);
			Target.Load(personId).ExpiresAt.Should().Be(null);
		}

		[Test]
		public void ShouldInsertEmploymentNumber()
		{
			var personId = Guid.NewGuid();

			Target.UpsertEmploymentNumber(personId, "123", "2017-02-14 08:00".Utc());

			var model = Target.Load(personId);
			model.EmploymentNumber.Should().Be("123");
			model.IsDeleted.Should().Be(true);
			model.ExpiresAt.Should().Be("2017-02-14 08:00".Utc());
		}

		[Test]
		public void ShouldInsertFirstAndLastName()
		{
			var personId = Guid.NewGuid();

			Target.UpsertName(personId, "bill", "gates", "2017-02-14 08:00".Utc());

			var model = Target.Load(personId);
			model.FirstName.Should().Be("bill");
			model.LastName.Should().Be("gates");
			model.IsDeleted.Should().Be(true);
			model.ExpiresAt.Should().Be("2017-02-14 08:00".Utc());
		}

		[Test]
		public void ShouldUpdateFirstAndLastName()
		{
			var personId = Guid.NewGuid();
			Target.PersistWithAssociation(new AgentStateReadModelForTest()
			{
				PersonId = personId,
				FirstName = "ashley",
				LastName = "andeen",
				IsDeleted = false
			});

			Target.UpsertName(personId, "bill", "gates", "2017-02-14 0:00".Utc());

			var model = Target.Load(personId);
			model.FirstName.Should().Be("bill");
			model.LastName.Should().Be("gates");
			model.IsDeleted.Should().Be(false);
			model.ExpiresAt.Should().Be(null);
		}


		[Test]
		public void ShouldUpdateTeamName()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Target.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				TeamId = teamId,
				TeamName = "team students"
			});

			Target.UpdateTeamName(teamId, "team preferences");

			Target.Load(personId).TeamName.Should().Be("team preferences");
		}

		[Test]
		public void ShouldUpdateSiteName()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Target.UpsertAssociation(new AssociationInfo
			{
				PersonId = personId,
				SiteId = siteId,
				SiteName = "london"
			});

			Target.UpdateSiteName(siteId, "paris");

			Target.Load(personId).SiteName.Should().Be("paris");
		}

		[Test]
		public void ShouldRemoveOldRows()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.PersistWithAssociation(model);
			Target.UpsertDeleted(personId, "2016-10-04 08:30".Utc());
			
			Target.DeleteOldRows("2016-10-04 08:30".Utc());

			Target.Load(personId).Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveOlderRows()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.PersistWithAssociation(model);
			Target.UpsertDeleted(personId, "2016-10-04 08:30".Utc());

			Target.DeleteOldRows("2016-10-04 09:00".Utc());

			Target.Load(personId).Should().Be.Null();
		}
		
		[Test]
		public void ShouldRemoveLotsOfRowsAtATime()
		{
			var personIds = Enumerable
				.Range(1, 110)
				.Select(x => Guid.NewGuid())
				.Select(personId =>
				{
					var model = new AgentStateReadModelForTest { PersonId = personId };
					Target.PersistWithAssociation(model);
					Target.UpsertDeleted(personId, "2016-10-04 08:29".Utc());
					return personId;
				})
				.ToArray();

			Target.DeleteOldRows("2016-10-04 09:00".Utc());

			personIds
				.ForEach(personId =>
				Target.Load(personId).Should().Be.Null());
		}


	}
}