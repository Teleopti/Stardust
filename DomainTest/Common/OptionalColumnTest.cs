 using System;
 using NUnit.Framework;
 using Teleopti.Ccc.Domain.Common;
 using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OptionalColumnTest
    {
        private OptionalColumn optionalColumn;

        [SetUp]
        public void Setup()
        {
            optionalColumn = new OptionalColumn("OptionalColumn");
        }

        [Test]
        public void VerifyCanCreate()
        {
            string columnName = "OptionalColumn";
            optionalColumn = new OptionalColumn(columnName);

            Assert.AreEqual(columnName, optionalColumn.Name);
            Assert.AreEqual(0, optionalColumn.ValueCollection.Count);
        }

        [Test]
        public void VerifyRootIsAggregateRoot()
        {
            string columnName = "OptionalColumn";
            optionalColumn = new OptionalColumn(columnName);
            
            OptionalColumnValue columnValue = new OptionalColumnValue("ColumnValue");
            optionalColumn.AddOptionalColumnValue(columnValue);

            Assert.AreSame(optionalColumn, ((IAggregateEntity)columnValue).Root());
            Assert.AreSame(optionalColumn, columnValue.Parent);
        }

        [Test]
        public void VerifyNameCanSet()
        {
            string columnName = "OptionalColumn";
            optionalColumn = new OptionalColumn(columnName);
            optionalColumn.Name = columnName;
            Assert.AreEqual(columnName, optionalColumn.Name );
        }

        [Test]
        public void VerifyTableNameCanSet()
        {
            string columnName = "OptionalColumn";
            string tableName = "Person";

            optionalColumn = new OptionalColumn(columnName);
            optionalColumn.TableName = tableName;

            Assert.AreEqual(tableName, optionalColumn.TableName);
        }

        [Test]
        public void VerifyAddOptionalColumnValue()
        {
            string columnName = "OptionalColumn";
            string tableName = "Person";
            string colValue = "ColumnValue";

            optionalColumn = new OptionalColumn(columnName);
            optionalColumn.TableName = tableName;

            OptionalColumnValue columnValue = new OptionalColumnValue(colValue);
            optionalColumn.AddOptionalColumnValue(columnValue);

            Assert.AreEqual(1, optionalColumn.ValueCollection.Count);
            Assert.AreEqual(colValue, optionalColumn.ValueCollection[0].Description);
        }

        [Test]
        public void VerifyRemoveOptionalColumnValue()
        {
            string columnName = "OptionalColumn";
            string tableName = "Person";
            string colValue = "ColumnValue";
            string colValueTwo = "ColumnValue";

            optionalColumn = new OptionalColumn(columnName);
            optionalColumn.TableName = tableName;

            OptionalColumnValue columnValue = new OptionalColumnValue(colValue);
            optionalColumn.AddOptionalColumnValue(columnValue);

            OptionalColumnValue columnValueTwo = new OptionalColumnValue(colValueTwo);
            optionalColumn.AddOptionalColumnValue(columnValueTwo);

            Assert.AreEqual(2, optionalColumn.ValueCollection.Count);

            optionalColumn.RemoveOptionalColumnValue(columnValueTwo);

            Assert.AreEqual(1, optionalColumn.ValueCollection.Count);
            Assert.AreEqual(colValue, optionalColumn.ValueCollection[0].Description);
        }

        [Test]
        public void VerifyGetColumnValueById()
        {
            Guid? idOne = Guid.NewGuid();
            Guid? idTwo = Guid.NewGuid();

            string columnName = "OptionalColumn";
            string tableName = "Person";
            string colValue = "ColumnValue";
            string colValueTwo = "ColumnValue";

            optionalColumn = new OptionalColumn(columnName);
            optionalColumn.TableName = tableName;

            OptionalColumnValue columnValue = new OptionalColumnValue(colValue);
            columnValue.ReferenceId = idOne;
            optionalColumn.AddOptionalColumnValue(columnValue);

            OptionalColumnValue columnValueTwo = new OptionalColumnValue(colValueTwo);
            columnValueTwo.ReferenceId = idTwo;
            optionalColumn.AddOptionalColumnValue(columnValueTwo);

            IOptionalColumnValue returnValue = optionalColumn.GetColumnValueById(idTwo);
            Assert.AreEqual(columnValueTwo, returnValue);
        }
    }
}
