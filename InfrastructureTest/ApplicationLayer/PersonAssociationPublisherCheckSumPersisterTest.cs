using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[UnitOfWorkTest]
	public class PersonAssociationPublisherCheckSumPersisterTest
	{
		public IPersonAssociationPublisherCheckSumPersister Persister;

		[Test]
		public void ShouldPersist()
		{
			var person = Guid.NewGuid();
			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person,
				CheckSum = 123
			});

			Persister.Get(person).CheckSum.Should().Be(123);
		}

		[Test]
		public void ShouldUpdate()
		{
			var person = Guid.NewGuid();
			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person,
				CheckSum = 123
			});

			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person,
				CheckSum = 234
			});
			Persister.Get(person).CheckSum.Should().Be(234);
		}

		[Test]
		public void ShouldGetAll()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person1,
				CheckSum = 123
			});

			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person2,
				CheckSum = 234
			});

			var checkSums = Persister.Get().ToLookup(p => p.PersonId);
			checkSums[person1].Single().CheckSum.Should().Be(123);
			checkSums[person2].Single().CheckSum.Should().Be(234);
		}

		[Test]
		public void ShouldReturnNullWhenItDoesntExist()
		{
			Persister.Get(Guid.NewGuid()).Should().Be.Null();
		}

		[Test]
		public void ShouldDeleteWhenNoCheckSum()
		{
			var person = Guid.NewGuid();

			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person,
				CheckSum = 123
			});
			Persister.Persist(new PersonAssociationCheckSum
			{
				PersonId = person,
				CheckSum = 0
			});

			Persister.Get(person).Should().Be.Null();
		}


	}
}