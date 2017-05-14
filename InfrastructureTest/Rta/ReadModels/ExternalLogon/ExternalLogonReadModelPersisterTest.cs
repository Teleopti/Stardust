using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.ExternalLogon
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class ExternalLogonReadModelPersisterTest
	{
		public IExternalLogonReadModelPersister Target;
		public IExternalLogonReader Reader;

		[Test]
		public void ShouldPersistOne()
		{
			Target.Add(new ExternalLogonReadModel());
			Target.Refresh();

			Reader.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistModel()
		{
			var person = Guid.NewGuid();
			Target.Add(new ExternalLogonReadModel
			{
				PersonId = person,
				DataSourceId = 3,
				UserCode = "usercode"
			});
			Target.Refresh();

			var result = Reader.Read().Single();
			result.PersonId.Should().Be(person);
			result.DataSourceId.Should().Be(3);
			result.UserCode.Should().Be("usercode");
		}

		[Test]
		public void ShouldDelete()
		{
			var person = Guid.NewGuid();
			Target.Add(new ExternalLogonReadModel
			{
				PersonId = person,
			});
			Target.Refresh();

			Target.Delete(person);
			Target.Refresh();

			Reader.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReadAddedUntilRefresh()
		{
			Target.Add(new ExternalLogonReadModel());

			Reader.Read().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotRemoveUntilRefresh()
		{
			var person = Guid.NewGuid();
			Target.Add(new ExternalLogonReadModel {PersonId = person });
			Target.Refresh();
			Target.Delete(person);

			Reader.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReadLatestAddedAfterRefresh()
		{
			var person = Guid.NewGuid();
			Target.Add(new ExternalLogonReadModel {PersonId = person, UserCode = "user1"});
			Target.Delete(person);
			Target.Add(new ExternalLogonReadModel {PersonId = person, UserCode = "user2"});
			Target.Delete(person);
			Target.Add(new ExternalLogonReadModel {PersonId = person, UserCode = "user3"});
			Target.Refresh();

			Reader.Read().Single().UserCode.Should().Be("user3");
		}

		[Test]
		public void ShouldReadLatestDeletionAfterRefresh()
		{
			var person = Guid.NewGuid();
			Target.Add(new ExternalLogonReadModel { PersonId = person, UserCode = "user1" });
			Target.Delete(person);
			Target.Add(new ExternalLogonReadModel { PersonId = person, UserCode = "user2" });
			Target.Delete(person);
			Target.Refresh();

			Reader.Read().Should().Be.Empty();
		}

	}
}