using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class OptionalColumnRepositoryTest : RepositoryTest<IOptionalColumn>
	{
		private OptionalColumnRepository repository;

		protected override void ConcreteSetup()
		{
			repository = OptionalColumnRepository.DONT_USE_CTOR(UnitOfWork);
		}

		protected override IOptionalColumn CreateAggregateWithCorrectBusinessUnit()
		{
			var opc = new OptionalColumn("OptionalColumn")
			{
				TableName = "Person",
				AvailableAsGroupPage = true
			};
			return opc;
		}

		protected override void VerifyAggregateGraphProperties(IOptionalColumn loadedAggregateFromDatabase)
		{
			IOptionalColumn opc = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(opc.Name, loadedAggregateFromDatabase.Name);
			Assert.AreEqual(opc.TableName, loadedAggregateFromDatabase.TableName);
			Assert.AreEqual(opc.AvailableAsGroupPage, loadedAggregateFromDatabase.AvailableAsGroupPage);
		}

		protected override Repository<IOptionalColumn> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return OptionalColumnRepository.DONT_USE_CTOR(currentUnitOfWork);
		}

		[Test]
		public void ShouldReturnUniqueValues()
		{
			var col = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("sdgf");
			person1.SetOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person2 = PersonFactory.CreatePerson("s");
			person2.SetOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person3 = PersonFactory.CreatePerson("gg");
			person3.SetOptionalColumnValue(new OptionalColumnValue("VAL2"), col);
			var person4 = PersonFactory.CreatePerson("hgyj");
			person4.SetOptionalColumnValue(new OptionalColumnValue("VAL3"), col);

			PersistAndRemoveFromUnitOfWork(col);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);

			UnitOfWork.PersistAll();
			CleanUpAfterTest();

			var ret = repository.UniqueValuesOnColumn(col.Id.Value);
			Assert.That(ret.Count, Is.EqualTo(3));
			var personRep = PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(UnitOfWork), null, null);
			personRep.Remove(person1);
			personRep.Remove(person2);
			personRep.Remove(person3);
			personRep.Remove(person4);
			repository.Remove(col);
			UnitOfWork.PersistAll();
		}


		[Test]
		public void ShouldReturnUniqueValuesExcludeValueContainingDeletedPersonOnly()
		{
			var col = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("sdgf");
			person1.SetOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person2 = PersonFactory.CreatePerson("s");
			person2.SetOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person3 = PersonFactory.CreatePerson("gg");
			person3.SetOptionalColumnValue(new OptionalColumnValue("VAL2"), col);
			var person4 = PersonFactory.CreatePerson("hgyj");
			(person4 as Person).SetDeleted();
			person4.SetOptionalColumnValue(new OptionalColumnValue("VAL3"), col);

			PersistAndRemoveFromUnitOfWork(col);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);

			UnitOfWork.PersistAll();
			CleanUpAfterTest();

			var ret = repository.UniqueValuesOnColumnWithValidPerson(col.Id.Value);
			Assert.That(ret.Count, Is.EqualTo(2));
			Assert.That(ret.Where(x => x.Description == "VAL3"), Is.Empty);
			var personRep = PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(UnitOfWork), null, null);
			personRep.Remove(person1);
			personRep.Remove(person2);
			personRep.Remove(person3);
			personRep.Remove(person4);
			repository.Remove(col);
			UnitOfWork.PersistAll();
		}

		[Test]
		public void VerifyGetOptionalColumns()
		{
			var column = CreateAggregateWithCorrectBusinessUnit();

			PersistAndRemoveFromUnitOfWork(column);

			IList<IOptionalColumn> returnList = repository.GetOptionalColumns<Person>();

			Assert.AreEqual(1, returnList.Count);
			Assert.AreEqual(column.Name, returnList[0].Name);
			Assert.AreEqual(true, returnList[0].AvailableAsGroupPage);
		}

		[Test]
		public void ShouldReturnValuesForGivenOptionalColumn()
		{
			var optionalColumn1 = CreateAggregateWithCorrectBusinessUnit();
			var optionalColumn2 = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("1");
			person1.SetOptionalColumnValue(new OptionalColumnValue("VAL1"), optionalColumn1);
			var person2 = PersonFactory.CreatePerson("2");
			person2.SetOptionalColumnValue(new OptionalColumnValue("VAL2"), optionalColumn1);
			var person3 = PersonFactory.CreatePerson("3");
			person3.SetOptionalColumnValue(new OptionalColumnValue("VAL3"), optionalColumn2);

			PersistAndRemoveFromUnitOfWork(optionalColumn1);
			PersistAndRemoveFromUnitOfWork(optionalColumn2);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);


			var ret = repository.OptionalColumnValues(optionalColumn1);
			Assert.That(ret.Count, Is.EqualTo(2));
			Assert.IsTrue(ret.Contains(person1.OptionalColumnValueCollection[0]));
			Assert.IsTrue(ret.Contains(person2.OptionalColumnValueCollection[0]));
		}

		[Test]
		public void ShouldNotReturnEmptyValuesForGivenOptionalColumn()
		{
			var optionalColumn1 = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("1");
			person1.SetOptionalColumnValue(new OptionalColumnValue(string.Empty), optionalColumn1);

			PersistAndRemoveFromUnitOfWork(optionalColumn1);
			PersistAndRemoveFromUnitOfWork(person1);


			var ret = repository.OptionalColumnValues(optionalColumn1);
			Assert.IsEmpty(ret);
		}
	}
}
