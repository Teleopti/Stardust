using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Rta.ReadModelUpdaters.ExternalLogons
{
	[TestFixture]
	[DomainTest]
	public class ExternalLogonReadModelUpdaterTest
	{
		public MutableNow Now;
		public ExternalLogonReadModelUpdater Target;
		public FakeExternalLogonReadModelPersister Persister;
		public IKeyValueStorePersister KeyValueStore;

		[Test]
		public void ShouldContainLogon()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode"
					}
				}
			});
			Target.Handle(new TenantMinuteTickEvent());

			var mapping = Persister.Read().Single();
			mapping.PersonId.Should().Be(person);
			mapping.UserCode.Should().Be("usercode");
			mapping.DataSourceId.Should().Be(2);
		}

		[Test]
		public void ShouldContain2Logons()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
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
			Target.Handle(new TenantMinuteTickEvent());

			var mappings = Persister.Read();
			mappings.Select(x => x.UserCode).Should().Have.SameValuesAs("usercode1", "usercode2");
			mappings.Select(x => x.DataSourceId).Should().Have.SameValuesAs(1, 2);
		}

		[Test]
		public void ShouldReplaceLogons()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode1"
					}
				}
			});
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode2"
					}
				}
			});
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read().Single().UserCode.Should().Be("usercode2");
		}

		[Test]
		public void ShouldRemoveWhenNoLogons()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode"
					}
				}
			});
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = Enumerable.Empty<ExternalLogon>()
			});
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveWhenNoLogons2()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 2, UserCode = "usercode" } }
			});
			Target.Handle(new TenantMinuteTickEvent());
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = Enumerable.Empty<ExternalLogon>()
			});
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read().Should().Be.Empty();
		}




		[Test]
		public void ShouldNotContainLogonUntilMinutelyRefresh()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode"
					}
				}
			});

			Persister.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotContain2LogonsUntilMinutelyRefresh()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
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

			Persister.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReplaceLogonsUntilMinutelyRefresh()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode1"
					}
				}
			});
			Target.Handle(new TenantMinuteTickEvent());
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						UserCode = "usercode2"
					}
				}
			});

			Persister.Read().Single().UserCode.Should().Be("usercode1");
		}

		[Test]
		public void ShouldNotRemoveLogonsUntilMinutelyRefresh()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[]
				{
					new ExternalLogon
					{
						DataSourceId = 2,
						UserCode = "usercode"
					}
				}
			});
			Target.Handle(new TenantMinuteTickEvent());
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				ExternalLogons = Enumerable.Empty<ExternalLogon>()
			});

			Persister.Read().Single().UserCode.Should().Be("usercode");
		}

	}

}