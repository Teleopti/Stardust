using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterExternalLogonPersonMappingTest
	{
		public IAgentStatePersister Target;
		public ICurrentUnitOfWork UnitOfWork;

		[Test]
		public void ShouldPrepareForEachUserCode()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 3,
						UserCode = "usercode1"
					},
					new ExternalLogon
					{
						DataSourceId = 4,
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {DataSourceId = 3, UserCode = "usercode1"}).Single().PersonId.Should().Be(person);
			Target.ReadForTest(new ExternalLogon {DataSourceId = 4, UserCode = "usercode2"}).Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldGetForMultiplePersons()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person1,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 7,
						UserCode = "usercode"
					}
				}
			}, DeadLockVictim.Yes);
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person2,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 7,
						UserCode = "usercode"
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {DataSourceId = 7, UserCode = "usercode"}).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldRemoveRemovedUserCode()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercodeA"
					},
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercodeA"
					},
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercodeB"
					},
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercodeB"
					},
				}
			}, DeadLockVictim.Yes);
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercodeA"
					},
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercodeB"
					},
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {DataSourceId = 1, UserCode = "usercodeA"}).Should().Have.Count.EqualTo(0);
			Target.ReadForTest(new ExternalLogon {DataSourceId = 2, UserCode = "usercodeA"}).Should().Have.Count.EqualTo(1);
			Target.ReadForTest(new ExternalLogon {DataSourceId = 1, UserCode = "usercodeB"}).Should().Have.Count.EqualTo(1);
			Target.ReadForTest(new ExternalLogon {DataSourceId = 2, UserCode = "usercodeB"}).Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShuoldRemoveAllWithNoLogons()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercodeA"
					}
				}
			}, DeadLockVictim.Yes);
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {UserCode = "usercodeA" })
				.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldGetOneByPersonId()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode1"
					},
					new ExternalLogon
					{
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode2" })
				.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldGetOneForEachPersonId()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode1"
					},
					new ExternalLogon
					{
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {UserCode = "usercode2"})
				.Single().PersonId.Should().Be(person);
		}
		
		[Test]
		public void ShouldUpdateOnMultipleUserCodes()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercode1"
					},
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);
			Target.Update(new AgentState
			{
				PersonId = person,
				StateCode = "statecode"
			});

			Target.ReadForTest(new ExternalLogon {DataSourceId = 1, UserCode = "usercode1"}).Single().StateCode.Should().Be("statecode");
			Target.ReadForTest(new ExternalLogon {DataSourceId = 2, UserCode = "usercode2"}).Single().StateCode.Should().Be("statecode");
		}

		[Test]
		public void ShouldKeepStateWhenChangingExternalLogon()
		{
			var person = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "usercode1"
					},
				}
			}, DeadLockVictim.Yes);
			Target.Update(new AgentState
			{
				PersonId = person,
				StateCode = "code",
				ReceivedTime = "2016-09-14".Utc()
			});
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon {DataSourceId = 2, UserCode = "usercode2"}).Single().StateCode.Should().Be("code");
			Target.ReadForTest(new ExternalLogon {DataSourceId = 2, UserCode = "usercode2"}).Single().ReceivedTime.Should().Be("2016-09-14".Utc());
		}





		[Test]
		public void ShouldGetOneForAllPersonIdsNotInSnapshot()
		{
			var person = Guid.NewGuid();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode1"
					},
					new ExternalLogon
					{
						UserCode = "usercode2"
					}
				}
			}, DeadLockVictim.Yes);
			Target.Update(new AgentState
			{
				PersonId = person,
				BatchId = "2016-08-29 13:00".Utc(),
				SourceId = "source"
			});

			var logons = Target.FindForClosingSnapshot("2016-08-29 13:01".Utc(), "source", "loggedout");
			Target.ReadForTest(logons)
				.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFindMany()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var usercodeA = RandomName.Make();
			var usercodeB = RandomName.Make();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person1,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeA
					},
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = usercodeA
					}
				}
			}, DeadLockVictim.Yes);
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person2,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeB
					}
				}
			}, DeadLockVictim.Yes);

			Target.ReadForTest(new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeB
					},
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeA
					}
				})
				.Select(x => x.PersonId)
				.OrderBy(x => x)
				.Should().Have.SameSequenceAs(new [] { person1, person2}.OrderBy(x => x));
		}

		[Test]
		public void ShouldFindAllExternalLogons()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var usercodeA = RandomName.Make();
			var usercodeB = RandomName.Make();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person1,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeA
					},
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = usercodeA
					}
				}
			}, DeadLockVictim.No);
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person2,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = usercodeB
					}
				}
			}, DeadLockVictim.No);

			var result = Target.FindForCheck();
			(result.Any(x => x.DataSourceId == 1 && x.UserCode == usercodeA) ^ 
				result.Any(x => x.DataSourceId == 2 && x.UserCode == usercodeA))
				.Should().Be.True();
			result.Any(x => x.DataSourceId == 1 && x.UserCode == usercodeB)
				.Should().Be.True();
		}

		[Test]
		public void ShouldRemoveLegacyData()
		{
			var person = Guid.NewGuid();
			UnitOfWork.Current().FetchSession()
				.CreateSQLQuery(@"INSERT INTO [dbo].[AgentState] (PersonId, DataSourceIdUserCode) VALUES (:PersonId, '')")
				.SetParameter("PersonId", person)
				.ExecuteUpdate();

			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 1,
						UserCode = "user"
					}
				}
			}, DeadLockVictim.Yes);

			UnitOfWork.Current().FetchSession()
				.CreateSQLQuery(@"SELECT COUNT(*) FROM [dbo].[AgentState] WHERE DataSourceIdUserCode = ''")
				.UniqueResult<int>().Should().Be(0);
		}
	}
}