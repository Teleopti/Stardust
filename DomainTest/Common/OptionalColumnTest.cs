using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class OptionalColumnTest
    {
        private OptionalColumn _optionalColumn;
    	private string _columnName;

    	[SetUp]
        public void Setup()
    	{
    		_columnName = "OptionalColumn";
			_optionalColumn = new OptionalColumn(_columnName);
        }

        [Test]
        public void VerifyCanCreate()
        {
            Assert.AreEqual(_columnName, _optionalColumn.Name);
        }

        [Test]
        public void VerifyNameCanSet()
        {
            _optionalColumn = new OptionalColumn(_columnName) {Name = _columnName};
        	Assert.AreEqual(_columnName, _optionalColumn.Name );
        }

        [Test]
        public void VerifyTableNameCanSet()
        {
            const string tableName = "Person";
        	_optionalColumn = new OptionalColumn(_columnName) {TableName = tableName};
        	Assert.AreEqual(tableName, _optionalColumn.TableName);
        }

		[Test]
		public void VerifyAvailableAsGroupPageCanSet()
		{
			_optionalColumn = new OptionalColumn(_columnName)
			{
				Name = _columnName,
				AvailableAsGroupPage = true
			};
			Assert.AreEqual(true, _optionalColumn.AvailableAsGroupPage);
		}
	}
}
