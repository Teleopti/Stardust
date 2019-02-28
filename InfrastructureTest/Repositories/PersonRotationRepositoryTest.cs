using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	///<summary>
	/// Tests PersonRotationRepository
	///</summary>
	[TestFixture]
	[Category("BucketB")]
	public class PersonRotationRepositoryTest : RepositoryTest<IPersonRotation>
	{
		private IPerson _person;
		private IRotation _rotation;
		private DateOnly _startDate;
		private Rotation _rotation2;
		private Rotation _rotation3;
		private Rotation _rotation4;

		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
			_person = PersonFactory.CreatePerson("bopsd");
			_rotation = new Rotation("Two weeks 1", 2 * 7);
			_rotation2 = new Rotation("Two weeks 2", 2 * 7);
			_rotation3 = new Rotation("Two weeks 3", 2 * 7);
			_rotation4 = new Rotation("Two weeks 4", 2 * 7);
			_startDate = new DateOnly(2008, 6, 30);

			PersistAndRemoveFromUnitOfWork(_person);
			PersistAndRemoveFromUnitOfWork(_rotation);
			PersistAndRemoveFromUnitOfWork(_rotation2);
			PersistAndRemoveFromUnitOfWork(_rotation3);
			PersistAndRemoveFromUnitOfWork(_rotation4);
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override IPersonRotation CreateAggregateWithCorrectBusinessUnit()
		{
			IPersonRotation personRotation = new PersonRotation(_person, _rotation, _startDate, 2);

			return personRotation;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IPersonRotation loadedAggregateFromDatabase)
		{
			IPersonRotation org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
			Assert.AreEqual(org.StartDate, loadedAggregateFromDatabase.StartDate);
			Assert.AreEqual(org.StartDay, loadedAggregateFromDatabase.StartDay);
			Assert.AreEqual(org.Rotation, loadedAggregateFromDatabase.Rotation);
		}

		protected override Repository<IPersonRotation> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return PersonRotationRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
		}

		[Test]
		public void VerifyCanGetPersonRotationByPerson()
		{
			IPersonRotation newPersonRotation1 = CreateAggregateWithCorrectBusinessUnit();
			IPerson person2 = PersonFactory.CreatePerson("Person2");
			IPersonRotation newPersonRotation2 = new PersonRotation(person2, _rotation, _startDate, 1);

			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(newPersonRotation1);
			PersistAndRemoveFromUnitOfWork(newPersonRotation2);

			IList<IPersonRotation> personRotations = PersonRotationRepository.DONT_USE_CTOR(UnitOfWork).Find(_person);

			Assert.AreEqual(1, personRotations.Count);
			Assert.AreEqual(newPersonRotation1, personRotations[0]);
		}

		[Test]
		public void VerifyCanGetPersonRotationByPersonList()
		{
			IPersonRotation newPersonRotation1 = CreateAggregateWithCorrectBusinessUnit();
			IPerson person2 = PersonFactory.CreatePerson("Person2");
			IPersonRotation newPersonRotation2 = new PersonRotation(person2, _rotation, _startDate, 1);

			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(newPersonRotation1);
			PersistAndRemoveFromUnitOfWork(newPersonRotation2);

			IList<IPersonRotation> personRotations = PersonRotationRepository.DONT_USE_CTOR(UnitOfWork).Find(new List<IPerson> { _person });

			Assert.AreEqual(1, personRotations.Count);
			Assert.AreEqual(newPersonRotation1, personRotations[0]);



		}

		[Test]
		public void VerifyLoadWithoutLazyLoading()
		{
			IPerson person2 = PersonFactory.CreatePerson("Person2");
			IPersonRotation newPersonRotation1 = CreateAggregateWithCorrectBusinessUnit();
			IPersonRotation newPersonRotation2 = new PersonRotation(_person, _rotation2, _startDate.AddDays(7), 1);
			IPersonRotation newPersonRotation3 = new PersonRotation(_person, _rotation3, _startDate.AddDays(21), 1);
			IPersonRotation newPersonRotation4 = new PersonRotation(person2, _rotation4, _startDate.AddDays(21), 1);

			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(newPersonRotation1);
			PersistAndRemoveFromUnitOfWork(newPersonRotation2);
			PersistAndRemoveFromUnitOfWork(newPersonRotation3);
			PersistAndRemoveFromUnitOfWork(newPersonRotation4);

			var testList = PersonRotationRepository.DONT_USE_CTOR(UnitOfWork).LoadPersonRotationsWithHierarchyData(new[] { _person }, _startDate.AddDays(14));

			var rotations = RotationRepository.DONT_USE_CTOR(UnitOfWork).LoadRotationsWithHierarchyData(new[] { _person }, _startDate.AddDays(14));

			Assert.That(rotations.Count(), Is.EqualTo(2));
			rotations = RotationRepository.DONT_USE_CTOR(UnitOfWork).LoadRotationsWithHierarchyData(new[] { person2 }, _startDate.AddDays(14));
			Assert.That(rotations.Count(), Is.EqualTo(1));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList.First().Rotation));
		}

		[Test]
		public void VerifyPersonCannotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => PersonRotationRepository.DONT_USE_CTOR(UnitOfWork).Find((IPerson)null));
		}

		[Test]
		public void VerifyPersonsCannotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => PersonRotationRepository.DONT_USE_CTOR(UnitOfWork).Find((IList<IPerson>)null));
		}
	}
}