using System.Collections.Generic;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class OptionalColumnRepositoryTest : RepositoryTest<IOptionalColumn>
	{
		private OptionalColumnRepository repository;

		protected override void ConcreteSetup()
		{
			repository = new OptionalColumnRepository(UnitOfWork);
		}

		protected override IOptionalColumn CreateAggregateWithCorrectBusinessUnit()
		{
			OptionalColumn opc = new OptionalColumn("OptionalColumn");
			opc.TableName = "Person";
			opc.AvailableAsGroupPage = true;
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
			return new OptionalColumnRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldReturnUniqueValues()
		{
			var col = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("sdgf");
			person1.AddOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person2 = PersonFactory.CreatePerson("s");
			person2.AddOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person3 = PersonFactory.CreatePerson("gg");
			person3.AddOptionalColumnValue(new OptionalColumnValue("VAL2"), col);
			var person4 = PersonFactory.CreatePerson("hgyj");
			person4.AddOptionalColumnValue(new OptionalColumnValue("VAL3"), col);

			PersistAndRemoveFromUnitOfWork(col);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);

			UnitOfWork.PersistAll();
			CleanUpAfterTest();

			var ret = repository.UniqueValuesOnColumn(col.Id.Value);
			Assert.That(ret.Count, Is.EqualTo(3));
			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork));
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
			const string columnName = "Column A";
			var columnA = new OptionalColumn(columnName)
			{
				TableName = "Person",
				AvailableAsGroupPage = true
			};

			PersistAndRemoveFromUnitOfWork(columnA);

			IList<IOptionalColumn> returnList = repository.GetOptionalColumns<Person>();

			Assert.AreEqual(1, returnList.Count);
			Assert.AreEqual(columnName, returnList[0].Name);
			Assert.AreEqual(true, returnList[0].AvailableAsGroupPage);
		}

		[Test]
		public void ShouldReturnValuesForGivenOptionalColumn()
		{
			var optionalColumn1 = CreateAggregateWithCorrectBusinessUnit();
			var optionalColumn2 = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("1");
			person1.AddOptionalColumnValue(new OptionalColumnValue("VAL1"), optionalColumn1);
			var person2 = PersonFactory.CreatePerson("2");
			person2.AddOptionalColumnValue(new OptionalColumnValue("VAL2"), optionalColumn1);
			var person3 = PersonFactory.CreatePerson("3");
			person3.AddOptionalColumnValue(new OptionalColumnValue("VAL3"), optionalColumn2);

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
	}
}
