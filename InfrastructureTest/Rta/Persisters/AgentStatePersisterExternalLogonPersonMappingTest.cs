using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterExternalLogonPersonMappingTest2
	{
		public IAgentStatePersister Target;

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
			});

			Target.Find(3, "usercode1").Single().PersonId.Should().Be(person);
			Target.Find(4, "usercode2").Single().PersonId.Should().Be(person);
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
			});
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
			});

			Target.Find(7, "usercode").Should().Have.Count.EqualTo(2);
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
			});
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
			});

			Target.Find(1, "usercodeA").Should().Have.Count.EqualTo(0);
			Target.Find(2, "usercodeA").Should().Have.Count.EqualTo(1);
			Target.Find(1, "usercodeB").Should().Have.Count.EqualTo(1);
			Target.Find(2, "usercodeB").Should().Have.Count.EqualTo(0);
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
			});
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = person
			});

			Target.Get(person).Should().Be.Null();
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
			});

			Target.Get(person).PersonId.Should().Be(person);
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
			});

			Target.Get(new[] {person}).Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldGetOneForAllPersonIds()
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
			});

			Target.GetStates().Single().PersonId.Should().Be(person);
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
			});
			Target.Update(new AgentState
			{
				PersonId = person,
				StateCode = "statecode"
			});

			Target.Find(1, "usercode1").Single().StateCode.Should().Be("statecode");
			Target.Find(2, "usercode2").Single().StateCode.Should().Be("statecode");
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
			});
			Target.Update(new AgentState
			{
				PersonId = person,
				BatchId = "2016-08-29 13:00".Utc(),
				SourceId = "source"
			});

			Target.GetStatesNotInSnapshot("2016-08-29 13:01".Utc(), "source")
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
			});
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
			});

			Target.Find(1, new[] {usercodeA, usercodeB})
				.Select(x => x.UserCode)
				.Should().Have.SameSequenceAs(usercodeA, usercodeB);
		}
	}
}