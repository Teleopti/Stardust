using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[DatabaseTest]
	public class DatabaseVersionTest
	{
		public DatabaseVersion Target;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void VerifyDatabaseVersionOnExistingRoot()
		{
			var p = PersonFactory.CreatePerson();
			WithUnitOfWork.Do(() => PersonRepository.Add(p));

			WithUnitOfWork.Do(() => Target.FetchFor(p, false)
				.Should().Be.GreaterThan(0));
		}

		[Test]
		public void VerifyDatabaseVersionOnExistingRootUsingPessimisticLock()
		{
			//does not verify that a pess lock is created, just that the method works logically
			//the lock itself will be tested in the use cases where needed
			var p = PersonFactory.CreatePerson();
			WithUnitOfWork.Do(() => PersonRepository.Add(p));

			WithUnitOfWork.Do(() => Target.FetchFor(p, true)
				.Should().Be.GreaterThan(0));
		}


		[Test]
		public void VerifyDatabaseVersionOnNonDatabaseExistingRoot()
		{
			var p = PersonFactory.CreatePerson();
			p.SetId(Guid.NewGuid());
			WithUnitOfWork.Do(() => Target.FetchFor(p, false)
				.Should().Not.Have.Value());
		}

		[Test]
		public void VerifyDatabaseVersionOnTransientRoot()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				WithUnitOfWork.Do(() => Target.FetchFor(PersonFactory.CreatePerson(), false));
			});
		}

		[Test]
		public void VerifyDatabaseVersionOnProxy()
		{
			//CleanUpAfterTest();
			var p = PersonFactory.CreatePerson();
			WithUnitOfWork.Do(() => PersonRepository.Add(p));

			WithUnitOfWork.Do(() =>
			{
				var pProxy = PersonRepository.Load(p.Id.Value);
				Target.FetchFor(pProxy, false)
					.Should().Be.GreaterThan(0);
			});
		}
	}
}