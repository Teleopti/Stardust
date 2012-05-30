#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

    /// <summary>
    /// Represents a OptionalColumnRepositoryTest
    /// </summary>

    [TestFixture]
    [Category("LongRunning")]
    public class OptionalColumnRepositoryTest : RepositoryTest<IOptionalColumn>
    {

        #region Fields - Instance Member

        private OptionalColumnRepository repository;

        #endregion

        protected override void ConcreteSetup()
        {
            repository = new OptionalColumnRepository(UnitOfWork);
        }

        protected override IOptionalColumn CreateAggregateWithCorrectBusinessUnit()
        {
            OptionalColumn opc = new OptionalColumn("OptionalColumn");
            opc.TableName = "Person";           
            return opc;
        }

        protected override void VerifyAggregateGraphProperties(IOptionalColumn loadedAggregateFromDatabase)
        {
            IOptionalColumn opc = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(opc.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(opc.TableName, loadedAggregateFromDatabase.TableName);
           // Assert.AreEqual(0, loadedAggregateFromDatabase.ValueCollection.Count);
        }

        protected override Repository<IOptionalColumn> TestRepository(IUnitOfWork unitOfWork)
        {
            return new OptionalColumnRepository(unitOfWork);
        }

		[Test]
		public void ShouldReturnUniqueValues()
		{
			var rep = new OptionalColumnRepository(UnitOfWork);
			var col = CreateAggregateWithCorrectBusinessUnit();
			IPerson person1 = PersonFactory.CreatePerson("sdgf");
			person1.AddOptionalColumnValue(new OptionalColumnValue("VAL1"),col );
			var person2 = PersonFactory.CreatePerson("s");
			person2.AddOptionalColumnValue(new OptionalColumnValue("VAL1"), col);
			var person3 =  PersonFactory.CreatePerson("gg");
			person3.AddOptionalColumnValue(new OptionalColumnValue("VAL2"), col);
			var person4 = PersonFactory.CreatePerson("hgyj");
			person4.AddOptionalColumnValue(new OptionalColumnValue("VAL3"), col);

			PersistAndRemoveFromUnitOfWork(col);
			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);
			
			UnitOfWork.PersistAll();
			SkipRollback();

			
			var ret = rep.UniqueValuesOnColumn(col.Id.Value);
			Assert.That(ret.Count,Is.EqualTo(3));
		}

		//[Test]
		//public void VerifyGetOptionalColumnValues()
		//{
		//    string columnName = "Column A";
		//    string columnValue = "Value of Column A";

		//    Guid guid = Guid.NewGuid();

		//    OptionalColumn columnA = new OptionalColumn(columnName);
		//    columnA.TableName = "Person";

		//    OptionalColumnValue columnAValue = new OptionalColumnValue(columnValue);
		//    columnAValue.ReferenceId = guid;

		//    columnA.AddOptionalColumnValue(columnAValue);

		//    PersistAndRemoveFromUnitOfWork(columnA);
 
		//    IList<IOptionalColumn> returnList = repository.GetOptionalColumnValues<Person>();
            
		//    Assert.AreEqual(1, returnList.Count);
		//    Assert.AreEqual(columnName, returnList[0].Name);
		//    //Assert.AreEqual(1, returnList[0].ValueCollection.Count);
		//   // Assert.AreEqual(columnValue, returnList[0].ValueCollection[0].Description);
		//   // Assert.AreEqual(guid, returnList[0].ValueCollection[0].ReferenceId);
		//}
    }
}
