using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class AuthorizationEntityExtenderTest
    {

        private IList<IAuthorizationEntity> _inputList;
        private IList<IAuthorizationEntity> _foreignEntityList;
        private IAuthorizationEntity _firstItem;

        [SetUp]
        public void Setup()
        {
            _inputList = AuthorizationObjectFactory.CreateAuthorizationEntityList();
            _foreignEntityList = AuthorizationObjectFactory.CreateAuthorizationForeignEntityList();
            _firstItem = _inputList[0];
        }

        [Test]
        public void VerifyConvertToGeneralList()
        {
            IList<AuthorizationEntity> inputList = new List<AuthorizationEntity>();
            inputList.Add(new AuthorizationEntity("KeyField", "NameField", "InfoField", "ValueField"));

            // output with null input
            IList<IAuthorizationEntity> resultList = AuthorizationEntityExtender.ConvertToBaseList<AuthorizationEntity>(null);
            resultList = resultList as List<IAuthorizationEntity>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);

            // normal input
            resultList = AuthorizationEntityExtender.ConvertToBaseList(inputList);
            Assert.IsNotNull(resultList);
            Assert.AreEqual(inputList.Count, resultList.Count);
        }

        [Test]
        public void VerifyConvertToSpecificList()
        {

            // output with null input
            IList<AuthorizationEntity> resultList = AuthorizationEntityExtender.ConvertToSpecificList<AuthorizationEntity>(null);
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);

            // output with foreign list
            IList<IAuthorizationEntity> inputGeneralList = new List<IAuthorizationEntity>(_foreignEntityList.OfType<IAuthorizationEntity>());
            resultList = AuthorizationEntityExtender.ConvertToSpecificList<AuthorizationEntity>(inputGeneralList);
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);

            // normal input
            inputGeneralList = new List<IAuthorizationEntity>(_inputList.OfType<IAuthorizationEntity>());
            resultList = AuthorizationEntityExtender.ConvertToSpecificList<AuthorizationEntity>(_inputList);
            Assert.IsNotNull(resultList);
            Assert.AreEqual(inputGeneralList.Count, resultList.Count);
        }

        [Test]
        public void VerifyIsAnyAuthorizationKeyEquals()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsAnyAuthorizationKeyEquals(_inputList, "KeyField"));
            Assert.IsFalse(AuthorizationEntityExtender.IsAnyAuthorizationKeyEquals(_inputList, "MyKeyField"));
        }

        [Test]
        public void VerifyIsAnyAuthorizationNameEquals()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsAnyAuthorizationNameEquals(_inputList, "NameField"));
            Assert.IsFalse(AuthorizationEntityExtender.IsAnyAuthorizationKeyEquals(_inputList, "MyNameField"));
        }

        [Test]
        public void VerifyIsAnyAuthorizationNameStartsWith()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsAnyAuthorizationNameStartsWith(_inputList, "Name"));
            Assert.IsFalse(AuthorizationEntityExtender.IsAnyAuthorizationNameStartsWith(_inputList, "MyName"));
        }

        [Test]
        public void VerifyIsAnyAuthorizationNameEndsWith()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsAnyAuthorizationNameEndsWith(_inputList, "Field"));
            Assert.IsFalse(AuthorizationEntityExtender.IsAnyAuthorizationNameEndsWith(_inputList, "MyField"));
        }

        [Test]
        public void VerifyIsAnyAuthorizationNameContains()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsAnyAuthorizationNameContains(_inputList, "Fie"));
            Assert.IsFalse(AuthorizationEntityExtender.IsAnyAuthorizationNameContains(_inputList, "MyFie"));
        }

        [Test]
        public void VerifyUnionTwoLists()
        {
            IList<IAuthorizationEntity> newList = AuthorizationObjectFactory.CreateAuthorizationEntityListWithThisFirstItem(_firstItem);
            IList<IAuthorizationEntity> resultList = AuthorizationEntityExtender.UnionTwoLists(_inputList, newList);
            Assert.AreEqual(3, resultList.Count);
        }

        [Test]
        public void VerifySubtractTwoLists()
        {
            IList<IAuthorizationEntity> newList = AuthorizationObjectFactory.CreateAuthorizationEntityListWithThisFirstItem(_firstItem);
            IList<IAuthorizationEntity> resultList = AuthorizationEntityExtender.SubtractTwoLists(_inputList, newList);
            Assert.AreEqual(1, resultList.Count);
        }

        [Test]
        public void VerifyIsEntityContainsItem()
        {
            Assert.IsTrue(AuthorizationEntityExtender.IsEntityListContainsItem<IAuthorizationEntity>(_inputList, _firstItem));
            Assert.IsFalse(AuthorizationEntityExtender.IsEntityListContainsItem<IAuthorizationEntity>(_inputList, new AuthorizationEntity("NotContainingKey", "NotContainingName", "NotContainingInfo", string.Empty)));

        }

        [Test]
        public void VerifyToString()
        {
            string expected = "'KeyField','AnotherKeyField'";
            string result = AuthorizationEntityExtender.ListToSqlString(_inputList);
            Assert.AreEqual(expected, result);

            expected = "''";
            result = AuthorizationEntityExtender.ListToSqlString(null);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyListToString()
        {
             string expected = "KeyField,AnotherKeyField";
            string result = AuthorizationEntityExtender.ListToString(_inputList, ",");
            Assert.AreEqual(expected, result);

            expected = string.Empty;
            result = AuthorizationEntityExtender.ListToString(null, ",");
            Assert.AreEqual(expected, result);
        }
    }
}
